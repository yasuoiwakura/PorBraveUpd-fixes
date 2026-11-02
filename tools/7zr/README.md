# 7zr.exe

Laufzeitabhaengigkeit des Portable Brave Updater zum Entpacken der Brave-Installer und von `Launcher.7z`.

| Merkmal | Wert |
|---|---|
| Datei | `7zr.exe` |
| Version | 7-Zip (r) 19.00 (x86), 2019-02-21 |
| SHA256 | `108AB5F1E36F2068E368FE97CD763C639E403CAC8F511C6681EAF19FC585D814` |
| Groesse | 523776 Bytes |
| Lizenz | Public Domain (LZMA SDK / 7-Zip) |
| Quelle | Offizielles Portable-Brave-Updater-Release v1.3.0.0, `Bin/7zr.exe` |

## Zweck

- Entpacken des Brave-Installers
- Entpacken von `Chrome.7z`
- Erzeugen und Pruefen von `Launcher.7z` im Build

## Aktualisierung

Wenn `7zr.exe` ersetzt wird:

1. Neue Datei von einer vertrauenswuerdigen Quelle beziehen.
2. SHA256 in dieser Datei und in `THIRD-PARTY-NOTICES.txt` aktualisieren.
3. Build und Smoke-Test erneut ausfuehren.
