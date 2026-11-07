<#
.SYNOPSIS
	Baut eine Release-Deployment-Mappe: Version abfragen, Release bauen, publish\ + ZIP + Release-Notizen.

.DESCRIPTION
	Die Version ist ein Build-Parameter (siehe Directory.Build.targets / version.props im Root), es wird
	pro Release also keine Projekt- oder Quelldatei angefasst.

	Der Deployment-Ordner ist bewusst ein einzelner Ordner: frmMain.cs installiert den Windows-Dienst mit
	"/assemblypath=<Application.StartupPath>\ServiceManager.exe", der Dienst muss also neben
	ServiceManager.UI.exe liegen. Der Extension-Loader sucht unter <BaseDirectory>\extensions, deshalb
	gehoert auch die Ausgabe jeder Extension in denselben Ordner.

	Schritte:
	  1. Version abfragen (oder -Version verwenden)
	  2. Ausgabemappen loeschen, msbuild /t:Restore + /t:Rebuild /p:Configuration=Release /p:SMVersion=<version>
	  3. publish\ zusammenbauen (UI-Ausgabe + extensions\<jeder Provider>)
	  4. Version der fertigen Bausteine nachpruefen
	  5. ZIP + RELEASE-NOTES.md
	  6. naechste Schritte ausgeben (Committen und Taggen uebernimmt der Entwickler)

.PARAMETER Version
	Vierstellige Version, z. B. 2.1.0.0. Ohne den Parameter fragt das Skript.

.PARAMETER Persist
	Die Version zusaetzlich in version.props schreiben, damit Visual Studio und CI dieselbe Nummer bauen.

.PARAMETER SkipPdb
	.pdb-Dateien aus publish\ und dem ZIP weglassen.

.PARAMETER SkipBuild
	Den letzten Release-Build wiederverwenden statt neu zu bauen.

.PARAMETER Strict
	Aus einer abweichenden Dateiversion einen Fehler machen. Fuer die CI, damit ein Paket nie mit
	vermischten Versionsnummern durchrauscht.

.EXAMPLE
	.\publish.ps1
	.\publish.ps1 -Version 2.1.0.0 -Persist
	.\publish.ps1 -SkipPdb
	.\publish.ps1 -Version 2.1.0.1 -SkipBuild -Strict   # so nutzt die CI das Skript
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
# Jeder Ordner unter Extensions\ ist ein Provider, keiner wird mehr von Hand eingetragen. Der Ordner
# im Paket entspricht dem Projektnamen - mit einer Ausnahme: TelegramNotifier war immer als
# TelegramBotNotifier installiert, und genau dort liegt die settings.cfg bestehender Installationen.
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

if (-not $ExtensionProjects) { throw "Kein Provider unter Extensions\ gefunden." }

$UiRelease      = Join-Path $UiProject      "bin\$Config"
$ServiceRelease = Join-Path $ServiceProject "bin\$Config"


# ClickOnce-Reste und Laufzeitabfall haben in einem Deployment nichts verloren.
$ExcludeDirs  = @('app.publish', 'logs')
$ExcludeFiles = @('ConfigBase.dll', 'ConfigBase.pdb', 'errors.txt')


# --------------------------------------------------------------------------------------
# Helfer
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

	throw 'MSBuild nicht gefunden. Das Skript aus einer Developer Command Prompt starten oder Visual Studio installieren.'
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
		Write-Host "Erwartet vier Zahlen, z. B. 2.1.0.0 (eingegeben: `"$value`")" -ForegroundColor Red
		return $false
	}

	$bad = $value.Split('.') | Where-Object { [Int64]$_ -gt 65535 }

	if ($bad)
	{
		# AssemblyVersion ist pro Stelle auf Int16 begrenzt, darueber lehnt die CLR das Assembly ab.
		Write-Host "AssemblyVersion darf pro Stelle max. 65535 sein: $value" -ForegroundColor Red
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
Write-Host "Projektmappe: $Root"

$default = Get-DefaultVersion
$lastTag = Get-LastTag

Write-Step 'Versionsquellen'
Write-Host "  version.props:`t$default"

if ($lastTag) { Write-Host "  letzter Tag:`t$lastTag" }

Write-Host '  Hinweis: Die Version ist ein Build-Parameter (/p:SMVersion), keine Dateianderung pro Release.'

if (-not $Version)
{
	$v = [Version]$default
	$suggested = "$($v.Major).$($v.Minor).$($v.Build).$($v.Revision + 1)"

	Write-Host ''
	$Version = (Read-Host "Neue Version (4-stellig, Standard $suggested)").Trim()

	if ([String]::IsNullOrWhiteSpace($Version)) { $Version = $suggested }
}

if (-not (Test-VersionFormat $Version)) { exit 1 }

Write-Host "`nNeue Version: $Version" -ForegroundColor Green

if ($Persist)
{
	$text = [System.IO.File]::ReadAllText($PropsFile) -replace '<SMVersion>[^<]*</SMVersion>', "<SMVersion>$Version</SMVersion>"

	[System.IO.File]::WriteAllText($PropsFile, $text, (New-Object System.Text.UTF8Encoding($true)))

	Write-Host "  version.props auf $Version gesetzt"
}


# --------------------------------------------------------------------------------------
# 2. Build
# --------------------------------------------------------------------------------------

if ($SkipBuild)
{
	Write-Step 'Build uebersprungen'
}
else
{
	# Ein Rebuild loescht keine Fremddateien: in bin\Release liegt seit 2021 ein ConfigBase.dll des
	# ehemaligen externen Projekts, das mit dem jetzigen nichts mehr zu tun hat.
	Write-Step 'Ausgabemappen saubermachen'

	foreach ($project in @($UiProject, $ServiceProject) + $ExtensionProjects.Project)
	{
		foreach ($sub in @('bin', 'obj'))
		{
			$path = Join-Path $project $sub

			if (Test-Path $path)
			{
				Remove-Item $path -Recurse -Force
				Write-Host "  entfernt: $($path.Substring($Root.Length + 1))"
			}
		}
	}

	$msbuild = Find-MsBuild

	Write-Step "Restore ($msbuild)"

	& $msbuild $Solution /t:Restore "/p:Configuration=$Config" "/p:SMVersion=$Version" /v:minimal /nologo

	if ($LASTEXITCODE -ne 0) { throw "Restore fehlgeschlagen (Exit $LASTEXITCODE)" }

	Write-Step "Rebuild $Version"

	& $msbuild $Solution /t:Rebuild "/p:Configuration=$Config" "/p:SMVersion=$Version" /v:minimal /nologo

	if ($LASTEXITCODE -ne 0) { throw "Rebuild fehlgeschlagen (Exit $LASTEXITCODE)" }
}

foreach ($path in @($UiRelease) + $ExtensionProjects.Release)
{
	if (-not (Test-Path $path)) { throw "Erwartete Ausgabe fehlt: $path" }
}


# --------------------------------------------------------------------------------------
# 3. publish\ zusammenbauen
# --------------------------------------------------------------------------------------

Write-Step 'publish\ zusammenbauen'

if (Test-Path $Publish) { Remove-Item $Publish -Recurse -Force }

New-Item $Publish -ItemType Directory | Out-Null

# Die UI-Ausgabe enthaelt schon ServiceManager.exe und alle referenzierten Bibliotheken.
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

# Jeder Provider gehoert nach <BaseDirectory>\extensions\<Ordner>, nur dort findet der Loader ihn.
foreach ($extension in $ExtensionProjects)
{
	$ExtTarget = Join-Path $Publish "extensions\$($extension.Folder)"

	New-Item $ExtTarget -ItemType Directory -Force | Out-Null

	Get-ChildItem $extension.Release -File |
		Where-Object { $_.Extension -ne '.pdb' -or -not $SkipPdb } |
		ForEach-Object { Copy-Item $_.FullName (Join-Path $ExtTarget $_.Name); $copied++ }

	Write-Host "  Provider: extensions\$($extension.Folder)"
}

Write-Host "  $copied Dateien uebernommen"

if (Test-Path $ServiceRelease)
{
	Write-Host "  Service-Ausgabe liegt ueber die UI-Referenz bereits in publish\, eigenes Verzeichnis: $($ServiceRelease.Substring($Root.Length + 1))"
}


# --------------------------------------------------------------------------------------
# 4. Version der Bausteine nachpruefen
# --------------------------------------------------------------------------------------

Write-Step 'Dateiversionen im Paket'

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
		Write-Host "  fehlt im Paket: $file" -ForegroundColor Red
		$mismatch = $true
		continue
	}

	$info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path)

	if ($info.FileVersion -ne $Version) { $mismatch = $true }

	$checked += [PSCustomObject]@{
		Datei   = Split-Path $file -Leaf
		Version = $info.FileVersion
	}
}

$checked | Format-Table -AutoSize | Out-String | Write-Host

if ($mismatch)
{
	if ($Strict) { throw "Mindestens eine Baustein traegt nicht $Version - Paket verworfen." }

	Write-Host "  Warnung: mindestens eine Datei traegt nicht $Version." -ForegroundColor Yellow
}
else
{
	Write-Host "  alle Bausteine tragen $Version" -ForegroundColor Green
}


# --------------------------------------------------------------------------------------
# 5. Release-Notizen + ZIP
# --------------------------------------------------------------------------------------

Write-Step 'Release-Notizen'

$sha  = & git -C $Root rev-parse --short HEAD
$date = Get-Date -Format 'dd.MM.yyyy HH:mm'

$log = if ($lastTag) { & git -C $Root log "$lastTag..HEAD" --pretty=format:'- %s' } else { & git -C $Root log --pretty=format:'- %s' -20 }

$changes = if ($log) { $log } else { '- keine Aenderungen seit dem letzten Tag' }

$notes = @(
    "## ServiceManager $Version",
    '',
    "Stand: $date - Commit $sha - .NET Framework 4.8.1",
    '',
    '### Aenderungen',
    '',
    $changes,
    '',
    '### Deployment',
    '',
    '1. ZIP in einen Ordner entpacken, z. B. C:\Program Files\ServiceManager.',
    '2. ServiceManager.UI.exe als Administrator starten.',
    '3. Dienst installieren: der UI-Dialog nutzt <Startpfad>\ServiceManager.exe, beide Programme muessen im selben Ordner bleiben.',
    '4. Extension-Einstellungen liegen pro Provider unter extensions\<Ordner>\settings.cfg.',
    '',
    '### Tag',
    '',
    '    git tag v' + $Version,
    '    git push origin v' + $Version,
    ''
) -join "`r`n"

$notesFile = Join-Path $Publish 'RELEASE-NOTES.md'

[System.IO.File]::WriteAllText($notesFile, $notes, (New-Object System.Text.UTF8Encoding($true)))

Write-Host '  geschrieben: publish\RELEASE-NOTES.md'

$zip = Join-Path $Root "ServiceManager-$Version.zip"

if (Test-Path $zip) { Remove-Item $zip -Force }

Compress-Archive -Path (Join-Path $Publish '*') -DestinationPath $zip -CompressionLevel Optimal

$size = [Math]::Round((Get-Item $zip).Length / 1MB, 2)

Write-Host "  Paket: $([IO.Path]::GetFileName($zip)) ($size MB)"


# --------------------------------------------------------------------------------------
# 6. naechste Schritte
# --------------------------------------------------------------------------------------

Write-Step 'Als Naechstes'

$persistHint = if ($Persist) { '  2. version.props committen: git add version.props' } else { '  2. (version.props wurde nicht geaendert - das passiert nur mit -Persist)' }

Write-Host @"
  1. Taggen und pushen:
	   git tag v$Version
	   git push origin main v$Version
$persistHint
  3. GitHub Release anlegen: https://github.com/MajMcCloud/ServiceManager/releases/new
	   Tag:          v$Version
	   Titel:        ServiceManager $Version
	   Beschreibung: Inhalt von publish\RELEASE-NOTES.md
	   Asset:        ServiceManager-$Version.zip
"@

Write-Host ''
