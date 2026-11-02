# 00 - Analysis

Last updated: 2026-11-02

## 1. Fork state

The fork started as an exact copy of the upstream repository. It now contains local build, CI, release and documentation changes.

- Upstream last commit: 2022.
- Upstream has 6 open issues, including SSL/TLS and failed-update reports.
- The fork is public and MIT-licensed.
- The fork currently targets the updater at `1.3.3.37` and the launchers at `1.3.3.7`.

## 2. Technical structure

| Component | Technology |
|---|---|
| Main updater | C# / .NET Framework 4.8 / WinForms (retargeted from 4.7) |
| Solution | VS 2019 format, `Portable Brave Updater.sln` |
| Launchers | 9 separate C# projects, .NET Framework 4.8 (retargeted from 4.0) |
| Update protocol | Brave/Omaha `update2` via HTTPS POST and XML |
| Extraction | external `Bin/7zr.exe` |
| Shortcuts | COM `IWshRuntimeLibrary` / `WScript.Shell` |
| Release | `.7z` with `Bin/7zr.exe`, `Bin/Launcher/*.exe` and updater EXE |
| License | MIT, Copyright 2020 UndertakerBen |

Important code locations:

- `Program.Main`: no command-line arguments yet.
- `Form1` constructor: performs network calls and UI setup.
- `DownloadFile`: tightly coupled to WinForms and progress controls.
- `CheckUpdate`: self-update check and UI.
- `CheckLauncher`: launcher update check.
- GitHub URLs: centralized in `RepositoryUrl` and `RawMasterUrl` and pointed at the fork.
- Running Brave blocks updates via `MessageBox`.
- Main and launcher projects are retargeted to .NET Framework 4.8.

## 3. Protocol test

The Brave update endpoint was tested on 2026-11-02 with the same Omaha request used by the code:

- HTTP 200
- Stable x64: version `156.1.98.63`
- Download URLs and manifest were returned.

The core protocol is therefore still functional. Extraction and installation were not tested; a baseline smoke test is still required.

## 4. Build complexity on Windows 11

Assessment: **medium**, manageable for a DevOps background.

Positive points:

- Pure .NET Framework/WinForms project, no modern SDK dependencies.
- NuGet is only needed for old analyzers; no runtime packages.
- MSBuild build is straightforward.

Pitfalls:

1. Visual Studio 2022 cannot build .NET Framework 4.0 out of the box. The launchers originally targeted 4.0.
2. The main project originally targeted 4.7; VS 2022 installs 4.7.2/4.8 by default. Either install the 4.7 targeting pack or retarget to 4.8.
3. The repository has no `Bin/` folder. A runtime test needs `7zr.exe` and launcher EXEs from the official release or from a local build.
4. The COM reference `IWshRuntimeLibrary` needs `tlbimp.exe` from the .NET Framework 4.8 SDK.
5. The old FxCop analyzers in `packages.config` can produce warnings on modern toolchains; remove them if necessary.

Recommended solution:

- Retarget all projects to **.NET Framework 4.8**.
- Install Visual Studio 2022 Build Tools or Community with the .NET desktop workload and Windows SDK.
- For the first prototype, use `Bin/` from the official release `.7z`.

## 5. Rewrite or keep C#?

**Recommendation: keep C#.**

Reasons:

- The planned change is small.
- A rewrite would have to recreate the WinForms GUI, Omaha protocol, 7z extraction, COM shortcuts, launcher profiles and self-update.
- Rewriting in Go, Rust or Python adds risk without a functional benefit for this change.
- MIT permits further development.
- .NET Framework 4.8 is part of Windows 11 and is supported with the Windows lifecycle.

Optional future modernization: .NET 8/9 WinForms. That is a separate project and not part of this change.

## 6. License

MIT explicitly allows:

- use and copying
- modification and merging
- publication and distribution
- sublicensing
- sale, including commercial use

Condition: the copyright notice and MIT license text must remain in copies or substantial portions. The fork may publish new versions as long as `LICENSE` is included. Additional copyright can be added.

Not covered by MIT:

- The Brave name and Brave logos (trademarks).
- Brave's terms of service for automated installer downloads.

The existing README disclaimer should be kept.

## 7. Main risks

| Risk | Impact | Mitigation |
|---|---|---|
| Self-update points upstream | Fork is replaced by upstream | Change URLs or disable self-update |
| UI logic in the core | Headless operation is difficult | Extract the update core |
| MessageBoxes | Automation blocks | `Notify` abstraction, `--quiet` |
| No tests | Regressions | Parser tests + VM smoke tests |
| .NET 4.0 launchers | Build with VS 2022 fails | Retarget to 4.8 |
| Missing `Bin/` | EXE starts but update fails | Provide release assets |
| Unsigned EXE | SmartScreen warning | Optional code signing |
| Self-update without hash check | Supply-chain risk | Disable or verify hash/signature |

## 8. Decisions

1. Launch parameter is `--launch-brave`.
2. Update scope: `--update=existing` as default, `--update=all` as alternative. `--update all` is accepted as a tolerant spelling.
3. Launch: if exactly one edition is installed, start it. Prefer the existing launcher.
4. Self-update: URLs point to the fork; release assets are provided by the fork release.

## 9. References

- .NET Framework Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- .NET Framework System Requirements: https://learn.microsoft.com/en-us/dotnet/framework/get-started/system-requirements
- VS Build Tools workload IDs: https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools
- 7-Zip license: https://7-zip.org/license.txt
- MIT license: https://opensource.org/license/mit
