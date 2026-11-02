# 7zr.exe

Runtime dependency of the Portable Brave Updater for extracting Brave installers and `Launcher.7z`.

| Item | Value |
|---|---|
| File | `7zr.exe` |
| Version | 7-Zip (r) 19.00 (x86), 2019-02-21 |
| SHA256 | `108AB5F1E36F2068E368FE97CD763C639E403CAC8F511C6681EAF19FC585D814` |
| Size | 523776 bytes |
| License | Public domain (LZMA SDK / 7-Zip) |
| Source | Official Portable Brave Updater v1.3.0.0 release, `Bin/7zr.exe` |

## Purpose

- Extract the Brave installer
- Extract `Chrome.7z`
- Create and verify `Launcher.7z` during the build

## Updating

When replacing `7zr.exe`:

1. Obtain the new file from a trusted source.
2. Update the SHA256 in this file and in `THIRD-PARTY-NOTICES.txt`.
3. Run the build and smoke test again.
