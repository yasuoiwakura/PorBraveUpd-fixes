# Changelog

Alle nennenswerten Änderungen an diesem Fork.

## 1.3.3.7 - 2026-11-02

Erstes Fork-Release auf Basis von Portable Brave Updater 1.3.0.0.

### Geändert

- Update- und Launcher-URLs auf `yasuoiwakura/PorBraveUpd-fixes` umgestellt.
- Updater und alle neun Launcher auf .NET Framework 4.8 retargetet.
- Updater und Launcher auf Version `1.3.3.7` angehoben.
- `Launcher/Launcher.7z` mit den neuen 1.3.3.7-Launchern neu erzeugt.
- Self-Update prüft und lädt jetzt gegen den Fork.

### Build und Release

- `build.ps1` für Build, Launcher-Prüfung, 7z-Paketierung und SHA256-Checksums.
- `tools/7zr/7zr.exe` inklusive Drittlizenzhinweis.
- GitHub Actions: CI für `master`/`dev`/PRs, zeitgestempelte Dev-Artefakte und tag-basiertes Release.
- Dev-Builds benötigen nur lesende Rechte und erzeugen keine GitHub Releases.

### Dokumentation

- Analyse, Build, CLI-Spezifikation, Architektur, Testplan, Release/Lizenz, Best Practices, Installationsprotokoll und ADRs.

### Hinweis

- Die CLI-Parameter sind spezifiziert, aber noch nicht implementiert.
- Self-Update-Assets werden mit diesem Release bereitgestellt.
- Keine Code-Signierung; Self-Update ohne Hashprüfung.
