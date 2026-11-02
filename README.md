# PorBraveUpd
 Portable Brave Updater

## About this fork

This is an **unofficial fork** of [UndertakerBen/PorBraveUpd](https://github.com/UndertakerBen/PorBraveUpd),
maintained at [yasuoiwakura/PorBraveUpd-fixes](https://github.com/yasuoiwakura/PorBraveUpd-fixes).

Many thanks to UndertakerBen for the original project, which is MIT-licensed and was
the foundation for this fork. Upstream has seen no commits since 2022; this fork keeps
the project usable for current Brave versions.

Changes in this fork:

- Retargeted to .NET Framework 4.8.
- Added a non-interactive CLI for pre-launcher and automated use.
- Self-update and release assets point to this fork.
- Updated build, CI and documentation.

This fork is not affiliated with, authorized, or endorsed by the original author.

## Package layout

`Portable Brave Updater.exe` is not a standalone executable. Keep the package layout:

```text
Portable Brave Updater.exe
Portable Brave Updater.exe.config
Bin\
├─ 7zr.exe
└─ Launcher\
   ├─ Brave Launcher.exe
   ├─ Brave Stable x64 Launcher.exe
   └─ ...
```

If only the EXE is copied, the updater can download the Brave installer but cannot unpack it. The GUI and CLI now report the exact missing path before downloading, for example `Bin\7zr.exe`. Use the full release package, not just the EXE.

## Notice of Non-Affiliation and Disclaimer
PorBraveUpd is not affiliated, associated, authorized, endorsed by, or in any way officially connected with Brave™, or any of its subsidiaries or its affiliates.
The name Brave™ as well as related names, marks, emblems and images are registered trademarks of their respective owners.

For unpacking the downloaded Brave offline installer i use 7zr.exe

- 7zr.exe - https://www.7-zip.org/ (Public Domain)

## Releases

See [CHANGELOG.md](CHANGELOG.md) and [docs/RELEASES.md](docs/RELEASES.md).
