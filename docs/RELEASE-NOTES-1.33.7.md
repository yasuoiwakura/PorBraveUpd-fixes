# Portable Brave Updater 1.33.7

Incremental release on top of `1.3.37`. The PortableApps integration dialog and the
runtime dependency preflight shipped with `1.3.37` (see RELEASE-NOTES-1.3.37.md);
this release covers launch behavior, release tooling and CI workflows only.

## Highlights (delta over 1.3.37)

- A green (up-to-date) edition button now starts the matching Brave edition when Brave is not already running, instead of only showing the "same version" message.
- New `release-help.bat`: checks branch, `Version.txt` and dirty working tree, then prints the correct release steps.
- New `nightly` workflow: builds `dev` daily (03:00 UTC) only when dev changed since the last successful nightly; `workflow_dispatch` can force a build.
- CI no longer auto-builds on `master`/`dev` pushes; builds run on pull requests and manual dispatch.
- `build.yml` accepts `ref` and `channel` inputs; `BUILD-INFO.txt` now records commit SHA, ref and channel.
- Version bumped from `1.3.37` to `1.33.7`.

## Validation

- Local build passed.
- A real update download and a real Brave launch from the green button remain manual tests.

## Notes

- Updater version is `1.33.7`.
- Launchers remain at `1.3.3.7`; `Launcher.7z` is unchanged.
- `Portable Brave Updater.exe` is not standalone. Keep `Bin\7zr.exe` and `Bin\Launcher\`.
- No code signing; self-update without hash verification.
- Release asset: `Portable.Brave.Updater.v1.33.7.7z`.

## Package

- Portable Brave Updater `1.33.7`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
- `update_and_close.bat`, `update_and_run_brave.bat`
