# 07 - Installation Log: Build Environment

Last updated: 2026-11-02  
Host: Windows 11 x64  
Shell: elevated PowerShell

## 1. Starting point

Visual Studio Build Tools 2022 was already installed, but without the .NET components required by this project.

| Item | Value |
|---|---|
| Product | Visual Studio Build Tools 2022 |
| Installation path | `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools` |
| Installation version | `17.14.36915.13` |
| Product version | 17.14.25 (January 2026) |
| MSBuild | `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe` |
| MSBuild version | `17.14.40.60911` |

Missing components before the change:

- `Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools`
- `Microsoft.Net.Component.4.8.TargetingPack`
- `Microsoft.Net.Component.4.8.SDK`
- `Microsoft.VisualStudio.Component.NuGet.BuildTools`

Already present:

- `Microsoft.Component.MSBuild`
- `Microsoft.VisualStudio.Component.Roslyn.Compiler`
- `Microsoft.VisualStudio.Component.Windows11SDK.26100`

## 2. winget finding

`winget` is installed (`v1.29.380`) but was not in `PATH` in the elevated PowerShell. The app execution alias is located at:

```text
C:\Users\<user>\AppData\Local\Microsoft\WindowsApps\winget.exe
```

Direct invocation works:

```powershell
& "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe" --version
```

`winget` was not required because the Build Tools were already installed. The installation was modified through the Visual Studio Installer.

## 3. Change performed

Modified the existing Build Tools installation, no reinstall:

```powershell
$installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
$installPath = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"

& $installer modify `
  --installPath $installPath `
  --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools `
  --add Microsoft.Net.Component.4.8.TargetingPack `
  --passive --norestart
```

Deliberately not used:

- `--includeRecommended`: would have installed .NET 8/9 SDK/runtime unnecessarily.
- `--includeOptional`.
- Visual Studio 2019.
- .NET Framework 4.0 Targeting Pack.

The workload includes the required components, in particular:

- .NET Framework 4.8 SDK
- MSBuild
- Roslyn compiler
- NuGet Build Tools
- .NET Framework 4.7.2 Targeting Pack (workload requirement)

Additionally added explicitly:

- .NET Framework 4.8 Targeting Pack

## 4. Verification

### vswhere

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * `
  -requires Microsoft.Net.Component.4.8.TargetingPack `
  -property installationPath
```

Result:

```text
C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools
```

### Reference assemblies

```powershell
Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
```

Result: `True`

### TlbImp

```powershell
Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs\Windows" -Recurse -Filter TlbImp.exe
```

Result:

```text
C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\TlbImp.exe
C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64\TlbImp.exe
```

### Component status

| Component | Status |
|---|---|
| `Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools` | installed |
| `Microsoft.Net.Component.4.8.TargetingPack` | installed |
| `Microsoft.Net.Component.4.8.SDK` | installed |
| `Microsoft.VisualStudio.Component.NuGet.BuildTools` | installed |
| `Microsoft.VisualStudio.Component.Windows11SDK.26100` | installed |
| `Microsoft.Component.MSBuild` | installed |
| `Microsoft.VisualStudio.Component.Roslyn.Compiler` | installed |
| `v4.8` reference assemblies | present |
| `TlbImp.exe` x86/x64 | present |

## 5. SDK safety assessment

- Target is **.NET Framework 4.8**. It is actively supported and part of Windows 11.
- `TlbImp.exe` has a file date from 2019. This is normal: the .NET Framework 4.8 SDK has not been re-released since then. The tool belongs to the supported 4.8 SDK, not the dead 4.0 stack.
- **.NET Framework 4.0** has been out of support since 2016. It is not installed and the launchers are retargeted to 4.8.
- **.NET 8/9 SDK** was not installed. It is not required for this project.
- **Visual Studio 2019** was not installed.
- **Windows 11 SDK 26100** is present; the relevant `tlbimp.exe` comes from the .NET Framework 4.8 SDK.

## 6. System changes

| Change | Details |
|---|---|
| Workload added | .NET Desktop Build Tools |
| Component added | .NET Framework 4.8 Targeting Pack |
| Component installed | .NET Framework 4.8 SDK |
| Component installed | NuGet Build Tools |
| No reinstall | existing Build Tools instance modified |
| No reboot | `--norestart` |

## 7. Status after build

Done on 2026-11-02:

1. Main project and nine launcher projects retargeted to .NET Framework 4.8.
2. `app.config` updated to 4.8.
3. GitHub URLs centralized and pointed to the fork.
4. Main project built successfully.
5. All nine launchers built successfully.

Open:

1. Live installation replacement for `1.3.37` and baseline update test.
2. Platform launch test for the PortableApps entries.
3. Verify the release workflow after pushing the `v1.3.37` tag.

`1.3.3.37` was released earlier on 2026-11-02. The PortableApps integration and runtime dependency preflight are part of `1.3.37`.

CLI MVP implemented and locally verified with `--help`, `--version`, invalid arguments, `--check`, `--dry-run`, `--quiet`, `--log` and `--wait-for-brave`.

Already produced:

- `dist/Portable.Brave.Updater.v1.3.37.7z`
  - `Portable Brave Updater.exe` (1.3.37)
  - `Bin/7zr.exe` (19.00)
  - `Bin/Launcher/*.exe` (1.3.3.7)
  - `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
  - `update_and_close.bat`, `update_and_run_brave.bat`
- `dist/SHA256SUMS.txt`
- `Launcher/Launcher.7z` (9 launchers, 1.3.3.7)

## 8. Reproduction

For a fresh machine:

```powershell
# 1. Install Build Tools if not present
& "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe" install `
  --id Microsoft.VisualStudio.2022.BuildTools `
  --source winget `
  --accept-source-agreements

# 2. Modify existing installation
$installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
$installPath = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"

& $installer modify `
  --installPath $installPath `
  --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools `
  --add Microsoft.Net.Component.4.8.TargetingPack `
  --passive --norestart

# 3. Verify
& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest -products * `
  -requires Microsoft.Net.Component.4.8.TargetingPack `
  -property installationPath

Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"

Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs\Windows" -Recurse -Filter TlbImp.exe
```

Note: if `winget` is missing from `PATH`, use the full path or open the Visual Studio Installer.

## 9. Build result 2026-11-02

Main project:

```text
bin\Release\Portable Brave Updater.exe
FileVersion: 1.3.37
Size: 143360 bytes
```

Launchers:

```text
All nine launcher projects built successfully.
Output under Launcher\<Name>\bin\Release\.
Launcher version: 1.3.3.7
Launcher/Launcher.7z: 9 EXEs, 314639 bytes.
```

Fork URLs:

```text
Version.txt          -> HTTP 200
Launcher/Version.txt -> HTTP 200
Launcher/Launcher.7z -> HTTP 200
```

Note: `Bin/` has not been assembled for the live installation yet. A runtime smoke test requires `Bin/7zr.exe` and `Bin/Launcher/*.exe`.
