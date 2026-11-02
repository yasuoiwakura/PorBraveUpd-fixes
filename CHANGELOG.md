# Changelog

All notable changes to this fork.

## 1.3.3.7 - 2026-11-02

First fork release based on Portable Brave Updater 1.3.0.0.

### Changed

- Switched update and launcher URLs to `yasuoiwakura/PorBraveUpd-fixes`.
- Retargeted the updater and all nine launchers to .NET Framework 4.8.
- Bumped updater and launchers to version `1.3.3.7`.
- Rebuilt `Launcher/Launcher.7z` with the new 1.3.3.7 launchers.
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

- CLI parameters are specified but not implemented yet.
- Self-update assets are provided by this release.
- No code signing; self-update without hash verification.
