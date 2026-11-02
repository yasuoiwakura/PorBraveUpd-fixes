# 05 - Release und Lizenz

Stand: 2026-11-02

## 1. MIT-Lizenz

Der Upstream steht unter der MIT-Lizenz (Copyright (c) 2020 UndertakerBen). MIT erlaubt:

- Nutzung und Kopie
- Aenderung und Zusammenfuehrung
- Veroeffentlichung und Verbreitung
- Unterlizenzierung
- Verkauf, auch kommerziell

Bedingung: Der Copyright-Hinweis und der MIT-Lizenztext muessen in allen Kopien bzw. wesentlichen Teilen enthalten bleiben.

### Pflichten fuer den Fork

- `LICENSE` unveraendert mitliefern.
- Eigenes Copyright ergaenzen, z. B. `Copyright (c) 2026 yasuoiwakura`.
- Im Release-Paket und im Repository auf die MIT-Lizenz hinweisen.
- Bei Aenderungen klarstellen, dass es ein inoffizieller Fork ist.

## 2. Marken und Brave

MIT uebertraegt keine Markenrechte.

- "Brave", Brave-Logos und zugehoerige Icons sind Marken der jeweiligen Inhaber.
- Der bestehende README-Disclaimer muss erhalten bleiben.
- Keine Formulierung, die eine offizielle Verbindung zu Brave suggeriert.
- Fuer eine breite Veroeffentlichung kann es sinnvoll sein, Brave-Icons zu ersetzen oder klar als inoffiziellen Fork zu kennzeichnen.

## 3. 7-Zip / 7zr.exe

Das Release enthaelt `Bin/7zr.exe`.

- `7zr.exe` aus dem LZMA SDK ist public domain.
- `7zr.exe` aus der 7-Zip-Distribution steht unter GNU LGPL; bei Weitergabe muessen die Lizenzinformationen reproduziert werden.
- Empfehlung: `7zr.exe` aus dem LZMA SDK verwenden und einen kurzen Drittlizenz-Hinweis beilegen.
- Unabhaengig davon: 7-Zip-Lizenztext im Release unter `THIRD-PARTY-NOTICES.txt` ablegen.

## 4. Brave-Installer und Nutzungsbedingungen

Der Updater laedt Brave-Installer zur Laufzeit von Brave-Servern. Das ist bestehendes Verhalten des Upstreams.

Fuer eine oeffentliche Verbreitung:

- Braves Nutzungsbedingungen pruefen.
- Keine Brave-Installer im eigenen Release mitverteilen, wenn nicht ausdruecklich erlaubt.
- Keine Aussagen treffen, die eine offizielle Genehmigung implizieren.

Dies ist keine Rechtsberatung.

## 5. Self-Update- und Update-URLs

Die GitHub-URLs sind seit 2026-11-02 im Code zentralisiert und zeigen auf den Fork:

- `Form1.cs`: `RepositoryUrl` = `https://github.com/yasuoiwakura/PorBraveUpd-fixes`
- `Form1.cs`: `RawMasterUrl` = `https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master`

Verwendet fuer:

- `Version.txt` (Self-Update und Update-Button)
- `Launcher/Version.txt`
- `Launcher/Launcher.7z`
- Release-Asset-Download

Wichtig:

- Self-Update ist funktional noch aktiv, aber auf den Fork umgestellt.
- Der Fork hat aktuell keine Releases. Die neue EXE und `Version.txt` stehen auf 1.3.3.7; ein 1.3.3.7-Build bietet daher kein Self-Update an. Aeltere 1.3.0.0-Builds wuerden erst nach dem Release-Asset erfolgreich aktualisieren.
- Vor dem ersten Versionsbump muss ein Release-Asset `Portable.Brave.Updater.v<Version>.7z` unter dem Fork existieren, sonst schlaegt der Self-Update-Download fehl.
- Fuer den Prototyp kann Self-Update spaeter deaktiviert werden.

Fuer ein Release:

- Release-Asset-Namen konsistent halten: `Portable.Brave.Updater.v<Version>.7z`.
- `Version.txt` im Repo-Root aktualisieren.
- `Launcher/Version.txt` aktualisieren, falls Launcher neu gebaut werden.
- Self-Update optional mit Hashpruefung absichern.

## 6. Versionierung

Konsistent halten:

| Ort | Bedeutung |
|---|---|
| `Properties/AssemblyInfo.cs` | Dateiversion der Updater-EXE |
| `Version.txt` | Vom Self-Update gelesene Version |
| `Launcher/Version.txt` | Launcher-Update-Version, aktuell `1.3.3.7` |
| `Launcher/Launcher.7z` | Launcher-Archiv fuer `CheckLauncher()` |
| Release-Tag | z. B. `v1.4.0.0` |
| Release-Asset | `Portable.Brave.Updater.v1.4.0.0.7z` |
| `dev-latest` | Pre-Release fuer Dev-Builds, nicht self-update-faehig |

Empfehlung: Semantische Versionierung bzw. die bestehende vierstellige .NET-Version beibehalten.

Wichtig:

- Der Updater und die Launcher stehen aktuell auf `1.3.3.7`.
- Vor einem stabilen Release muessen `Version.txt` und `AssemblyFileVersion` der Updater-EXE auf dieselbe neue Version gesetzt werden.
- `dev-latest` darf `Version.txt` niemals aendern.

## 7. Release-Checkliste

### Vor dem Build

- [ ] Ziel-Framework auf .NET Framework 4.8 gesetzt.
- [ ] Alle Launcher auf 4.8 retargetet oder bewusst vorkompiliert uebernommen.
- [ ] Self-Update deaktiviert oder auf Fork-URLs umgestellt.
- [ ] Versionsnummern konsistent.
- [ ] `LICENSE` und Copyright aktuell.
- [ ] README-Disclaimer vorhanden.

### Build und Test

- [ ] Hauptprojekt baut in Release.
- [ ] Launcher bauen oder Release-Launcher uebernehmen.
- [ ] `Launcher/Launcher.7z` mit den neuen Launchern neu erzeugt.
- [ ] `Launcher/Version.txt` passt zur Launcher-Version (`1.3.3.7`).
- [ ] `Bin/7zr.exe` vorhanden.
- [ ] Smoke-Test: `--help`, `--version`, `--check`.
- [ ] Update-Test einer Edition.
- [ ] GUI-Regressionstest.
- [ ] Testplan (`04-TESTPLAN.md`) relevant bestanden.

### Paket

- [ ] `Portable Brave Updater.exe`
- [ ] `Bin/7zr.exe`
- [ ] `Bin/Launcher/*.exe`
- [ ] `LICENSE`
- [ ] `README.md`
- [ ] `THIRD-PARTY-NOTICES.txt`
- [ ] Optional: `CHANGELOG.md`

### Veroeffentlichung

- [ ] SHA256-Checksums erzeugen.
- [ ] Optional: EXE mit Authenticode signieren.
- [ ] GitHub Release mit Tag und Asset erstellen.
- [ ] Tag zeigt auf `master`; `Version.txt` auf `master` ist aktuell.
- [ ] Release-Notes schreiben.
- [ ] `Version.txt` nur fuer stabile Releases aktualisieren.
- [ ] `dev-latest` nur als Pre-Release veroeffentlichen; `Version.txt` unveraendert lassen.
- [ ] Self-Update-Pfad testen, falls aktiviert.

## 8. Sicherheit

- Keine ungepruefte Selbstersetzung.
- Wenn Self-Update aktiviert wird:
  - HTTPS erzwingen,
  - Hash (SHA256) pruefen,
  - optional Signatur pruefen,
  - keine Ausfuehrung bei Hash-Mismatch.
- Brave-Installer optional per Authenticode pruefen.
- Keine Admin-Rechte anfordern.
- Keine Secrets loggen.

## 9. Drittlizenzen

| Komponente | Lizenz | Hinweis |
|---|---|---|
| PorBraveUpd | MIT | `LICENSE` |
| 7zr.exe | LZMA SDK public domain oder 7-Zip LGPL | Drittlizenz-Hinweis |
| Brave | Brave-Marken und -Bedingungen | Disclaimer, keine offizielle Verbindung |
| .NET Framework | Microsoft | Laufzeit-Bestandteil von Windows |

## 10. Referenzen

- MIT License: https://opensource.org/license/mit
- 7-Zip License: https://7-zip.org/license.txt
- .NET Framework Support: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
