# Portable Brave Updater 1.3.3.7

Fork-Release mit .NET-4.8-Retargeting, Fork-URLs, neuem Build-/Release-Setup und aktualisierten Launchern.

## Highlights

- Updater und alle neun Launcher auf .NET Framework 4.8 aktualisiert.
- Update- und Launcher-URLs zeigen auf `yasuoiwakura/PorBraveUpd-fixes`.
- Updater und Launcher auf `1.3.3.7` angehoben; `Launcher.7z` neu gebaut.
- Reproduzierbarer Build mit `build.ps1`, `7zr.exe` und SHA256-Checksums.
- GitHub Actions: CI, zeitgestempelte Dev-Artefakte und tag-basiertes Release.

## Hinweis

- Die CLI-Parameter sind dokumentiert, aber noch nicht implementiert.
- Self-Update zeigt auf den Fork; das Release-Asset ist Teil dieser Version.
- Keine Code-Signierung; Self-Update ohne Hashprüfung.

## Paket

- Portable Brave Updater `1.3.3.7`
- `Bin/7zr.exe`
- `Bin/Launcher/*.exe` (`1.3.3.7`)
- `README.md`, `LICENSE`, `THIRD-PARTY-NOTICES.txt`
