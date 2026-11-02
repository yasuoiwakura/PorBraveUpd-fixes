# 01 - Build und Entwicklungsumgebung

Stand: 2026-11-02

## 1. Zielbild

- Ziel-Framework: **.NET Framework 4.8** fuer Hauptprojekt und Launcher.
- Build-Tool: **MSBuild aus Visual Studio 2022 Build Tools** (oder Community).
- Kein .NET-Framework-4.0-Targeting mehr.
- Kein .NET-SDK-Zwang; `dotnet build` ist fuer diese alten Projektdateien nicht der primaere Weg.

Begruendung: .NET Framework 4.8 ist aktiv unterstuetzt und auf Windows 11 vorinstalliert. .NET Framework 4.0 ist seit 2016 out of support und mit VS 2022 nicht mehr ohne Workaround baubar.

### Aktueller Installationsstand (2026-11-02)

Die Build-Umgebung ist eingerichtet. Details und exakte Befehle stehen in `07-INSTALLATIONSPROTOKOLL.md`.

| Merkmal | Wert |
|---|---|
| Build Tools | Visual Studio Build Tools 2022, 17.14.25 |
| InstallationPath | `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools` |
| MSBuild | `17.14.40.60911` |
| Workload | .NET Desktop Build Tools |
| .NET Framework 4.8 SDK | installiert |
| .NET Framework 4.8 Targeting Pack | installiert |
| Windows 11 SDK | 26100 installiert |
| TlbImp | `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\TlbImp.exe` |

## 2. Was installiert werden muss

### Pflicht

| Komponente | Zweck |
|---|---|
| Visual Studio 2022 Build Tools oder Community | MSBuild, Roslyn, NuGet |
| Workload ".NET Desktop Build Tools" bzw. ".NET-Desktopentwicklung" | WinForms-Build |
| .NET Framework 4.8 Targeting Pack | Referenzassemblies fuer `net48` |
| Windows 11 SDK | `tlbimp.exe` fuer COM-Referenz `IWshRuntimeLibrary` |
| Git | Repository |
| 7-Zip oder `7zr.exe` | Release-Paket erstellen bzw. `Bin/` bereitstellen |

### Optional

| Komponente | Zweck |
|---|---|
| GitHub CLI (`gh`) | Releases und PRs |
| Visual Studio Code | Leichtere Editor-Alternative |
| Code-Signing-Zertifikat | Signierte Releases, weniger SmartScreen-Warnungen |

### Nicht noetig

- .NET Framework 4.0 Developer/Targeting Pack.
- Visual Studio 2019.
- .NET 8/9 SDK.

## 3. Installation Schritt fuer Schritt

### Variante A: Build Tools (schlank, empfohlen)

1. Installer starten:

   ```powershell
   winget install Microsoft.VisualStudio.2022.BuildTools
   ```

   Hinweis: Wenn `winget` in der erhoehten PowerShell nicht im `PATH` liegt, den vollstaendigen Pfad verwenden:

   ```powershell
   & "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe" install Microsoft.VisualStudio.2022.BuildTools
   ```

   Oder den Visual Studio Installer direkt oeffnen.

2. Visual Studio Installer oeffnen, "Aendern" waehlen.
3. Workload **".NET Desktop Build Tools"** aktivieren.
4. Unter "Einzelne Komponenten" sicherstellen:
   - `.NET Framework 4.8 targeting pack`
   - `.NET Framework 4.8 SDK`
   - `Windows 11 SDK` (oder die aktuell angebotene Windows-SDK-Version)
   - `NuGet targets and build tasks`
   - `C# and Visual Basic Roslyn compilers`
5. Installation abschliessen.

### Variante A2: Bestehende Build Tools per CLI modifizieren

Wenn die Build Tools bereits installiert sind, keine Neuinstallation ausfuehren, sondern die Instanz modifizieren:

```powershell
$installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
$installPath = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"

& $installer modify `
  --installPath $installPath `
  --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools `
  --add Microsoft.Net.Component.4.8.TargetingPack `
  --passive --norestart
```

Bewusst ohne `--includeRecommended`, damit nicht unnoetig .NET 8/9 SDK installiert wird. Die Workload bringt .NET 4.8 SDK, MSBuild, Roslyn und NuGet mit.

### Variante B: Community (mit IDE)

1. Installer starten:

   ```powershell
   winget install Microsoft.VisualStudio.2022.Community
   ```

2. Workload **".NET-Desktopentwicklung"** aktivieren.
3. Einzelne Komponenten wie bei Variante A ergaenzen.
4. Installation abschliessen.

### 7-Zip

Fuer das Erstellen des Release-`.7z`:

```powershell
winget install 7zip.7zip
```

Fuer den Prototyp reicht es, `Bin/7zr.exe` aus dem offiziellen Release zu uebernehmen.

## 4. Projekt auf .NET Framework 4.8 retargeten

### Hauptprojekt

In `Portable Brave Updater.csproj`:

```xml
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
```

In `app.config` optional:

```xml
<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
```

### Launcher

In allen neun Launcher-CSProjects:

```xml
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
```

Betroffen sind alle Projekte unter `Launcher/`, z. B.:

- `Launcher/Brave Stable x64 Launcher/Brave Stable x64 Launcher.csproj`
- `Launcher/Brave Beta x86 Launcher/Brave Beta x86 Launcher.csproj`
- usw.

Nach dem Retargeting muessen die Launcher neu gebaut werden. Fuer den ersten Prototyp kann man stattdessen die vorcompilierten Launcher aus dem offiziellen Release verwenden.

## 5. Fehlende `Bin/`-Assets

Das Repo enthaelt keinen `Bin/`-Ordner. Das offizielle Release-`.7z` enthaelt:

```text
Bin/7zr.exe
Bin/Launcher/Brave Beta x64 Launcher.exe
Bin/Launcher/Brave Beta x86 Launcher.exe
Bin/Launcher/Brave Dev x64 Launcher.exe
Bin/Launcher/Brave Dev x86 Launcher.exe
Bin/Launcher/Brave Launcher.exe
Bin/Launcher/Brave Nightly x64 Launcher.exe
Bin/Launcher/Brave Nightly x86 Launcher.exe
Bin/Launcher/Brave Stable x64 Launcher.exe
Bin/Launcher/Brave Stable x86 Launcher.exe
Portable Brave Updater.exe
```

### Schnellster Prototyp

1. Offizielles Release `Portable.Brave.Updater.v1.3.0.0.7z` entpacken.
2. Entpackten Inhalt als Basis verwenden.
3. Neu gebaute `Portable Brave Updater.exe` hineinkopieren.
4. Testen.

Damit umgeht man zunaechst die 4.0-Launcher-Problematik vollstaendig.

### Sauberer Release-Build

1. Launcher auf 4.8 retargeten.
2. Alle neun Launcher bauen.
3. Ausgaben nach `Bin/Launcher/` kopieren.
4. `7zr.exe` aus dem LZMA SDK oder dem offiziellen Release nach `Bin/` kopieren.
5. Hauptprojekt bauen.
6. Alles mit 7-Zip paketieren.

## 6. Build-Befehle

### Hauptprojekt

```powershell
msbuild "Portable Brave Updater.sln" /p:Configuration=Release /p:Platform="Any CPU"
```

Ausgabe: `bin\Release\Portable Brave Updater.exe`

### Launcher

Beispiel fuer einen Launcher:

```powershell
msbuild "Launcher\Brave Stable x64 Launcher\Brave Stable x64 Launcher.sln" /p:Configuration=Release
```

`build.ps1` baut alle Launcher und stellt das Release-Paket zusammen.

## 7. GitHub Actions

Workflows:

| Datei | Trigger | Zweck |
|---|---|---|
| `.github/workflows/build.yml` | `workflow_call` | Reusable Build, Paketierung, Artifacts |
| `.github/workflows/ci.yml` | Push auf `master`/`dev`, PR, manuell | CI-Build ohne Release |
| `.github/workflows/release.yml` | Tags `v*` | Stabiles GitHub Release |

`dev-latest.yml` gibt es nicht mehr. Dev-Builds werden als zeitgestempeltes Actions-Artefakt veroeffentlicht, nicht als GitHub Release.

Ablauf in `build.yml`:

1. Checkout
2. MSBuild bereitstellen
3. .NET Framework 4.8 Targeting Pack und `TlbImp.exe` pruefen
4. NuGet-Restore
5. `build.ps1 -Configuration Release` ausfuehren
6. `dist/BUILD-INFO.txt` mit Zeitstempel, Commit, Ref, Version und Run-URL erzeugen
7. Artifact(s) hochladen

`build.ps1`:

- baut Hauptprojekt und alle neun Launcher
- stellt `dist/staging` zusammen
- kopiert `tools/7zr/7zr.exe` nach `Bin/7zr.exe`
- erzeugt `dist/Portable.Brave.Updater.v<Version>.7z`
- erzeugt `dist/SHA256SUMS.txt`

Artifacts:

| Branch/Event | Artifact |
|---|---|
| `dev` Push oder manuell auf `dev` | `portable-brave-updater-dev-<YYYYMMDD-HHMMSSZ>-<sha>` |
| `master`, PR, Release-Tag | `portable-brave-updater` |

Dev-Artefakt:

- enthaelt `.7z`, `SHA256SUMS.txt`, `BUILD-INFO.txt`
- Retention: 30 Tage
- Berechtigungen: nur `contents: read`
- kein Release, kein Tag, kein Repo-Write
- wird nicht automatisch vom Updater geladen; nur manueller Download

Release:

- Trigger nur Tag `v*`
- prueft Tag, `Version.txt`, `AssemblyFileVersion` und Assetnamen
- laedt `.7z` und `SHA256SUMS.txt` hoch
- kein automatisches Commit von `Version.txt`
- Der Tag muss auf `master` zeigen; der Updater liest `Version.txt` von `master`.
- Der Workflow erscheint in der Actions-Liste erst, wenn `release.yml` auf dem Default-Branch `master` liegt und/oder ein Tag `v*` gepusht wurde.

Hinweis: `windows-latest` bringt VS 2022 und .NET Framework 4.8 in der Regel mit. Der Workflow prueft das explizit und bricht sonst mit einer klaren Meldung ab.

## 8. Typische Build-Fehler

| Fehler | Ursache | Loesung |
|---|---|---|
| `winget` nicht gefunden | App-Execution-Alias nicht im `PATH` der erhoehten Shell | vollstaendigen Pfad nutzen oder Visual Studio Installer oeffnen |
| `MSB3644` | Targeting Pack fehlt | .NET Framework 4.8 Targeting Pack installieren |
| `MSB3644` fuer v4.0 | Launcher noch auf 4.0 | auf 4.8 retargeten |
| `tlbimp.exe` nicht gefunden | .NET Framework 4.8 SDK fehlt | Workload/.NET 4.8 SDK installieren |
| COM-Referenzfehler | `IWshRuntimeLibrary` | TlbImp/.NET 4.8 SDK pruefen oder COM-Interop spaeter ersetzen |
| NuGet-Restore-Fehler | alte Analyzer | `packages.config` bereinigen |
| ResX-Fehler | Designer-Ressourcen | Projekt in VS 2022 neu laden, ResX regenerieren |

## 9. Verifikation

Ist-Stand und exakte Ausgaben: `07-INSTALLATIONSPROTOKOLL.md`.

Kurzpruefung:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * `
  -requires Microsoft.Net.Component.4.8.TargetingPack `
  -property installationPath

Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"

Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs\Windows" -Recurse -Filter TlbImp.exe
```

Nach der CLI-Implementierung:

```powershell
.\bin\Release\Portable Brave Updater.exe --help
```

Fuer den Baseline-Test zunaechst ohne CLI:

1. `Bin/` aus dem Release bereitstellen.
2. Updater starten.
3. Eine Edition aktualisieren.
4. `Brave <Edition>\updates\Version.log` pruefen.
5. Sicherstellen, dass die Launcher-EXE kopiert wurde.

## 10. Build-Ergebnis 2026-11-02

Hauptprojekt:

```text
bin\Release\Portable Brave Updater.exe
FileVersion: 1.3.3.7
Groesse: 103424 Bytes
```

Launcher: alle neun Projekte erfolgreich gebaut.

Fork-URLs:

```text
https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master/Version.txt          -> HTTP 200
https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master/Launcher/Version.txt -> HTTP 200
https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master/Launcher/Launcher.7z -> HTTP 200
```

Build-Artefakte werden ueber `.gitignore` von `bin/`/`obj/` ferngehalten.

## 11. Referenzen

- .NET Framework Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- .NET Framework Developer Pack: https://learn.microsoft.com/en-us/dotnet/framework/install/guide-for-developers
- VS Build Tools Komponenten: https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools
- Microsoft.NETFramework.ReferenceAssemblies: https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies
