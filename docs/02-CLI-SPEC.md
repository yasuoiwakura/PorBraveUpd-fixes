# 02 - CLI-Spezifikation

Stand: 2026-11-02  
Status: verbindlich fuer die Implementierung

## 1. Ziel

Der Updater soll ohne Mausklick bedienbar werden:

- als automatischer Update-Job,
- als Vor-Launcher, der aktualisiert und danach Brave startet,
- als reiner Check fuer Monitoring.

Die bestehende GUI bleibt fuer normale Nutzer erhalten.

## 2. Grundsaetze

- GNU/POSIX-Stil: Long-Options mit `--`, keine `/`-Switches.
- Optionsnamen und Werte werden case-insensitiv verglichen.
- `--option=value` und `--option value` sind gleichwertig.
- `--` trennt Updater-Optionen von Argumenten, die an Brave weitergegeben werden.
- Keine interaktiven Dialoge im nicht-interaktiven Modus.
- Klare Exit-Codes.
- Idempotenz: Ein zweiter Lauf ohne neue Version aendert nichts.
- `--check` und `--dry-run` laden keine Installer herunter.

## 3. Aufrufsyntax

```text
Portable Brave Updater.exe [Optionen] [-- Brave-Argumente...]
```

### Optionen

| Option | Wert | Default | Bedeutung |
|---|---|---|---|
| `--update` | `existing` oder `all` | `existing` | Update-Scope |
| `--launch-brave` | optional `channel:arch` | aus | Nach Erfolg Brave starten |
| `--quit-after` | - | aus | Nach Abschluss beenden |
| `--if-new` | - | an | Nur bei neuerer Version aktualisieren |
| `--force` | - | aus | Auch gleiche Version neu installieren |
| `--check` | - | aus | Nur pruefen, nichts installieren |
| `--dry-run` | - | aus | Plan anzeigen, nichts installieren |
| `--quiet` | - | aus | Keine GUI/Dialoge, Log auf stdout/stderr |
| `--log` | Pfad | aus | Zusaetzlich in Datei loggen |
| `--wait-for-brave` | - | aus | Bei laufendem Brave warten |
| `--timeout` | Sekunden | `300` | Max. Wartezeit fuer `--wait-for-brave` |
| `--help` | - | - | Hilfe ausgeben, Exit 0 |
| `--version` | - | - | Version ausgeben, Exit 0 |

### Tolerante Schreibweisen

Akzeptiert:

```text
--update=existing
--update existing
--update=all
--update all
--launch-brave
--launch-brave=stable:x64
--timeout=120
--timeout 120
```

Nicht akzeptiert:

```text
/updateall
/autoclose
--update
--timeout
```

`--update` ohne Wert ist ein Syntaxfehler (Exit 2). Wenn `--update` gar nicht angegeben wird, gilt `existing`.

## 4. Semantik

### 4.1 `--update=existing` (Default)

- Ermittelt alle bereits installierten Editionen.
- Eine Edition gilt als installiert, wenn `Brave <Edition>\updates\Version.log` oder `Brave <Edition>\Brave.exe` existiert.
- Fuer jede installierte Edition wird die aktuelle Brave-Version geprueft.
- Ist eine neuere Version verfuegbar, wird sie heruntergeladen und installiert.
- Fehlende Editionen werden **nicht** installiert.
- Wenn keine Edition installiert ist: keine Aktion, Exit 0, Logmeldung.

Empfohlen fuer Scheduled Tasks und wiederkehrende Update-Jobs.

### 4.2 `--update=all`

- Alle unterstuetzten Editionen werden sichergestellt und aktualisiert.
- Auf einem 64-Bit-Windows: Nightly, Dev, Beta, Stable jeweils x86 und x64.
- Auf einem 32-Bit-Windows: nur die x86-Editionen.
- Fehlende Editionen werden installiert, vorhandene aktualisiert.
- Achtung: Das kann mehrere hundert MB bis ueber 1 GB herunterladen.

Empfohlen fuer die Erstinstallation oder den Wunsch "alles aktuell halten".

### 4.3 `--if-new` und `--force`

- Default ist `--if-new`: Eine Edition wird nur aktualisiert, wenn die vom Server gemeldete Version neuer ist als die lokale `Version.log`.
- `--force`: Installiert auch dann neu, wenn dieselbe Version bereits lokal vorhanden ist. Entspricht der bisherigen Option "Versionkontrolle ignorieren".
- `--if-new` und `--force` gleichzeitig: Syntaxfehler (Exit 2).

### 4.4 `--launch-brave`

Startet nach erfolgreichem Update die passende Brave-Edition. Ohne Update startet es die einzige installierte Edition.

Auswahlreihenfolge:

1. Explizite Angabe `--launch-brave=channel:arch`.
2. Genau eine Edition wurde in diesem Lauf aktualisiert.
3. Genau eine Edition ist installiert.
4. Sonst: keine eindeutige Auswahl. Kein Start, Exit 6, Logmeldung mit Hinweis auf `--launch-brave=channel:arch`.

Startreihenfolge:

1. Bevorzugt: `Brave <Channel> <Arch> Launcher.exe` im Updater-Verzeichnis.
2. Wenn `Profile.txt` fuer die Edition existiert: Launcher ohne Argumente starten.
3. Wenn `Profile.txt` fehlt: `Brave.exe` direkt starten mit `--user-data-dir="<Edition>\profile"`.
4. Wenn der Launcher fehlt: `Brave.exe` direkt starten.

Der Updater wartet nicht auf Brave. Nach `Process.Start` kehrt er zurueck.

Argumente nach `--` werden an den gestarteten Prozess weitergegeben:

```powershell
Portable Brave Updater.exe --update=existing --launch-brave=stable:x64 -- --incognito https://example.org
```

`--launch-brave` ist mit `--check` und `--dry-run` nicht kombinierbar (Exit 2).

### 4.5 `--quit-after`

- Beendet den Updater nach Abschluss der Operation, auch bei Fehlern.
- In Kombination mit `--quiet` oder `--check` ist der Prozess ohnehin nicht-interaktiv und beendet sich; `--quit-after` ist dann redundant, aber erlaubt.
- Ohne `--quit-after` und ohne `--quiet` kann die GUI nach dem Update sichtbar bleiben.

### 4.6 `--check`

- Prueft nur Versionen und Installationsstatus.
- Kein Download, keine Installation.
- Exit 5, wenn mindestens eine Aktion moeglich waere.
- Exit 0, wenn alles aktuell ist.
- Exit 4 bei Netzwerk-/Serverfehler.
- Mit `--update=all` werden auch fehlende Editionen als "installierbar" gemeldet.

### 4.7 `--dry-run`

- Fuehrt die Versionspruefung aus.
- Gibt aus, welche Editionen aktualisiert oder installiert wuerden.
- Kein Download, keine Installation.
- Exit-Codes wie `--check`.

### 4.8 `--quiet`

- Keine Fenster, keine MessageBoxen.
- Fortschritt und Ergebnisse auf stdout.
- Fehler auf stderr.
- Impliziert nicht-interaktiven Modus und automatisches Beenden.
- `--log` bleibt zusaetzlich moeglich.

### 4.9 `--log <pfad>`

- Schreibt Logzeilen mit Zeitstempel in die angegebene Datei.
- Verzeichnis muss existieren; sonst Exit 2.
- Fehler werden immer zusaetzlich auf stderr ausgegeben.
- Empfehlung: `--log "logs\updater.log"`.

### 4.10 `--wait-for-brave` und `--timeout`

- Wenn ein `Brave`-Prozess laeuft:
  - Interaktiv: bisherige MessageBox.
  - Mit `--quiet` ohne `--wait-for-brave`: sofort Exit 3.
  - Mit `--wait-for-brave`: warten, bis Brave beendet ist, maximal `--timeout` Sekunden.
- Timeout abgelaufen: Exit 3.
- `--timeout` ohne `--wait-for-brave`: Syntaxfehler (Exit 2).

## 5. Exit-Codes

| Code | Bedeutung |
|---|---|
| 0 | Erfolg oder keine Aktion noetig |
| 1 | Allgemeiner Fehler |
| 2 | Ungueltige Argumente / Syntaxfehler |
| 3 | Brave laeuft noch / Wartezeit abgelaufen |
| 4 | Netzwerk-, Server- oder Versionspruefungsfehler |
| 5 | Update oder Installation verfuegbar (`--check`/`--dry-run`) |
| 6 | Launch fehlgeschlagen oder Edition nicht eindeutig |

## 6. Ausgabe

Beispiele fuer stdout:

```text
[2026-11-02 12:00:00] Update scope: existing
[2026-11-02 12:00:01] Installed: Brave Stable x64 1.98.63
[2026-11-02 12:00:02] Available: Brave Stable x64 1.98.70
[2026-11-02 12:00:02] Action: update Stable:x64
[2026-11-02 12:02:10] Updated: Brave Stable x64 1.98.70
[2026-11-02 12:02:10] Launch: Brave Stable x64 Launcher.exe
```

Fehlerbeispiel:

```text
[2026-11-02 12:00:00] ERROR: Network error while checking Stable:x64
```

## 7. Beispiele

```powershell
# Standard: nur installierte Editionen aktualisieren, danach beenden
Portable Brave Updater.exe --update=existing --quit-after

# Alle Editionen installieren/aktualisieren, leise, mit Log
Portable Brave Updater.exe --update=all --quiet --log "logs\update-all.log"

# Nur pruefen, fuer Monitoring
Portable Brave Updater.exe --check --quiet

# Aktualisieren und Stable x64 starten
Portable Brave Updater.exe --update=existing --launch-brave=stable:x64

# Automatischer Start der einzigen installierten Edition
Portable Brave Updater.exe --update=existing --launch-brave --quit-after

# Brave-Argumente durchreichen
Portable Brave Updater.exe --update=existing --launch-brave=stable:x64 -- --incognito
```

## 8. Abnahmekriterien

- `--update=existing` installiert keine fehlende Edition.
- `--update=all` installiert fehlende Editionen und aktualisiert vorhandene.
- `--update all` verhaelt sich wie `--update=all`.
- Ohne `--update` gilt `existing`.
- `--check` und `--dry-run` laden keine Installer herunter.
- `--quiet` erzeugt keine MessageBox und keine sichtbare GUI.
- `--quit-after` beendet den Prozess nach Abschluss.
- `--launch-brave` startet bei genau einer installierten Edition den Launcher.
- Bei mehreren installierten Editionen und ohne explizite Auswahl: Exit 6.
- `--launch-brave=stable:x64` startet gezielt diese Edition.
- `--`-Argumente werden an Brave weitergegeben.
- Exit-Codes entsprechen der Tabelle.
- Der interaktive GUI-Modus funktioniert unveraendert.

## 9. Nicht-Ziele (MVP)

- Windows-Autostart registrieren.
- GUI-Neudesign.
- Umschreiben der Launcher.
- Self-Update im headless Modus.
- JSON-Ausgabe.
- Parallele Updates mehrerer Editionen.
- Signaturpruefung der Brave-Installer (spaeter).

## 10. Spaetere Erweiterungen

- `--edition=stable:x64` zum gezielten Aktualisieren einer Edition.
- `--json` fuer Monitoring.
- `--register-autostart` / `--unregister-autostart`.
- `--verify-signature` fuer heruntergeladene Installer.
- `--no-self-update` als explizite Option, falls Self-Update wieder aktiviert wird.
