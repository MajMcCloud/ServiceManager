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

	Steps:
	  1. ask for the version (or use -Version)
	  2. clear the output folders, msbuild /t:Restore + /t:Rebuild /p:Configuration=Release /p:SMVersion=<version>
	  3. assemble publish\ (UI output + extensions\<every provider>)
	  4. check the version of the finished parts
	  5. ZIP + RELEASE-NOTES.md
	  6. print the next steps (committing and tagging is up to the developer)

.PARAMETER Version
	Four part version, e.g. 2.1.0.0. Without the parameter the script asks for it.

.PARAMETER Persist
	Write the version into version.props as well, so Visual Studio and CI build the same number.

.PARAMETER SkipPdb
	Leave the .pdb files out of publish\ and the ZIP.

.PARAMETER SkipBuild
	Reuse the last release build instead of building it again.

.PARAMETER Strict
	Turn a deviating file version into an error. Meant for the CI, so a package never slips through
	with mixed version numbers.

.EXAMPLE
	.\publish.ps1
	.\publish.ps1 -Version 2.1.0.0 -Persist
	.\publish.ps1 -SkipPdb
	.\publish.ps1 -Version 2.1.0.1 -SkipBuild -Strict   # this is how the CI uses the script
#>
[CmdletBinding()]
param(
	[String]$Version,
	[Switch]$Persist,
	[Switch]$SkipPdb,
	[Switch]$SkipBuild,
	[Switch]$Strict
)

$ErrorActionPreference = 'Stop'

$Root      = $PSScriptRoot
$Solution  = Join-Path $Root 'ServiceManager.sln'
$Publish   = Join-Path $Root 'publish'
$Config    = 'Release'
$PropsFile = Join-Path $Root 'version.props'

$UiProject      = Join-Path $Root 'ServiceManagerUI'
$ServiceProject = Join-Path $Root 'ServiceManager.Service'
# Every folder under Extensions\ is a provider, none of them is entered by hand any more. The folder
# inside the package matches the project name - with one exception: TelegramNotifier has always been
# installed as TelegramBotNotifier, and that is where the settings.cfg of existing installations sits.
$FolderNames = @{ 'TelegramNotifier' = 'TelegramBotNotifier' }

$ExtensionProjects = Get-ChildItem (Join-Path $Root 'Extensions') -Directory |
	ForEach-Object {
		$csproj = Get-ChildItem $_.FullName -Filter '*.csproj' -File | Select-Object -First 1

		if (-not $csproj) { return }

		[PSCustomObject]@{
			Project = $_.FullName
			Release = Join-Path $_.FullName "bin\$Config"
			Folder  = if ($FolderNames.ContainsKey($_.Name)) { $FolderNames[$_.Name] } else { $_.Name }
			Dll     = $csproj.BaseName + '.dll'
		}
	}

if (-not $ExtensionProjects) { throw "No provider found under Extensions\." }

$UiRelease      = Join-Path $UiProject      "bin\$Config"
$ServiceRelease = Join-Path $ServiceProject "bin\$Config"


# ClickOnce leftovers and runtime debris have no place in a deployment.
$ExcludeDirs  = @('app.publish', 'logs')
$ExcludeFiles = @('ConfigBase.dll', 'ConfigBase.pdb', 'errors.txt')


# --------------------------------------------------------------------------------------
# Helpers
# --------------------------------------------------------------------------------------

function Write-Step([String]$text)
{
	Write-Host ''
	Write-Host "== $text" -ForegroundColor Cyan
}

function Find-MsBuild
{
	$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'

	if (Test-Path $vswhere)
	{
		$found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' |
				 Select-Object -First 1

		if ($found) { return $found }
	}

	foreach ($candidate in @('C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe',
							 'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe'))
	{
		if (Test-Path $candidate) { return $candidate }
	}

	throw 'MSBuild not found. Start the script from a Developer Command Prompt or install Visual Studio.'
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

Write-Host '  Note: the version is a build parameter (/p:SMVersion), not a file change per release.'

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
	# A rebuild does not delete foreign files: since 2021 bin\Release holds a ConfigBase.dll of the
	# former external project, which has nothing to do with the current one.
	Write-Step 'Clearing the output folders'

	foreach ($project in @($UiProject, $ServiceProject) + $ExtensionProjects.Project)
	{
		foreach ($sub in @('bin', 'obj'))
		{
			$path = Join-Path $project $sub

			if (Test-Path $path)
			{
				Remove-Item $path -Recurse -Force
				Write-Host "  removed: $($path.Substring($Root.Length + 1))"
			}
		}
	}

	$msbuild = Find-MsBuild

	Write-Step "Restore ($msbuild)"

	& $msbuild $Solution /t:Restore "/p:Configuration=$Config" "/p:SMVersion=$Version" /v:minimal /nologo

	if ($LASTEXITCODE -ne 0) { throw "Restore failed (exit $LASTEXITCODE)" }

	Write-Step "Rebuild $Version"

	& $msbuild $Solution /t:Rebuild "/p:Configuration=$Config" "/p:SMVersion=$Version" /v:minimal /nologo

	if ($LASTEXITCODE -ne 0) { throw "Rebuild failed (exit $LASTEXITCODE)" }
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
		($_.FullName -notmatch '\\(' + ($ExcludeDirs -join '|') + '\\)')
	} |
	ForEach-Object {
		$target = $_.FullName.Replace($UiRelease, $Publish)

		New-Item (Split-Path $target) -ItemType Directory -Force | Out-Null
		Copy-Item $_.FullName $target

		$copied++
	}

# Every provider belongs in <BaseDirectory>\extensions\<folder>, that is the only place the loader finds it.
foreach ($extension in $ExtensionProjects)
{
	$ExtTarget = Join-Path $Publish "extensions\$($extension.Folder)"

	New-Item $ExtTarget -ItemType Directory -Force | Out-Null

	Get-ChildItem $extension.Release -File |
		Where-Object { $_.Extension -ne '.pdb' -or -not $SkipPdb } |
		ForEach-Object { Copy-Item $_.FullName (Join-Path $ExtTarget $_.Name); $copied++ }

	Write-Host "  Provider: extensions\$($extension.Folder)"
}

Write-Host "  $copied files copied"

if (Test-Path $ServiceRelease)
{
	Write-Host "  The service output is already inside publish\ through the UI reference, own folder: $($ServiceRelease.Substring($Root.Length + 1))"
}


# --------------------------------------------------------------------------------------
# 4. Check the version of the parts
# --------------------------------------------------------------------------------------

Write-Step 'File versions inside the package'

$checked = @()
$mismatch = $false

$packageFiles = @('ServiceManager.UI.exe', 'ServiceManager.exe', 'ServiceManager.Extensions.dll',
					'ServiceManager.Base.dll', 'ServiceManager.ConfigBase.dll')

$packageFiles += $ExtensionProjects | ForEach-Object { "extensions\$($_.Folder)\$($_.Dll)" }

foreach ($file in $packageFiles)
{
	$path = Join-Path $Publish $file

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

if ($mismatch)
{
	if ($Strict) { throw "At least one part does not carry $Version - package discarded." }

	Write-Host "  warning: at least one file does not carry $Version." -ForegroundColor Yellow
}
else
{
	Write-Host "  every part carries $Version" -ForegroundColor Green
}


# --------------------------------------------------------------------------------------
# 5. Release notes + ZIP
# --------------------------------------------------------------------------------------

Write-Step 'Release notes'

$sha  = & git -C $Root rev-parse --short HEAD
$date = Get-Date -Format 'yyyy-MM-dd HH:mm'

$log = if ($lastTag) { & git -C $Root log "$lastTag..HEAD" --pretty=format:'- %s' } else { & git -C $Root log --pretty=format:'- %s' -20 }

$changes = if ($log) { $log } else { '- no changes since the last tag' }

$notes = @(
    "## ServiceManager $Version",
    '',
    "As of: $date - commit $sha - .NET Framework 4.8.1",
    '',
    '### Changes',
    '',
    $changes,
    '',
    '### Deployment',
    '',
    '1. Unpack the ZIP into a single folder, e.g. C:\Program Files\ServiceManager.',
    '2. Start ServiceManager.UI.exe as an administrator.',
    '3. Install the service: the UI dialog uses <start path>\ServiceManager.exe, both programs have to stay in the same folder.',
    '4. Every provider keeps its settings in extensions\<folder>\settings.cfg.',
    '',
    '### Tag',
    '',
    '    git tag v' + $Version,
    '    git push origin v' + $Version,
    ''
) -join "`r`n"

$notesFile = Join-Path $Publish 'RELEASE-NOTES.md'

[System.IO.File]::WriteAllText($notesFile, $notes, (New-Object System.Text.UTF8Encoding($true)))

Write-Host '  written: publish\RELEASE-NOTES.md'

$zip = Join-Path $Root "ServiceManager-$Version.zip"

if (Test-Path $zip) { Remove-Item $zip -Force }

Compress-Archive -Path (Join-Path $Publish '*') -DestinationPath $zip -CompressionLevel Optimal

$size = [Math]::Round((Get-Item $zip).Length / 1MB, 2)

Write-Host "  package: $([IO.Path]::GetFileName($zip)) ($size MB)"


# --------------------------------------------------------------------------------------
# 6. Next steps
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
	   Title:        ServiceManager $Version
	   Description:  the content of publish\RELEASE-NOTES.md
	   Asset:        ServiceManager-$Version.zip
"@

Write-Host ''
