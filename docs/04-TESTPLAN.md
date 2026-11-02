# 04 - Testplan

Stand: 2026-11-02

## 1. Testziele

- Bestehende GUI-Funktion bleibt erhalten.
- CLI verhaelt sich gemaess `02-CLI-SPEC.md`.
- Update-Pfade funktionieren fuer x86 und x64.
- Fehlerfaelle liefern saubere Exit-Codes und keine blockierenden Dialoge.
- Ein Release-Paket ist vollstaendig und lizenzkonform.

## 2. Testumgebung

- Windows 11 x64 (primaer).
- Optional Windows 10 x64 fuer Kompatibilitaet.
- Testverzeichnis, nicht die produktive Installation.
- Vor Update-Tests: Backup der Edition bzw. frische Kopie.
- Netzwerkzugriff auf `updates.bravesoftware.com` und GitHub.
- Fuer Netzwerkfehler: Netzwerk deaktivieren oder Proxy auf nicht erreichbar setzen.

## 3. Teststufen

| Stufe | Inhalt | Automatisierbar |
|---|---|---|
| Build | Hauptprojekt und Launcher bauen | ja |
| Parser | Argumente, Defaults, Fehler | ja |
| Smoke | `--help`, `--version`, `--check` | teilweise |
| Integration | echtes Update einer Edition | nein, VM/manuell |
| Regression | interaktiver GUI-Modus | manuell |
| Release | Paket, Lizenzen, Checksums | teilweise |

## 4. Basistests

| ID | Test | Vorgehen | Erwartung |
|---|---|---|---|
| T01 | Hauptprojekt bauen | `msbuild ... /p:Configuration=Release` | Build erfolgreich |
| T02 | Launcher bauen | alle neun Launcher-Solutions | Build erfolgreich |
| T03 | `--help` | EXE mit `--help` starten | Hilfe, Exit 0 |
| T04 | `--version` | EXE mit `--version` starten | Version, Exit 0 |
| T05 | Ungueltiges Argument | `--foo` | Fehlermeldung, Exit 2 |
| T06 | `--update` ohne Wert | `--update` | Fehlermeldung, Exit 2 |
| T07 | `--timeout` ohne `--wait-for-brave` | `--timeout 10` | Fehlermeldung, Exit 2 |
| T08 | `--if-new --force` | beide Flags | Fehlermeldung, Exit 2 |

## 5. CLI-Tests

| ID | Test | Vorgehen | Erwartung |
|---|---|---|---|
| C01 | Default-Scope | `--check --quiet` ohne `--update` | Scope `existing` |
| C02 | `--update=existing` | eine Edition installiert, eine fehlt | nur installierte wird geprueft/aktualisiert |
| C03 | `--update all` | gleiche Situation | fehlende Edition wird als installierbar gemeldet |
| C04 | `--update=all` | gleiche Situation | identisch zu C03 |
| C05 | `--check` ohne Update | alles aktuell | Exit 0, kein Download |
| C06 | `--check` mit Update | neuere Version verfuegbar | Exit 5, kein Download |
| C07 | `--check` Netzwerkfehler | Netzwerk aus | Exit 4 |
| C08 | `--dry-run` | Update verfuegbar | Plan wird ausgegeben, kein Download |
| C09 | `--quiet` | Update durchfuehren | keine MessageBox, keine sichtbare GUI |
| C10 | `--quit-after` | Update durchfuehren | Prozess endet nach Abschluss |
| C11 | `--launch-brave` eine Edition | nur Stable x64 installiert | Launcher wird gestartet |
| C12 | `--launch-brave` mehrere Editionen | Stable x64 und Beta x86 installiert, keine eindeutige Auswahl | Exit 6, kein Start |
| C13 | `--launch-brave=stable:x64` | mehrere Editionen installiert | gezielter Start |
| C14 | `--launch-brave` ohne Installation | keine Edition installiert | Exit 6 |
| C15 | Durchreichen von Argumenten | `--launch-brave=stable:x64 -- --incognito` | Brave erhaelt `--incognito` |
| C16 | `--launch-brave` mit `--check` | Kombination | Exit 2 |
| C17 | Gross-/Kleinschreibung | `--Update=Existing` | funktioniert |
| C18 | `--` ohne `--launch-brave` | `-- --incognito` | Exit 2 |

## 6. Update-Integrationstests

| ID | Test | Vorgehen | Erwartung |
|---|---|---|---|
| I01 | Stable x64 Update | frische Installation, Update | neue Version in `Version.log`, `Brave.exe` startbar |
| I02 | Stable x86 Update | auf x64-Windows | x86-Edition aktualisiert |
| I03 | Beta x64 Update | frische Installation | Update erfolgreich |
| I04 | Nightly x86 Update | frische Installation | Update erfolgreich |
| I05 | `--force` | gleiche Version erneut | Neuinstallation, Exit 0 |
| I06 | Keine neue Version | `--update=existing` | keine Aenderung, Exit 0 |
| I07 | Brave laeuft | Brave starten, Update | interaktiv Dialog; `--quiet` Exit 3 |
| I08 | Brave laeuft mit Warten | `--wait-for-brave --timeout 10`, Brave beenden | Update startet nach Beenden |
| I09 | Warte-Timeout | Brave laeuft, Timeout 1s | Exit 3 |
| I10 | Fehlendes `Bin/7zr.exe` | `Bin/7zr.exe` umbenennen | klarer Fehler, Exit 1 |
| I11 | Fehlender Launcher | Launcher-EXE entfernen | Fallback auf `Brave.exe` oder Exit 6 |
| I12 | `Profile.txt` fehlt | `--launch-brave` | Fallback auf `Brave.exe --user-data-dir` |
| I13 | Self-Update auf Fork | Start | kein Upstream-Zugriff; bei gleicher Version keine Ersetzung |
| I14 | Netzwerkfehler beim Download | Netzwerk trennen | Exit 4, keine korrupte Installation |
| I15 | Abbruch/Neustart | Update unterbrechen, erneut starten | kein kaputter Zustand, erneuter Lauf moeglich |

## 7. Regressionstests GUI

| ID | Test | Erwartung |
|---|---|---|
| G01 | GUI startet | Fenster erscheint, Versionen werden geladen |
| G02 | Einzel-Update ueber Button | Update funktioniert wie vorher |
| G03 | "Alle installieren" | funktioniert wie vorher |
| G04 | Checkboxen | x86/x64/Ordner/Versionkontrolle/Shortcut unveraendert |
| G05 | Beenden-Button | Anwendung schliesst |
| G06 | Brave laeuft | bisherige Warnung erscheint |
| G07 | Sprache | DE/EN/RU unveraendert |

## 8. Release-Tests

| ID | Test | Erwartung |
|---|---|---|
| R01 | Paketinhalt | `Bin/7zr.exe`, neun Launcher, Updater-EXE |
| R02 | LICENSE | MIT-Text und Copyright enthalten |
| R03 | 7-Zip-Hinweis | Lizenzhinweis fuer `7zr.exe` enthalten |
| R04 | README-Disclaimer | Brave-Marken-Disclaimer enthalten |
| R05 | Versionen | `AssemblyInfo`, `Version.txt`, `Launcher/Version.txt` konsistent |
| R06 | Checksums | SHA256 fuer Release-Assets vorhanden |
| R07 | Self-Update-URLs | zeigen auf Fork oder sind deaktiviert |
| R08 | Signatur | optional: EXE signiert, SmartScreen ok |
| R09 | `dev-latest` Pre-Release | `Version.txt` bleibt unveraendert; Updater bietet dev-latest nicht an |

## 9. Abnahmekriterien

- T01-T08 und C01-C18 bestehen.
- Mindestens I01, I02, I05, I06, I07, I08, I10, I13 bestehen.
- G01-G07 bestehen.
- R01-R09 bestehen.
- Keine offenen MessageBoxen im `--quiet`-Modus.
- Keine Upstream-URLs im Release-Build, ausser bewusst dokumentiert.

## 10. Bekannte Einschraenkungen

- Launcher kann beim ersten Start einen Profil-Dialog zeigen, wenn `Profile.txt` fehlt. Deshalb der Fallback auf `Brave.exe`.
- `--update=all` ist bandwidthintensiv und braucht Zeit.
- Self-Update ist aktiv, aber auf den Fork umgestellt; Release-Assets fehlen noch.
- `dev-latest` ist ein manuell herunterladbarer Pre-Release und kein Self-Update-Kanal.
- Ein echter 32-Bit-Windows-Test ist optional; Windows 11 ist x64-only.
