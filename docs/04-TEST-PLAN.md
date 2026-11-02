# 04 - Test Plan

Last updated: 2026-11-02

## 1. Test goals

- Existing GUI behavior remains intact.
- CLI behavior matches `02-CLI-SPEC.md`.
- Update paths work for x86 and x64.
- Error cases produce clean exit codes and no blocking dialogs.
- A release package is complete and license-compliant.

## 2. Test environment

- Windows 11 x64 (primary).
- Optional Windows 10 x64 for compatibility.
- Test directory, not the production installation.
- Before update tests: back up the edition or use a fresh copy.
- Network access to `updates.bravesoftware.com` and GitHub.
- For network failures: disable the network or point the proxy to an unreachable target.

## 3. Test levels

| Level | Content | Automatable |
|---|---|---|
| Build | Build main project and launchers | yes |
| Parser | Arguments, defaults, errors | yes |
| Smoke | `--help`, `--version`, `--check` | partly |
| Integration | Real update of one edition | no, VM/manual |
| Regression | Interactive GUI mode | manual |
| Release | Package, licenses, checksums | partly |

## 4. Base tests

| ID | Test | Procedure | Expectation |
|---|---|---|---|
| T01 | Build main project | `msbuild ... /p:Configuration=Release` | Build succeeds |
| T02 | Build launchers | all nine launcher solutions | Build succeeds |
| T03 | `--help` | Start EXE with `--help` | Help, exit 0 |
| T04 | `--version` | Start EXE with `--version` | Version, exit 0 |
| T05 | Invalid argument | `--foo` | Error message, exit 2 |
| T06 | `--update` without value | `--update` | Error message, exit 2 |
| T07 | `--timeout` without `--wait-for-brave` | `--timeout 10` | Error message, exit 2 |
| T08 | `--if-new --force` | both flags | Error message, exit 2 |

## 5. CLI tests

| ID | Test | Procedure | Expectation |
|---|---|---|---|
| C01 | Default scope | `--check --quiet` without `--update` | Scope `existing` |
| C02 | `--update=existing` | one edition installed, one missing | only installed edition is checked/updated |
| C03 | `--update all` | same situation | missing edition reported as installable |
| C04 | `--update=all` | same situation | identical to C03 |
| C05 | `--check` without update | everything current | exit 0, no download |
| C06 | `--check` with update | newer version available | exit 5, no download |
| C07 | `--check` network error | network off | exit 4 |
| C08 | `--dry-run` | update available | plan is printed, no download |
| C09 | `--quiet` | perform update | no message box, no visible GUI |
| C10 | `--quit-after` | perform update | process exits after completion |
| C11 | `--launch-brave` one edition | only Stable x64 installed | launcher starts |
| C12 | `--launch-brave` multiple editions | Stable x64 and Beta x86 installed, no unique selection | exit 6, no start |
| C13 | `--launch-brave=stable:x64` | multiple editions installed | targeted start |
| C14 | `--launch-brave` without installation | no edition installed | exit 6 |
| C15 | Pass-through arguments | `--launch-brave=stable:x64 -- --incognito` | Brave receives `--incognito` |
| C16 | `--launch-brave` with `--check` | combination | exit 2 |
| C17 | Case-insensitivity | `--Update=Existing` | works |
| C18 | `--` without `--launch-brave` | `-- --incognito` | exit 2 |

## 6. Update integration tests

| ID | Test | Procedure | Expectation |
|---|---|---|---|
| I01 | Stable x64 update | fresh installation, update | new version in `Version.log`, `Brave.exe` starts |
| I02 | Stable x86 update | on x64 Windows | x86 edition updated |
| I03 | Beta x64 update | fresh installation | update succeeds |
| I04 | Nightly x86 update | fresh installation | update succeeds |
| I05 | `--force` | same version again | reinstall, exit 0 |
| I06 | No new version | `--update=existing` | no change, exit 0 |
| I07 | Brave running | start Brave, update | interactive dialog; `--quiet` exit 3 |
| I08 | Brave running with wait | `--wait-for-brave --timeout 10`, close Brave | update starts after close |
| I09 | Wait timeout | Brave running, timeout 1s | exit 3 |
| I10 | Missing `Bin/7zr.exe` | rename `Bin/7zr.exe` | clear error, exit 1 |
| I11 | Missing launcher | remove launcher EXE | fallback to `Brave.exe` or exit 6 |
| I12 | `Profile.txt` missing | `--launch-brave` | fallback to `Brave.exe --user-data-dir` |
| I13 | Self-update on fork | start | no upstream access; no replacement when version is equal |
| I14 | Network error during download | disconnect network | exit 4, no corrupt installation |
| I15 | Cancel/restart | interrupt update, start again | no broken state, rerun possible |

## 7. GUI regression tests

| ID | Test | Expectation |
|---|---|---|
| G01 | GUI starts | window appears, versions load |
| G02 | Single update via button | update works as before |
| G03 | "Install all" | works as before |
| G04 | Checkboxes | x86/x64/folder/version check/shortcut unchanged |
| G05 | Quit button | application closes |
| G06 | Brave running | previous warning appears |
| G07 | Language | DE/EN/RU unchanged |

## 8. Release tests

| ID | Test | Expectation |
|---|---|---|
| R01 | Package content | `Bin/7zr.exe`, nine launchers, updater EXE |
| R02 | LICENSE | MIT text and copyright included |
| R03 | 7-Zip notice | license notice for `7zr.exe` included |
| R04 | README disclaimer | Brave trademark disclaimer included |
| R05 | Versions | `AssemblyInfo`, `Version.txt`, `Launcher/Version.txt` consistent |
| R06 | Checksums | SHA256 for release assets present |
| R07 | Self-update URLs | point to fork or are disabled |
| R08 | Signature | optional: EXE signed, SmartScreen OK |
| R09 | Dev artifact | name contains timestamp and SHA; `BUILD-INFO.txt` present; no release/tag |
| R10 | `update_and_close.bat` | Runs `--update=existing --quit-after`; updater exits; exit code is returned |
| R11 | `update_and_run_brave.bat` | Runs with auto/explicit edition; `--launch-brave` starts Brave or returns exit 6 |
| R12 | Missing `Bin\7zr.exe` (GUI) | Immediate message naming `Bin\7zr.exe`; no download starts |
| R13 | Missing `Bin\7zr.exe` (CLI) | Exit 7; log names `Bin\7zr.exe`; no download starts |
| R14 | Missing launcher source | Message/log names `Bin\Launcher\<Edition> Launcher.exe` |
| R15 | PortableApps icons | ICO has 16, 24, 32, 48, 64, 128, 256 px frames at 32-bit; PNGs are 32-bit |
| R16 | Update entry icon | `Updater icon` and `Brave icon` produce the expected `appicon3` or updater-only icon; setting persists as `UpdateIcon` |
| R17 | Three menu entries | All selected: `Icons=3`, `Start1=PortableAppsUpdater...`, `Start2=PortableAppsStart...`, `Start3=PortableAppsUpdate...`; `appicon1..3` match |
| R18 | Menu descriptions | `Brave` shows `stable, x64`; `Brave (updated)` shows `Update Brave, then start Brave`; updater entry has no extra description |

## 9. Acceptance criteria

- T01-T08 and C01-C18 pass.
- At least I01, I02, I05, I06, I07, I08, I10, I13 pass.
- G01-G07 pass.
- R01-R18 pass.
- No open message boxes in `--quiet` mode.
- No upstream URLs in the release build, except where intentionally documented.

## 10. Known limitations

- The launcher may show a profile dialog on first start if `Profile.txt` is missing. Hence the fallback to `Brave.exe`.
- `--update=all` is bandwidth-intensive and takes time.
- Self-update is active but points to the fork; release assets are provided by the fork release.
- Dev builds are timestamped Actions artifacts and not a self-update channel.
- A real 32-bit Windows test is optional; Windows 11 is x64-only.
