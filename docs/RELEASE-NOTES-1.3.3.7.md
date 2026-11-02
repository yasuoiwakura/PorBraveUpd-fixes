# Portable Brave Updater 1.3.3.7

Fork release with .NET 4.8 retargeting, fork URLs, a new build/release setup and updated launchers.

## Highlights

- Updater and all nine launchers updated to .NET Framework 4.8.
- Update and launcher URLs now point to `yasuoiwakura/PorBraveUpd-fixes`.
- Updater and launchers bumped to `1.3.3.7`; `Launcher.7z` rebuilt.
- Reproducible build with `build.ps1`, `7zr.exe` and SHA256 checksums.
- GitHub Actions: CI, timestamped dev artifacts and tag-based releases.

## Notes

- CLI parameters are documented but not implemented yet.
- Self-update points to the fork; the release asset is part of this version.
- No code signing; self-update without hash verification.

## Package

- Portable Brave Updater `1.3.3.7`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
