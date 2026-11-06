# Changelog

All notable changes to this fork.

## 1.33.7 - 2026-11-02

PortableApps integration and runtime-dependency release.

### Added

- PortableApps integration dialog: generates `appinfo.ini`, icons, no-space start/update batches and a settings file with backup of existing files.
- Transparent runtime dependency preflight for GUI and CLI. Missing files are reported by exact path before download or extraction.

### Changed

- Updater version is `1.33.7`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- The static `PortableAppsUpdate.bat` was removed. The app now generates `PortableAppsUpdate-stable-x64.bat` and `PortableAppsStart-stable-x64.bat`.
- PortableApps icons use the Brave icon for the launcher entries and the updater icon for the update entry.
- PortableApps ICO files now contain multi-resolution 32-bit frames (16-256 px) extracted from the high-resolution EXE icons, fixing pixelated and low-color-depth menu icons.
- PortableApps dialog now has three menu entries: `Portable Brave Updater`, `Brave` and `Brave (newest stable)`; the icon dropdown sits directly next to `Brave (newest stable)`. The update entry name is derived from the selected edition.
- PortableApps dialog lets you choose the update entry icon (`Updater icon` or `Brave icon`). The choice is saved as `UpdateIcon` in `PortableBraveUpdater.ini`.
- Menu icons follow the selected entries: `appicon` for the main entry and `appicon1..N` for `Start1..N`.
- Menu descriptions clarify the Brave entries: `stable, x64` for `Brave` and `Update Brave, then start Brave` for `Brave (newest stable)`.
- `DisplayVersion` is omitted from the generated appinfo so the Platform does not show the updater version beside the Brave entries; `PackageVersion` remains.
- CI now runs for pull requests and manual dispatch; `dev`/`master` pushes no longer start an automatic build.
- New nightly workflow builds `dev` only when changes exist since the last successful nightly; manual dispatch can force a build.
- A green (up-to-date) edition button now starts the matching Brave edition if Brave is not already running, instead of showing the "same version" message.
- Shortened the PortableApps dialog text and moved the experimental/tracking note into a compact warning.

### Validation

- Local build and CLI preflight tests passed. Platform entry tests remain manual.

### Notes

- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.33.7.7z`.

## 1.3.3.37 - 2026-11-02

CLI release for the fork.

### Added

- CLI parameters: `--update=existing|all`, `--launch-brave[=channel:arch]`, `--quit-after`, `--if-new`, `--force`, `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave`, `--timeout`, `--help` and `--version`.
- Headless execution with exit codes and no GUI dialogs.
- Detection of both single-folder (`Brave`) and per-version (`Brave <Channel> <Arch>`) installations.
- `--check` and `--dry-run` without downloads.
- Optional log file and wait-for-Brave handling.
- `--launch-brave` support for the existing launcher with fallback to `Brave.exe`.
- Batch test scripts `update_and_close.bat` and `update_and_run_brave.bat` included in the package.

### Changed

- `Program.Main` now parses arguments, attaches to the parent console and returns the CLI exit code.
- `Form1` supports a hidden headless mode and routes update errors to the CLI.
- Updater version is `1.3.3.37`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.

### Validation

- Local build and CLI smoke tests passed for `--help`, `--version`, invalid arguments, `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave` and the no-installation cases.
- A real update download and an actual Brave launch were not tested end-to-end because Brave was running during the local test.

### Notes

- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.3.3.37.7z`.

## 1.3.3.7 - 2026-11-02

Build and retargeting preparation for the fork.

### Changed

- Switched update and launcher URLs to `yasuoiwakura/PorBraveUpd-fixes`.
- Retargeted the updater and all nine launchers to .NET Framework 4.8.
- Set launcher version to `1.3.3.7` and rebuilt `Launcher/Launcher.7z`.
- Self-update now checks and downloads from the fork.

### Build and release

- `build.ps1` for build, launcher validation, 7z packaging and SHA256 checksums.
- `tools/7zr/7zr.exe` including third-party notice.
- GitHub Actions: CI for `master`/`dev`/PRs, timestamped dev artifacts and tag-based releases.
- Dev builds require read-only permissions and do not create GitHub Releases.

### Documentation

- Analysis, build, CLI specification, architecture, test plan, release/license, best practices, installation log and ADRs.
- Documentation is written in English.

### Notes

- This version is superseded by `1.3.3.37`.
- No code signing; self-update without hash verification.
