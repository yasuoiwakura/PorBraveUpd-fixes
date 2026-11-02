# Portable Brave Updater 1.3.3.37

CLI release for the fork, based on the .NET 4.8 and fork-URL work from 1.3.3.7.

## Highlights

- CLI parameters: `--update`, `--launch-brave`, `--quit-after`, `--if-new`, `--force`, `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave`, `--timeout`, `--help` and `--version`.
- Headless execution with no GUI dialogs and defined exit codes.
- Supports both single-folder (`Brave`) and per-version (`Brave <Channel> <Arch>`) installations.
- `--check` and `--dry-run` do not download or install anything.
- `--launch-brave` starts the matching launcher and falls back to `Brave.exe` if needed.
- `--wait-for-brave` and `--timeout` support unattended update jobs.

## Validation

- Local build passed.
- CLI smoke tests passed for `--help`, `--version`, invalid arguments, `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave` and the no-installation cases.
- A real update download and an actual Brave launch were not tested end-to-end because Brave was running during the local test.

## Notes

- Updater version is `1.3.3.37`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.3.3.37.7z`.

## Package

- Portable Brave Updater `1.3.3.37`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
