# Portable Brave Updater 1.3.3.337

PortableApps integration and runtime-dependency release. Builds on the released `1.3.3.37` CLI.

## Highlights

- PortableApps integration dialog: generates `appinfo.ini`, icons, no-space start/update batches and a settings file.
- Existing `App\AppInfo`, generated batches and settings are backed up before writing.
- Launcher entries use the Brave icon; the update entry uses the updater icon.
- Generated batches avoid spaces and arguments in `appinfo.ini` `Start` values.
- The static `PortableAppsUpdate.bat` was removed. The app generates `PortableAppsStart-stable-x64.bat` and `PortableAppsUpdate-stable-x64.bat`.
- Transparent runtime dependency preflight for GUI and CLI. Missing files are reported by exact path before download or extraction.
- New CLI exit code `7` for missing runtime files.

## Validation

- Local build passed.
- CLI preflight tests passed for missing `Bin\7zr.exe` and missing launcher files.
- A real update download and Platform entry test remain manual.

## Notes

- Updater version is `1.3.3.337`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- `Portable Brave Updater.exe` is not standalone. Keep `Bin\7zr.exe` and `Bin\Launcher\`.
- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.3.3.337.7z`.

## Package

- Portable Brave Updater `1.3.3.337`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
- `update_and_close.bat`, `update_and_run_brave.bat`
