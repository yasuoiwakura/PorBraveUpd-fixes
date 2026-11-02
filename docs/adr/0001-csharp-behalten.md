# ADR 0001 - C# beibehalten, keine Neuimplementierung

- Status: akzeptiert
- Datum: 2026-11-02

## Kontext

Der Portable Brave Updater ist eine Windows-WinForms-Anwendung in C# (.NET Framework). Geplant sind kleine CLI-Erweiterungen. Zur Diskussion stand, das Projekt fuer diese Aenderungen in einer anderen Sprache neu zu bauen.

## Entscheidung

Das Projekt bleibt in C#. Es wird gezielt refactort, aber nicht neu implementiert.

## Begruendung

- Die geplante Aenderung ist klein.
- Ein Rewrite muesste WinForms, Omaha-Protokoll, 7z-Entpacken, COM-Shortcuts, Launcher-Profile und Self-Update nachbilden.
- C#/.NET Framework ist auf Windows 11 nativ vorhanden.
- MIT erlaubt die Weiterentwicklung.
- Ein Sprachwechsel bringt keinen funktionalen Mehrwert, aber hohes Regressionsrisiko.

## Konsequenzen

- Bestehende Projektdateien und Build-Toolchain bleiben relevant.
- .NET Framework 4.0 der Launcher muss auf 4.8 retargetet werden.
- Ein Refactoring ist noetig, um UI und Update-Kern zu trennen.
- Eine spaetere Modernisierung auf .NET 8/9 WinForms bleibt moeglich, ist aber nicht Teil dieser Entscheidung.

## Alternativen

| Alternative | Bewertung |
|---|---|
| Go | GUI, COM, 7z und Launcher muessten neu gebaut werden; hoher Aufwand |
| Rust | aehnlich, zusaetzlich hoehere Einstiegshuerde |
| Python | Runtime-/Packaging-Probleme, AV-False-Positives, kein Vorteil |
| .NET 8 WinForms | moeglich, aber groesserer Port; fuer diese Aenderung nicht noetig |
