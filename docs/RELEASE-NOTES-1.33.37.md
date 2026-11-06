# Portable Brave Updater 1.33.37

Incremental release on top of `1.33.7`. It fixes the green-button launch failure in
single-folder installs and drops the start batches from the generated PortableApps
`appinfo.ini` where they are not needed.

## Highlights (delta over 1.33.7)

- Fixed: a green (up-to-date) edition button passed an inverted single-folder/per-version
  flag to the launch path. In single-folder installs the button looked for
  `Brave Stable x64 Launcher.exe` / `Brave Stable x64\Brave.exe`, both missing, and showed
  "Brave konnte nicht gestartet werden." without details. The CLI `--launch-brave` path was
  unaffected because it auto-detects the layout.
- Launch failures now list the exact paths that were checked.
- Generated `appinfo.ini`: `Start`/`StartN` for the updater and launcher entries now point
  directly at `Portable Brave Updater.exe` and the launcher EXE (unquoted). `PortableAppsStart-stable-x64.bat`
  and `PortableAppsUpdater-stable-x64.bat` are no longer generated. Only the update-then-start
  entry keeps `PortableAppsUpdate-stable-x64.bat` because it passes arguments.
- Revalidated against the PAF 3.9 spec: `[Control]` fields must not contain double quotes;
  unquoted values with spaces are fine. The earlier live failure was caused by quotes and
  arguments, not spaces. Legacy start batches are backed up and not regenerated.
- Version bumped from `1.33.7` to `1.33.37`.

## Validation

- Local build passed; `FileVersion` is `1.33.37`.
- Regenerating the live `appinfo.ini` and launching the direct EXE `Start` entries from the
  PortableApps.com Platform remain manual tests.

## Notes

- Updater version is `1.33.37`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- `Portable Brave Updater.exe` is not standalone. Keep `Bin\7zr.exe` and `Bin\Launcher\`.
- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.33.37.7z`.

## Package

- Portable Brave Updater `1.33.37`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
- `update_and_close.bat`, `update_and_run_brave.bat`
