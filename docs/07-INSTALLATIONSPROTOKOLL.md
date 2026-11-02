# 07 - Installationsprotokoll Kompilierungssetup

Stand: 2026-11-02  
Host: Windows 11 x64  
Shell: erhoehte PowerShell

## 1. Ausgangslage

Visual Studio Build Tools 2022 war bereits installiert, aber ohne die fuer dieses Projekt benoetigten .NET-Komponenten.

| Merkmal | Wert |
|---|---|
| Produkt | Visual Studio Build Tools 2022 |
| InstallationPath | `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools` |
| InstallationVersion | `17.14.36915.13` |
| Produktversion | 17.14.25 (January 2026) |
| MSBuild | `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe` |
| MSBuild-Version | `17.14.40.60911` |

Fehlende Komponenten vor der Aenderung:

- `Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools`
- `Microsoft.Net.Component.4.8.TargetingPack`
- `Microsoft.Net.Component.4.8.SDK`
- `Microsoft.VisualStudio.Component.NuGet.BuildTools`

Bereits vorhanden:

- `Microsoft.Component.MSBuild`
- `Microsoft.VisualStudio.Component.Roslyn.Compiler`
- `Microsoft.VisualStudio.Component.Windows11SDK.26100`

## 2. winget-Befund

`winget` ist installiert (`v1.29.380`), war aber in der erhoehten PowerShell nicht im `PATH`. Der App-Execution-Alias liegt unter:

```text
C:\Users\<Benutzer>\AppData\Local\Microsoft\WindowsApps\winget.exe
```

Direkter Aufruf funktioniert:

```powershell
& "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe" --version
```

`winget` war fuer die Einrichtung nicht notwendig, weil die Build Tools bereits installiert waren. Die Installation wurde ueber den Visual Studio Installer modifiziert.

## 3. Durchgefuehrte Aenderung

Bestehende Build-Tools-Installation modifiziert, keine Neuinstallation:

```powershell
$installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
$installPath = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"

& $installer modify `
  --installPath $installPath `
  --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools `
  --add Microsoft.Net.Component.4.8.TargetingPack `
  --passive --norestart
```

Bewusst nicht verwendet:

- `--includeRecommended`: haette unnoetig .NET 8/9 SDK/Runtime mitinstalliert.
- `--includeOptional`.
- Visual Studio 2019.
- .NET Framework 4.0 Targeting Pack.

Die Workload bringt die benoetigten Pflichtkomponenten mit, insbesondere:

- .NET Framework 4.8 SDK
- MSBuild
- Roslyn-Compiler
- NuGet Build Tools
- .NET Framework 4.7.2 Targeting Pack (Workload-Pflicht)

Zusaetzlich explizit ergaenzt:

- .NET Framework 4.8 Targeting Pack

## 4. Verifikation

### vswhere

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * `
  -requires Microsoft.Net.Component.4.8.TargetingPack `
  -property installationPath
```

Ergebnis:

```text
C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools
```

### Referenzassemblies

```powershell
Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
```

Ergebnis: `True`

### TlbImp

```powershell
Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs\Windows" -Recurse -Filter TlbImp.exe
```

Ergebnis:

```text
C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\TlbImp.exe
C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64\TlbImp.exe
```

### Komponentenstatus

| Komponente | Status |
|---|---|
| `Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools` | installiert |
| `Microsoft.Net.Component.4.8.TargetingPack` | installiert |
| `Microsoft.Net.Component.4.8.SDK` | installiert |
| `Microsoft.VisualStudio.Component.NuGet.BuildTools` | installiert |
| `Microsoft.VisualStudio.Component.Windows11SDK.26100` | installiert |
| `Microsoft.Component.MSBuild` | installiert |
| `Microsoft.VisualStudio.Component.Roslyn.Compiler` | installiert |
| `v4.8`-Referenzassemblies | vorhanden |
| `TlbImp.exe` x86/x64 | vorhanden |

## 5. Bewertung der SDK-Sicherheit

- Ziel ist **.NET Framework 4.8**. Es ist aktiv unterstuetzt und Bestandteil von Windows 11.
- `TlbImp.exe` traegt ein Dateidatum aus 2019. Das ist normal: Das .NET Framework 4.8 SDK wurde seitdem nicht neu veroeffentlicht. Das Werkzeug ist Teil des unterstuetzten 4.8-SDK, nicht des toten 4.0-Stacks.
- **.NET Framework 4.0** ist seit 2016 out of support. Es wird nicht installiert und die Launcher werden auf 4.8 retargetet.
- **.NET 8/9 SDK** wurde nicht installiert. Es ist fuer dieses Projekt nicht erforderlich.
- **Visual Studio 2019** wurde nicht installiert.
- **Windows 11 SDK 26100** ist vorhanden; der relevante `tlbimp.exe` kommt aus dem .NET Framework 4.8 SDK.

## 6. Systemaenderungen

| Aenderung | Details |
|---|---|
| Workload hinzugefuegt | .NET Desktop Build Tools |
| Komponente hinzugefuegt | .NET Framework 4.8 Targeting Pack |
| Komponente mitinstalliert | .NET Framework 4.8 SDK |
| Komponente mitinstalliert | NuGet Build Tools |
| Keine Neuinstallation | bestehende Build-Tools-Instanz modifiziert |
| Kein Reboot | `--norestart` |

## 7. Stand nach Build

Erledigt am 2026-11-02:

1. Hauptprojekt und neun Launcher-Projekte auf .NET Framework 4.8 retargetet.
2. `app.config` auf 4.8 angepasst.
3. GitHub-URLs zentralisiert und auf den Fork umgestellt.
4. Hauptprojekt erfolgreich gebaut.
5. Alle neun Launcher erfolgreich gebaut.

Offen:

1. Live-Installation noch nicht ersetzt; zuerst Testkopie und Baseline-Smoke-Test.
2. CLI-Implementierung gemaess `02-CLI-SPEC.md`.
3. GitHub-Actions-Workflows nach dem Push auf GitHub verifizieren.

Bereits erzeugt:

- `dist/Portable.Brave.Updater.v1.3.3.7.7z`
  - `Portable Brave Updater.exe` (1.3.3.7)
  - `Bin/7zr.exe` (19.00)
  - `Bin/Launcher/*.exe` (1.3.3.7)
  - `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
- `dist/SHA256SUMS.txt`
- `Launcher/Launcher.7z` (9 Launcher, 1.3.3.7)

## 8. Reproduktion

Fuer eine frische Maschine:

```powershell
# 1. Build Tools installieren, falls nicht vorhanden
& "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe" install `
  --id Microsoft.VisualStudio.2022.BuildTools `
  --source winget `
  --accept-source-agreements

# 2. Bestehende Installation modifizieren
$installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
$installPath = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"

& $installer modify `
  --installPath $installPath `
  --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools `
  --add Microsoft.Net.Component.4.8.TargetingPack `
  --passive --norestart

# 3. Verifizieren
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * `
  -requires Microsoft.Net.Component.4.8.TargetingPack `
  -property installationPath

Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"

Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs\Windows" -Recurse -Filter TlbImp.exe
```

Hinweis: Wenn `winget` im `PATH` fehlt, den vollstaendigen Pfad verwenden oder den Visual Studio Installer grafisch oeffnen.

## 9. Build-Ergebnis 2026-11-02

Hauptprojekt:

```text
bin\Release\Portable Brave Updater.exe
FileVersion: 1.3.3.7
Groesse: 103424 Bytes
```

Launcher:

```text
Alle neun Launcher-Projekte erfolgreich gebaut.
Ausgaben unter Launcher\<Name>\bin\Release\.
Launcher-Version: 1.3.3.7
Launcher/Launcher.7z neu erzeugt: 9 EXEs, 314639 Bytes.
```

Fork-URLs:

```text
Version.txt          -> HTTP 200
Launcher/Version.txt -> HTTP 200
Launcher/Launcher.7z -> HTTP 200
```

Hinweis: `Bin/` ist noch nicht zusammengestellt. Fuer einen Laufzeit-Smoke-Test muessen `Bin/7zr.exe` und `Bin/Launcher/*.exe` bereitgestellt werden.
