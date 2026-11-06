# Portable Brave Updater 1.4.10

Incremental release on top of `1.3.37`. Replaces the withdrawn interim versions
`1.33.7` and `1.33.37`; all changes after `1.3.37` are attributed to this release.

## Highlights (delta over 1.3.37)

- A green (up-to-date) edition button now starts the matching Brave edition when Brave is not already running, instead of only showing the "same version" message.
- Fixed the inverted single-folder/per-version flag behind that launch path: in single-folder installs the button looked for `Brave Stable x64 Launcher.exe` / `Brave Stable x64\Brave.exe`, both missing, and showed "Brave konnte nicht gestartet werden." without details. The CLI `--launch-brave` path was unaffected because it auto-detects the layout. Launch failures now list the exact paths that were checked.
- Generated `appinfo.ini`: `Start`/`StartN` for the updater and launcher entries point directly at `Portable Brave Updater.exe` and the launcher EXE (unquoted). `PortableAppsStart-stable-x64.bat` and `PortableAppsUpdater-stable-x64.bat` are no longer generated. Only the update-then-start entry keeps `PortableAppsUpdate-stable-x64.bat` because it passes arguments.
- PortableApps generation no longer backs up previous files by default; an optional unchecked dialog checkbox ("Backup der bisherigen Dateien (nicht empfohlen)") enables it.
- Revalidated against the PAF 3.9 spec: `[Control]` fields must not contain double quotes; unquoted values with spaces are fine. The earlier live failure was caused by quotes and arguments, not spaces. Legacy start batches are backed up and not regenerated.
- New `release-help.bat`: checks branch, `Version.txt` and dirty working tree, then prints the correct release steps.
- New `nightly` workflow: builds `dev` daily (03:00 UTC) only when dev changed since the last successful nightly; `workflow_dispatch` can force a build.
- CI no longer auto-builds on `master`/`dev` pushes; builds run on pull requests and manual dispatch.
- `build.yml` accepts `ref` and `channel` inputs; `BUILD-INFO.txt` now records commit SHA, ref and channel.
- Version bumped from `1.3.37` to `1.4.10`.

## Validation

- Local build passed; `FileVersion` is `1.4.10`.
- A real update download, a real Brave launch from the green button, and launching the
  direct EXE `Start` entries from the PortableApps.com Platform remain manual tests.

## Notes

- Updater version is `1.4.10`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- `Portable Brave Updater.exe` is not standalone. Keep `Bin\7zr.exe` and `Bin\Launcher\`.
- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.4.10.7z`.

## Package

- Portable Brave Updater `1.4.10`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
- `update_and_close.bat`, `update_and_run_brave.bat`
