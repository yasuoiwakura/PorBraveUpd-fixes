# ADR 0003 - CLI-Konventionen

- Status: akzeptiert
- Datum: 2026-11-02

## Kontext

Der Updater hat bisher keine Kommandozeilenargumente. Fuer Automatisierung und Launcher-Betrieb wird eine CLI benoetigt. Zur Diskussion standen Windows-`/`-Switches und GNU/POSIX-Long-Options.

## Entscheidung

Es werden **GNU/POSIX-Long-Options mit `--`** verwendet.

- `--update=existing` (Default)
- `--update=all`
- `--launch-brave`
- `--quit-after`
- `--check`
- `--dry-run`
- `--quiet`
- `--log`
- `--wait-for-brave`
- `--timeout`
- `--help`
- `--version`

`--update all` und `--update existing` werden als tolerante Schreibweisen akzeptiert.

## Begruendung

- `--` ist plattformuebergreifend und toolfreundlich.
- `/` kollidiert mit Windows-Pfaden und ist in vielen CLI-Ökosystemen unueblich.
- Lange, sprechende Namen sind selbsterklaerend.
- `--launch-brave` ist eindeutiger als `--autostart`.
- `--quit-after` ist eindeutiger als `--autoclose`.
- `--if-new` ist das sichere Default-Verhalten; `--force` ist explizit.

## Konsequenzen

- Parser muss `--option=value` und `--option value` unterstuetzen.
- `--` trennt Updater- und Brave-Argumente.
- Unbekannte Optionen fuehren zu Exit 2.
- Die Dokumentation muss Beispiele enthalten.
- Windows-Nutzer, die `/` gewohnt sind, werden auf `--` verwiesen.

## Alternativen

| Alternative | Bewertung |
|---|---|
| `/updateall` | Windows-typisch, aber kollisionsanfaellig und nicht best practice |
| Kurzoptionen `-u`, `-l` | optional spaeter; nicht fuer MVP noetig |
| Nur Umgebungsvariablen | schlecht discoverbar, schlecht testbar |
| JSON-Konfigurationsdatei | fuer Scheduled Tasks unhandlich |
