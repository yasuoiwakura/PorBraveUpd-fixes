# 01 - Build and Development Environment

Last updated: 2026-11-02

## 1. Target state

- Target framework: **.NET Framework 4.8** for the main project and launchers.
- Build tool: **MSBuild from Visual Studio 2022 Build Tools** (or Community).
- No .NET Framework 4.0 targeting.
- No .NET SDK requirement; `dotnet build` is not the primary path for these old project files.

Reason: .NET Framework 4.8 is actively supported and preinstalled on Windows 11. .NET Framework 4.0 has been out of support since 2016 and cannot be built with VS 2022 without workarounds.

### Current environment (2026-11-02)

The build environment is configured. Exact commands and results are in `07-INSTALLATION-LOG.md`.

| Item | Value |
|---|---|
| Build Tools | Visual Studio Build Tools 2022, 17.14.25 |
| Installation path | `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools` |
| MSBuild | `17.14.40.60911` |
| Workload | .NET Desktop Build Tools |
| .NET Framework 4.8 SDK | installed |
| .NET Framework 4.8 Targeting Pack | installed |
| Windows 11 SDK | 26100 installed |
| TlbImp | `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\TlbImp.exe` |

## 2. Required components

### Required

| Component | Purpose |
|---|---|
| Visual Studio 2022 Build Tools or Community | MSBuild, Roslyn, NuGet |
| ".NET Desktop Build Tools" workload | WinForms build |
| .NET Framework 4.8 Targeting Pack | Reference assemblies for `net48` |
| Windows 11 SDK | `tlbimp.exe` for the `IWshRuntimeLibrary` COM reference |
| Git | Repository |
| 7-Zip or `7zr.exe` | Create release packages / provide `Bin/` |

### Optional

| Component | Purpose |
|---|---|
| GitHub CLI (`gh`) | Releases and PRs |
| Visual Studio Code | Lightweight editor alternative |
| Code-signing certificate | Signed releases, fewer SmartScreen warnings |

### Not required

- .NET Framework 4.0 Developer/Targeting Pack.
- Visual Studio 2019.
- .NET 8/9 SDK.

## 3. Installation step by step

### Option A: Build Tools (lean, recommended)

1. Start the installer:

   ```powershell
   winget install Microsoft.VisualStudio.2022.BuildTools
   ```

   Note: if `winget` is not in `PATH` in the elevated PowerShell, use the full path:

   ```powershell
   & "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe" install Microsoft.VisualStudio.2022.BuildTools
   ```

   Or open the Visual Studio Installer directly.

2. Open the Visual Studio Installer and choose "Modify".
3. Enable the **".NET Desktop Build Tools"** workload.
4. Under "Individual components", make sure these are selected:
   - `.NET Framework 4.8 targeting pack`
   - `.NET Framework 4.8 SDK`
   - `Windows 11 SDK` (or the currently offered Windows SDK)
   - `NuGet targets and build tasks`
   - `C# and Visual Basic Roslyn compilers`
5. Finish the installation.

### Option A2: Modify existing Build Tools via CLI

If the Build Tools are already installed, do not reinstall; modify the instance:

```powershell
$installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
$installPath = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"

& $installer modify `
  --installPath $installPath `
  --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools `
  --add Microsoft.Net.Component.4.8.TargetingPack `
  --passive --norestart
```

Deliberately without `--includeRecommended`, so .NET 8/9 SDK is not installed unnecessarily. The workload brings in .NET 4.8 SDK, MSBuild, Roslyn and NuGet.

### Option B: Community (with IDE)

1. Start the installer:

   ```powershell
   winget install Microsoft.VisualStudio.2022.Community
   ```

2. Enable the **".NET desktop development"** workload.
3. Add the individual components from Option A.
4. Finish the installation.

### 7-Zip

To create the release `.7z`:

```powershell
winget install 7zip.7zip
```

For the prototype, using `tools/7zr/7zr.exe` is enough.

## 4. Retarget the project to .NET Framework 4.8

### Main project

In `Portable Brave Updater.csproj`:

```xml
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
```

In `app.config` (optional):

```xml
<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
```

### Launchers

In all nine launcher CSProjects:

```xml
<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
```

Affected projects under `Launcher/`, for example:

- `Launcher/Brave Stable x64 Launcher/Brave Stable x64 Launcher.csproj`
- `Launcher/Brave Beta x86 Launcher/Brave Beta x86 Launcher.csproj`
- etc.

After retargeting, the launchers must be rebuilt. For the first prototype, precompiled launchers from the official release can be used instead.

## 5. `Bin/` assets

The repository does not contain a compiled `Bin/` folder. It does contain `tools/7zr/7zr.exe`; `build.ps1` copies it into the staging `Bin/` folder.

The official release `.7z` contains:

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

### Fastest prototype

1. Extract the official release `Portable.Brave.Updater.v1.3.0.0.7z`.
2. Use the extracted content as the base.
3. Copy the newly built `Portable Brave Updater.exe` into it.
4. Test.

This avoids the legacy .NET 4.0 launcher problem.

### Clean release build

1. Retarget the launchers to 4.8.
2. Build all nine launchers.
3. Copy outputs to `Bin/Launcher/`.
4. Copy `7zr.exe` from `tools/7zr/` or the official release into `Bin/`.
5. Build the main project.
6. Package everything with 7-Zip.

## 6. Build commands

### Main project

```powershell
msbuild "Portable Brave Updater.sln" /p:Configuration=Release /p:Platform="Any CPU"
```

Output: `bin\Release\Portable Brave Updater.exe`

### Launchers

Example for one launcher:

```powershell
msbuild "Launcher\Brave Stable x64 Launcher\Brave Stable x64 Launcher.sln" /p:Configuration=Release
```

`build.ps1` builds all launchers and assembles the release package.

## 7. GitHub Actions

Workflows:

| File | Trigger | Purpose |
|---|---|---|
| `.github/workflows/build.yml` | `workflow_call` | Reusable build, packaging, artifacts |
| `.github/workflows/ci.yml` | PR, manual | CI build without release |
| `.github/workflows/nightly.yml` | Daily 03:00 UTC, manual | Dev build only when `dev` changed |
| `.github/workflows/release.yml` | Tags `v*` | Stable GitHub release |

`dev-latest.yml` no longer exists. Dev builds are published as timestamped Actions artifacts, not GitHub Releases. Pushes to `master` and `dev` do not start a build automatically.

Nightly behavior:

- schedule is `03:00 UTC`; GitHub may delay scheduled runs
- checks out `dev`
- skips if a `v*` tag points at the current commit
- skips if the last successful `nightly` run has the same commit SHA
- `workflow_dispatch` can force a build with `force=true`

If the `ci` workflow is disabled in the GitHub Actions UI, the separate `nightly` workflow and its manual button remain available.

`build.yml` accepts optional `ref` and `channel` inputs. Nightly passes `ref=dev` and `channel=dev`.

`build.yml` flow:

1. Checkout
2. Set up MSBuild
3. Verify .NET Framework 4.8 Targeting Pack and `TlbImp.exe`
4. NuGet restore
5. Run `build.ps1 -Configuration Release`
6. Create `dist/BUILD-INFO.txt` with timestamp, commit, ref, version and run URL
7. Upload artifact(s)

`build.ps1`:

- builds the main project and all nine launchers
- assembles `dist/staging`
- copies `tools/7zr/7zr.exe` to `Bin/7zr.exe`
- creates `dist/Portable.Brave.Updater.v<Version>.7z`
- creates `dist/SHA256SUMS.txt`

Artifacts:

| Branch/event | Artifact |
|---|---|
| Nightly `dev` build | `portable-brave-updater-dev-<YYYYMMDD-HHMMSSZ>-<sha>` |
| PR, release tag | `portable-brave-updater` |

Dev artifact:

- contains `.7z`, `SHA256SUMS.txt`, `BUILD-INFO.txt`
- retention: 30 days
- permissions: `contents: read` only
- no release, no tag, no repo write
- not loaded automatically by the updater; manual download only

Release:

- trigger only on tag `v*`
- verifies tag, `Version.txt`, `AssemblyFileVersion`, asset name and that release docs exist
- uploads `.7z` and `SHA256SUMS.txt`
- does not commit `Version.txt`
- tag must point to `master`; the updater reads `Version.txt` from `master`
- release notes are read from `docs/RELEASE-NOTES-<Version>.md` (required)
- the workflow appears in the Actions list once `release.yml` is on the default branch `master` and/or a `v*` tag is pushed

Note: `windows-latest` normally includes VS 2022 and .NET Framework 4.8. The workflow verifies this and fails with a clear message otherwise.

## 8. Common build errors

| Error | Cause | Fix |
|---|---|---|
| `winget` not found | App execution alias is not in the elevated shell's `PATH` | Use the full path or open the Visual Studio Installer |
| `MSB3644` | Targeting Pack missing | Install the .NET Framework 4.8 Targeting Pack |
| `MSB3644` for v4.0 | Launcher still targets 4.0 | Retarget to 4.8 |
| `tlbimp.exe` not found | .NET Framework 4.8 SDK missing | Install the workload/.NET 4.8 SDK |
| COM reference error | `IWshRuntimeLibrary` | Check TlbImp/.NET 4.8 SDK or replace COM interop later |
| NuGet restore error | Old analyzers | Clean up `packages.config` |
| ResX error | Designer resources | Reload the project in VS 2022 and regenerate ResX |

## 9. Verification

Current state and exact output: `07-INSTALLATION-LOG.md`.

Quick check:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * `
  -requires Microsoft.Net.Component.4.8.TargetingPack `
  -property installationPath

Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"

Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs\Windows" -Recurse -Filter TlbImp.exe
```

CLI verification:

```powershell
.\bin\Release\Portable Brave Updater.exe --help
.\bin\Release\Portable Brave Updater.exe --version
.\bin\Release\Portable Brave Updater.exe --check
```

For the baseline test without the CLI:

1. Provide `Bin/` from the release.
2. Start the updater.
3. Update one edition.
4. Check `Brave <Edition>\updates\Version.log`.
5. Make sure the launcher EXE was copied.

## 10. Build result 2026-11-02

Main project:

```text
bin\Release\Portable Brave Updater.exe
FileVersion: 1.4.10
Size: 144384 bytes
```

Launchers: all nine projects built successfully.

Fork URLs:

```text
https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master/Version.txt          -> HTTP 200
https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master/Launcher/Version.txt -> HTTP 200
https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master/Launcher/Launcher.7z -> HTTP 200
```

Local release package:

```text
dist/Portable.Brave.Updater.v1.4.10.7z
Checksum: see dist/SHA256SUMS.txt
```

Build artifacts are kept out of the repository by `.gitignore`.

## 11. References

- .NET Framework Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- .NET Framework Developer Pack: https://learn.microsoft.com/en-us/dotnet/framework/install/guide-for-developers
- VS Build Tools components: https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools
- Microsoft.NETFramework.ReferenceAssemblies: https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies
