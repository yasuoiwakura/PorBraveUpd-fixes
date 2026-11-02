[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = "dist",
    [string]$Version = "",
    [string]$SevenZipPath = "tools\7zr\7zr.exe"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $root

if (-not $Version) {
    $Version = (Get-Content -LiteralPath "Version.txt" -Raw).Trim()
}
if (-not $Version) {
    throw "Version.txt ist leer"
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw "vswhere.exe nicht gefunden: $vswhere"
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    throw "MSBuild nicht gefunden"
}

$sevenZip = Join-Path $root $SevenZipPath
if (-not (Test-Path -LiteralPath $sevenZip)) {
    throw "7zr.exe nicht gefunden: $sevenZip"
}

$output = Join-Path $root $OutputDirectory
$stage = Join-Path $output "staging"

if (Test-Path -LiteralPath $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null

Write-Host "== Build main =="
& $msbuild "Portable Brave Updater.sln" "/p:Configuration=$Configuration" "/p:Platform=Any CPU" /m /v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "Hauptprojekt-Build fehlgeschlagen"
}

Write-Host "== Build launchers =="
$launcherSolutions = Get-ChildItem -LiteralPath "Launcher" -Recurse -Filter *.sln
foreach ($solution in $launcherSolutions) {
    & $msbuild $solution.FullName "/p:Configuration=$Configuration" /m /v:minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Launcher-Build fehlgeschlagen: $($solution.FullName)"
    }
}

Write-Host "== Verify launcher versions =="
$launcherVersionFile = (Get-Content -LiteralPath "Launcher\Version.txt" -Raw).Trim()
$builtLauncher = Join-Path $root "Launcher\Brave Launcher\bin\$Configuration\Brave Launcher.exe"
if (-not (Test-Path -LiteralPath $builtLauncher)) {
    throw "Launcher-EXE fehlt: $builtLauncher"
}
$builtLauncherVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Resolve-Path -LiteralPath $builtLauncher).Path).FileVersion
if ($builtLauncherVersion -ne $launcherVersionFile) {
    throw "Launcher-Version $builtLauncherVersion != Launcher/Version.txt $launcherVersionFile"
}

$launcherCheck = Join-Path $output "launcher-check"
New-Item -ItemType Directory -Path $launcherCheck -Force | Out-Null
& $sevenZip e "Launcher\Launcher.7z" "-o$launcherCheck" "Brave Launcher.exe" -y | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Launcher.7z konnte nicht gelesen werden"
}
$archiveLauncher = Join-Path $launcherCheck "Brave Launcher.exe"
$archiveLauncherVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($archiveLauncher).FileVersion
if ($archiveLauncherVersion -ne $launcherVersionFile) {
    throw "Launcher.7z Version $archiveLauncherVersion != Launcher/Version.txt $launcherVersionFile"
}

Write-Host "== Stage =="
$mainExe = Join-Path $root "bin\$Configuration\Portable Brave Updater.exe"
$mainConfig = Join-Path $root "bin\$Configuration\Portable Brave Updater.exe.config"
if (-not (Test-Path -LiteralPath $mainExe)) {
    throw "Updater-EXE fehlt: $mainExe"
}
Copy-Item -LiteralPath $mainExe -Destination (Join-Path $stage "Portable Brave Updater.exe")
if (Test-Path -LiteralPath $mainConfig) {
    Copy-Item -LiteralPath $mainConfig -Destination (Join-Path $stage "Portable Brave Updater.exe.config")
}

$binDir = Join-Path $stage "Bin"
$launcherDir = Join-Path $binDir "Launcher"
New-Item -ItemType Directory -Path $launcherDir -Force | Out-Null
Copy-Item -LiteralPath $sevenZip -Destination (Join-Path $binDir "7zr.exe")

foreach ($projectDir in (Get-ChildItem -LiteralPath "Launcher" -Directory)) {
    $exe = Join-Path $projectDir.FullName "bin\$Configuration\$($projectDir.Name).exe"
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Launcher-EXE fehlt: $exe"
    }
    Copy-Item -LiteralPath $exe -Destination (Join-Path $launcherDir ($projectDir.Name + ".exe"))
}

foreach ($file in @("README.md", "LICENSE", "THIRD-PARTY-NOTICES.txt")) {
    if (Test-Path -LiteralPath $file) {
        Copy-Item -LiteralPath $file -Destination (Join-Path $stage $file)
    }
}

Write-Host "== Package =="
$archive = Join-Path $output "Portable.Brave.Updater.v$Version.7z"
Push-Location $stage
try {
    & $sevenZip a -t7z "$archive" * | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "7zr-Archivierung fehlgeschlagen"
    }
}
finally {
    Pop-Location
}

$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
"$hash  $(Split-Path -Leaf $archive)" | Set-Content -LiteralPath (Join-Path $output "SHA256SUMS.txt") -Encoding ASCII

Write-Host "Archive: $archive"
Write-Host "SHA256: $hash"
