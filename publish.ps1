<#
.SYNOPSIS
	Builds a release deployment folder: ask for the version, build the release, publish\ + ZIP + release notes.

.DESCRIPTION
	The version is a build parameter (see Directory.Build.targets / version.props in the root), so a
	release never touches a project file or a source file.

	The deployment folder is one single folder on purpose: frmMain.cs installs the Windows service with
	"/assemblypath=<Application.StartupPath>\ServiceManager.exe", so the service has to sit next to
	ServiceManager.UI.exe. The extension loader looks under <BaseDirectory>\extensions, which is why the
	output of every extension belongs in that same folder.

	The release is built into Temp\ rather than into the bin folders of the projects. Those folders hold
	the state Visual Studio works with, deleting them forces a full rebuild afterwards and fails on a
	locked DLL while the UI runs. Temp is created fresh, so it cannot hold foreign files either, and it
	is removed again once the ZIP is written.

	Two kinds of package come out of the script. The framework dependent one is small but needs the
	.NET 11 Desktop runtime and the ASP.NET Core runtime on the target machine. Every runtime identifier
	given with -Runtime adds a self contained package that carries its own runtime, so it runs on a
	machine without any .NET installation.

	Steps:
	  1. ask for the version (or use -Version)
	  2. dotnet restore + dotnet build -c Release -p:SMVersion=<version> -p:OutDir=Temp
	  3. assemble publish\ (UI output + extensions\<every provider>)
	  4. publish one self contained folder per runtime identifier
	  5. check the version of every package
	  6. RELEASE-NOTES.md + one ZIP per package
	  7. delete Temp
	  8. print the next steps (committing and tagging is up to the developer)

.PARAMETER Version
	Four part version, e.g. 2.1.0.0. Without the parameter the script asks for it.

.PARAMETER Persist
	Write the version into version.props as well, so Visual Studio and CI build the same number.

.PARAMETER Runtime
	One or more runtime identifiers, win-x64 by default. Every identifier adds a self contained package
	that carries its own runtime, so the target machine needs no .NET installation.

.PARAMETER SkipSelfContained
	Leave the self contained packages out and ship the framework dependent one only.

.PARAMETER SkipPdb
	Leave the .pdb files out of publish\ and the ZIPs.

.PARAMETER SkipBuild
	Reuse the last build from the bin folders of the projects instead of building into Temp.

.PARAMETER Strict
	Turn a deviating file version into an error. Meant for the CI, so a package never slips through
	with mixed version numbers.

.EXAMPLE
	.\publish.ps1
	.\publish.ps1 -Version 2.1.0.0 -Persist
	.\publish.ps1 -SkipPdb
	.\publish.ps1 -Runtime win-x64,win-arm64
	.\publish.ps1 -Version 2.1.0.1 -Runtime win-x64,win-arm64 -SkipBuild -Strict   # this is how the CI uses the script
#>
[CmdletBinding()]
param(
	[String]$Version,
	[String[]]$Runtime = @('win-x64'),
	[Switch]$Persist,
	[Switch]$SkipPdb,
	[Switch]$SkipBuild,
	[Switch]$SkipSelfContained,
	[Switch]$Strict
)

$ErrorActionPreference = 'Stop'

$Root      = $PSScriptRoot
$Solution  = Join-Path $Root 'ServiceManager.sln'
$Publish   = Join-Path $Root 'publish'
$Temp      = Join-Path $Root 'Temp'
$Config    = 'Release'
$PropsFile = Join-Path $Root 'version.props'

# The CI hands the identifiers over in one environment variable, so a single string with commas
# has to become the list the rest of the script works with.
$Runtime = @($Runtime | ForEach-Object { $_ -split '[,\s]+' } | Where-Object { $_ })

if (-not $Runtime) { $Runtime = @('win-x64') }

$UiProject      = Join-Path $Root 'ServiceManager.UI'
$ServiceProject = Join-Path $Root 'ServiceManager.Service'
# Every folder under Extensions\ is a provider, none of them is entered by hand any more. The folder
# inside the package matches the project name - with one exception: TelegramNotifier has always been
# installed as TelegramBotNotifier, and that is where the settings.cfg of existing installations sits.
$FolderNames = @{ 'TelegramNotifier' = 'TelegramBotNotifier' }

# Without -SkipBuild the release is built into Temp\<project name>, with it the last build in the bin
# folders is reused. Both layouts are settled here, the rest of the script only knows the folders it
# has to read from.
$BuildRoot = if ($SkipBuild) { $null } else { $Temp }

$ExtensionProjects = Get-ChildItem (Join-Path $Root 'Extensions') -Directory |
	ForEach-Object {
		$csproj = Get-ChildItem $_.FullName -Filter '*.csproj' -File | Select-Object -First 1

		if (-not $csproj) { return }

		[PSCustomObject]@{
			Project = $_.FullName
			Release = if ($BuildRoot) { Join-Path $BuildRoot $csproj.BaseName } else { Join-Path $_.FullName "bin\$Config" }
			Folder  = if ($FolderNames.ContainsKey($_.Name)) { $FolderNames[$_.Name] } else { $_.Name }
			Dll     = $csproj.BaseName + '.dll'
		}
	}

if (-not $ExtensionProjects) { throw "No provider found under Extensions\." }

$UiRelease      = if ($BuildRoot) { Join-Path $BuildRoot 'ServiceManager.UI' }      else { Join-Path $UiProject      "bin\$Config" }
$ServiceRelease = if ($BuildRoot) { Join-Path $BuildRoot 'ServiceManager.Service' } else { Join-Path $ServiceProject "bin\$Config" }


# ClickOnce leftovers and runtime debris have no place in a deployment. The extensions get
# their folder through their own loop, and a RID publish leaves bin\<Config>\<rid> behind,
# which belongs to the self contained package only.
$ExcludeDirs  = @('app.publish', 'logs', 'extensions')
$ExcludeFiles = @('ConfigBase.dll', 'ConfigBase.pdb', 'errors.txt')
$ExcludeRids  = '(win|linux|osx|browser)-[a-z0-9-]+'


# --------------------------------------------------------------------------------------
# Helpers
# --------------------------------------------------------------------------------------

function Write-Step([String]$text)
{
	Write-Host ''
	Write-Host "== $text" -ForegroundColor Cyan
}

# The loader only looks under <BaseDirectory>\extensions\<folder>, so every package needs the providers.
function Copy-Extensions([String]$Target)
{
	$count = 0

	foreach ($extension in $ExtensionProjects)
	{
		$folder = Join-Path $Target "extensions\$($extension.Folder)"

		New-Item $folder -ItemType Directory -Force | Out-Null

		Get-ChildItem $extension.Release -File |
			Where-Object { $_.Extension -ne '.pdb' -or -not $SkipPdb } |
			ForEach-Object { Copy-Item $_.FullName (Join-Path $folder $_.Name); $count++ }

		Write-Host "  Provider: extensions\$($extension.Folder)"
	}

	return $count
}

# Returns $true as soon as a part of the package does not carry the version of the release.
function Test-PackageVersions([String]$Folder)
{
	$checked  = @()
	$mismatch = $false

	$packageFiles = @('ServiceManager.UI.exe', 'ServiceManager.exe', 'ServiceManager.Extensions.dll',
						'ServiceManager.Base.dll', 'ServiceManager.ConfigBase.dll')

	$packageFiles += $ExtensionProjects | ForEach-Object { "extensions\$($_.Folder)\$($_.Dll)" }

	foreach ($file in $packageFiles)
	{
		$path = Join-Path $Folder $file

		if (-not (Test-Path $path))
		{
			Write-Host "  missing in the package: $file" -ForegroundColor Red
			$mismatch = $true
			continue
		}

		$info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path)

		if ($info.FileVersion -ne $Version) { $mismatch = $true }

		$checked += [PSCustomObject]@{
			Name    = Split-Path $file -Leaf
			Version = $info.FileVersion
		}
	}

	$checked | Format-Table -AutoSize | Out-String | Write-Host

	return $mismatch
}

function Get-DefaultVersion
{
	if (Test-Path $PropsFile)
	{
		$match = [RegEx]::Match([System.IO.File]::ReadAllText($PropsFile), '<SMVersion>([^<]*)</SMVersion>')

		if ($match.Success -and $match.Groups[1].Value -match '^\d+\.\d+\.\d+\.\d+$')
		{
			return $match.Groups[1].Value
		}
	}

	return '1.0.0.0'
}

function Test-VersionFormat([String]$value)
{
	if ($value -notmatch '^\d+\.\d+\.\d+\.\d+$')
	{
		Write-Host "Expected four numbers, e.g. 2.1.0.0 (entered: `"$value`")" -ForegroundColor Red
		return $false
	}

	$bad = $value.Split('.') | Where-Object { [Int64]$_ -gt 65535 }

	if ($bad)
	{
		# AssemblyVersion is limited to Int16 per part, above that the CLR refuses the assembly.
		Write-Host "AssemblyVersion may hold max. 65535 per part: $value" -ForegroundColor Red
		return $false
	}

	return $true
}

function Get-LastTag
{
	$tag = & git -C $Root describe --tags --abbrev=0 2>$null

	if ($LASTEXITCODE -ne 0) { return $null }

	return $tag
}

# The notes are read by whoever installs the package, so the commits are not simply listed one after
# the other: the first word of the subject says whether the commit brings something new, repairs
# something or changes what is already there.
function Split-ChangeLog
{
	param([String[]]$Subjects)

	$groups = [Ordered]@{
		'New features'              = New-Object System.Collections.Generic.List[String]
		'Changes'                   = New-Object System.Collections.Generic.List[String]
		'Bug fixes and corrections' = New-Object System.Collections.Generic.List[String]
	}

	foreach ($subject in $Subjects)
	{
		# A repaired bug stays a bug fix even if the subject also mentions a test.
		if ($subject -match '^(Fix|Correct|Restore)\b')
		{
			$groups['Bug fixes and corrections'].Add($subject)
		}
		# Work on tests, CI, docs or screenshots brings nothing new to whoever installs the
		# package, so it goes under Changes even when the subject starts with "Add".
		elseif ($subject -match '\b(tests?|CI|documentation|screenshots?|readme)\b')
		{
			$groups['Changes'].Add($subject)
		}
		elseif ($subject -match '^(Add|Support|Introduce)\b')
		{
			$groups['New features'].Add($subject)
		}
		else
		{
			$groups['Changes'].Add($subject)
		}
	}

	return $groups
}

# --------------------------------------------------------------------------------------
# 1. Version
# --------------------------------------------------------------------------------------

Write-Host 'ServiceManager Publish' -ForegroundColor Green
Write-Host "Project folder: $Root"

$default = Get-DefaultVersion
$lastTag = Get-LastTag

Write-Step 'Version sources'
Write-Host "  version.props:`t$default"

if ($lastTag) { Write-Host "  last tag:`t$lastTag" }

Write-Host '  Note: the version is a build parameter (-p:SMVersion), not a file change per release.'

if (-not $Version)
{
	$v = [Version]$default
	$suggested = "$($v.Major).$($v.Minor).$($v.Build).$($v.Revision + 1)"

	Write-Host ''
	$Version = (Read-Host "New version (4 parts, default $suggested)").Trim()

	if ([String]::IsNullOrWhiteSpace($Version)) { $Version = $suggested }
}

if (-not (Test-VersionFormat $Version)) { exit 1 }

Write-Host "`nNew version: $Version" -ForegroundColor Green

if ($Persist)
{
	$text = [System.IO.File]::ReadAllText($PropsFile) -replace '<SMVersion>[^<]*</SMVersion>', "<SMVersion>$Version</SMVersion>"

	[System.IO.File]::WriteAllText($PropsFile, $text, (New-Object System.Text.UTF8Encoding($true)))

	Write-Host "  version.props set to $Version"
}


# --------------------------------------------------------------------------------------
# 2. Build
# --------------------------------------------------------------------------------------

if ($SkipBuild)
{
	Write-Step 'Build skipped'
}
else
{
	# Temp is created fresh, so it cannot hold foreign files. Those used to be removed with the folders:
	# since 2021 bin\Release holds a ConfigBase.dll of the former external project, which has nothing
	# to do with the current one.
	Write-Step 'Preparing Temp'

	if (Test-Path $Temp) { Remove-Item $Temp -Recurse -Force }

	New-Item $Temp -ItemType Directory | Out-Null

	Write-Step 'Restore'

	& dotnet restore $Solution "/p:SMVersion=$Version" -v minimal --nologo

	if ($LASTEXITCODE -ne 0) { throw "Restore failed (exit $LASTEXITCODE)" }

	Write-Step "Build $Version into $($Temp.Substring($Root.Length + 1))"

	# OutDir is a global property, so it wins over the OutputPath of every project, and
	# GenerateProjectSpecificOutputFolder turns it into one folder per project below Temp.
	& dotnet build $Solution -c $Config --no-restore "/p:SMVersion=$Version" "/p:OutDir=$Temp" -p:GenerateProjectSpecificOutputFolder=true -v minimal --nologo

	if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }
}

foreach ($path in @($UiRelease) + $ExtensionProjects.Release)
{
	if (-not (Test-Path $path)) { throw "Expected output is missing: $path" }
}


# --------------------------------------------------------------------------------------
# 3. Assemble publish\
# --------------------------------------------------------------------------------------

Write-Step 'Assembling the publish folder'

if (Test-Path $Publish) { Remove-Item $Publish -Recurse -Force }

New-Item $Publish -ItemType Directory | Out-Null

# The UI output already holds ServiceManager.exe and every referenced library.
$copied = 0

Get-ChildItem $UiRelease -Recurse -File |
	Where-Object {
		($_.Extension -ne '.pdb' -or -not $SkipPdb) -and
		($_.Name -notin $ExcludeFiles) -and
		($_.Name -notlike '*.application') -and
		($_.Name -notlike '*.manifest') -and
		($_.FullName -notmatch '\\(' + ($ExcludeDirs -join '|') + '\\)') -and
		($_.FullName -notmatch '\\(' + $ExcludeRids + '\\)')
	} |
	ForEach-Object {
		$target = $_.FullName.Replace($UiRelease, $Publish)

		New-Item (Split-Path $target) -ItemType Directory -Force | Out-Null
		Copy-Item $_.FullName $target

		$copied++
	}

$copied += Copy-Extensions $Publish

Write-Host "  $copied files copied"

if (Test-Path $ServiceRelease)
{
	Write-Host "  The service output is already inside publish\ through the UI reference, own folder: $($ServiceRelease.Substring($Root.Length + 1))"
}

$Packages = @([PSCustomObject]@{ Label = 'framework dependent'; Rid = $null; Folder = $Publish })


# --------------------------------------------------------------------------------------
# 4. Self contained packages
# --------------------------------------------------------------------------------------

if ($SkipSelfContained)
{
	Write-Step 'Self contained packages skipped'
}
else
{
	Write-Step "Publishing self contained ($($Runtime -join ', '))"

	$UiProjectFile = Join-Path $UiProject 'ServiceManager.UI.csproj'

	foreach ($rid in $Runtime)
	{
		$selfOut = Join-Path $Temp "self\$rid"

		if (Test-Path $selfOut) { Remove-Item $selfOut -Recurse -Force }

		# A RID publish of the UI project pulls the service in through the project reference and adds
		# the whole runtime, so the folder runs on a machine without any .NET installation.
		& dotnet publish $UiProjectFile -c $Config -r $rid --self-contained "/p:SMVersion=$Version" -o $selfOut -v minimal --nologo

		if ($LASTEXITCODE -ne 0) { throw "Self contained publish for $rid failed (exit $LASTEXITCODE)" }

		# The providers sit outside the reference tree of the UI, the loader only finds them under extensions\.
		$null = Copy-Extensions $selfOut

		if ($SkipPdb)
		{
			Get-ChildItem $selfOut -Recurse -Filter '*.pdb' -File | Remove-Item -Force
		}

		$Packages += [PSCustomObject]@{ Label = $rid; Rid = $rid; Folder = $selfOut }
	}
}


# --------------------------------------------------------------------------------------
# 5. Check the version of the parts
# --------------------------------------------------------------------------------------

Write-Step 'File versions inside the packages'

$mismatchPackages = @()

foreach ($package in $Packages)
{
	Write-Host "  package: $($package.Label)" -ForegroundColor White

	if (Test-PackageVersions $package.Folder) { $mismatchPackages += $package.Label }
}

if ($mismatchPackages)
{
	if ($Strict) { throw "At least one part does not carry $Version - package discarded." }

	Write-Host "  warning: a file does not carry $Version in: $($mismatchPackages -join ', ')" -ForegroundColor Yellow
}
else
{
	Write-Host "  every part of every package carries $Version" -ForegroundColor Green
}


# --------------------------------------------------------------------------------------
# 6. Release notes and one ZIP per package
# --------------------------------------------------------------------------------------

Write-Step 'Release notes'

$sha  = & git -C $Root rev-parse --short HEAD
$date = Get-Date -Format 'yyyy-MM-dd HH:mm'

$subjects = @(if ($lastTag) { & git -C $Root log "$lastTag..HEAD" --pretty=format:'%s' } else { & git -C $Root log --pretty=format:'%s' -20 })

$groups = Split-ChangeLog $subjects

$notes = @(
    "## ServiceManager $Version",
    '',
    "As of: $date - commit $sha - .NET 11",
    ''
)

foreach ($group in $groups.GetEnumerator())
{
    if ($group.Value.Count -eq 0) { continue }

    $notes += @("### $($group.Key)", '')
    $notes += ($group.Value | ForEach-Object { "- $_" })
    $notes += ''
}

if ($subjects.Count -eq 0)
{
    $notes += @('- no changes since the last tag', '')
}

$notes += @(
    '### Deployment',
    '',
    '1. Pick a package: the one without a runtime identifier needs the .NET 11 Desktop runtime and the ASP.NET Core runtime, the ones with a runtime identifier carry their own runtime and need no installation.',
    '2. Unpack the ZIP into a single folder, e.g. C:\Program Files\ServiceManager.',
    '3. Start ServiceManager.UI.exe as an administrator.',
    '4. Install the service: the UI dialog uses <start path>\ServiceManager.exe, both programs have to stay in the same folder.',
    '5. Every provider keeps its settings in extensions\<folder>\settings.cfg.',
    '',
    '### Tag',
    '',
    "    git tag v$Version",
    "    git push origin v$Version",
    ''
)

$notes = $notes -join "`r`n"

$notesFile = Join-Path $Publish 'RELEASE-NOTES.md'

[System.IO.File]::WriteAllText($notesFile, $notes, (New-Object System.Text.UTF8Encoding($true)))

Write-Host '  written: publish\RELEASE-NOTES.md'

$zipNames = @()

foreach ($package in $Packages)
{
	$name = if ($package.Rid) { "ServiceManager-$Version-$($package.Rid).zip" } else { "ServiceManager-$Version.zip" }
	$zip  = Join-Path $Root $name

	if (Test-Path $zip) { Remove-Item $zip -Force }

	Compress-Archive -Path (Join-Path $package.Folder '*') -DestinationPath $zip -CompressionLevel Optimal

	$size = [Math]::Round((Get-Item $zip).Length / 1MB, 2)

	Write-Host "  package: $name ($size MB)"

	$zipNames += $name
}


# --------------------------------------------------------------------------------------
# 7. Remove Temp
# --------------------------------------------------------------------------------------

Write-Step 'Cleaning up'

if (Test-Path $Temp)
{
	Remove-Item $Temp -Recurse -Force
	Write-Host "  removed: $($Temp.Substring($Root.Length + 1))"
}


# --------------------------------------------------------------------------------------
# 8. Next steps
# --------------------------------------------------------------------------------------

Write-Step 'Next steps'

$persistHint = if ($Persist) { '  2. commit version.props: git add version.props' } else { '  2. (version.props was not changed - that only happens with -Persist)' }

Write-Host @"
  1. Tag and push:
	   git tag v$Version
	   git push origin main v$Version
$persistHint
  3. Create the GitHub release: https://github.com/MajMcCloud/ServiceManager/releases/new
	   Tag:          v$Version
	   Title:        what the release brings, e.g. "Multiple notification providers"
	   Description:  the content of publish\RELEASE-NOTES.md
	   Assets:       $($zipNames -join ', ')
"@

Write-Host ''
