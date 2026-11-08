# Changelog

All notable changes to this fork.

## [Unreleased]

### Changed

- Self-update: when the online version is older than the installed one, show a red inline warning instead of no feedback; the update controls stay visible but the update button is disabled.
- Version comparison now normalizes the component count, so `1.4.10` and `1.4.10.0` compare as equal.
- Bumped updater version to `1.4.11`.

## 1.4.10 - 2026-11-06

Incremental release on top of `1.3.37`. Replaces the withdrawn interim versions `1.33.7` and `1.33.37`; all changes after `1.3.37` are attributed to this release.

### Added

- A green (up-to-date) edition button starts the matching Brave edition if Brave is not already running, instead of showing the "same version" message.
- New `release-help.bat`: checks branch, `Version.txt` and dirty working tree, then prints the correct release steps.
- New nightly workflow builds `dev` daily (03:00 UTC) only when dev changed since the last successful nightly; manual dispatch can force a build.

### Changed

- Updater version is `1.4.10`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- CI no longer auto-builds on `master`/`dev` pushes; pull requests and manual dispatch still build.
- `build.yml` accepts `ref` and `channel` inputs; `BUILD-INFO.txt` now records commit SHA, ref and channel.
- Generated `appinfo.ini` no longer uses start batches: `Start`/`StartN` for the updater and launcher entries point directly at the EXEs (unquoted; spaces are allowed, double quotes are forbidden per PAF 3.9). Only the update-then-start entry keeps a batch because it passes arguments. Legacy start batches are backed up and not regenerated.
- PortableApps generation no longer backs up previous files by default; an optional unchecked dialog checkbox ("Backup der bisherigen Dateien (nicht empfohlen)") enables it.

### Fixed

- Green-button launch used an inverted single-folder/per-version flag: in single-folder installs the button looked for `Brave Stable x64` paths and failed with "Brave konnte nicht gestartet werden." without details. The CLI `--launch-brave` path (auto-detected layout) was unaffected. Launch failures now list the paths that were checked.

### Validation

- Local build passed. Real update download, green-button launch and Platform menu launch remain manual tests.

### Notes

- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.4.10.7z`.

## 1.3.37 - 2026-11-02

PortableApps integration and runtime-dependency release. Builds on the released `1.3.3.37` CLI.

### Added

- PortableApps integration dialog: generates `appinfo.ini`, icons, no-space start/update batches and a settings file with backup of existing files.
- Transparent runtime dependency preflight for GUI and CLI. Missing files are reported by exact path before download or extraction.

### Changed

- Updater version is `1.3.37`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- PortableApps dialog, icons, menu entries and descriptions were added (see release notes).
- `DisplayVersion` is omitted from the generated appinfo; `PackageVersion` remains.

### Validation

- Local build and CLI preflight tests passed. Platform entry tests remain manual.

### Notes

- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.3.37.7z`.

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
