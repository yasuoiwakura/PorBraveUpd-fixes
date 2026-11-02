# ADR 0002 - Zielsystem .NET Framework 4.8

- Status: akzeptiert
- Datum: 2026-11-02

## Kontext

Das Hauptprojekt zielt auf .NET Framework 4.7, die neun Launcher auf .NET Framework 4.0. Visual Studio 2022 kann 4.0 nicht mehr out-of-the-box bauen. .NET Framework 4.0 ist seit 2016 out of support. .NET Framework 4.8 ist aktiv und auf Windows 11 Bestandteil des Betriebssystems.

## Entscheidung

Hauptprojekt und Launcher werden auf **.NET Framework 4.8** retargetet.

## Begruendung

- 4.8 ist aktiv unterstuetzt und auf Windows 11 vorhanden.
- VS 2022 kann 4.8 ohne Legacy-Targeting-Pack bauen.
- Kein Visual Studio 2019 und kein 4.0-Targeting-Pack notwendig.
- Geringes Code-Risiko: Die verwendeten APIs sind in 4.8 vorhanden.
- 4.8.1 ist ebenfalls unterstuetzt, aber 4.8 ist die breitere portable Basis.

## Konsequenzen

- `Portable Brave Updater.csproj`: `TargetFrameworkVersion` auf `v4.8`.
- Alle Launcher-CSProjects: `TargetFrameworkVersion` auf `v4.8`.
- `app.config` optional auf 4.8 anpassen.
- Launcher muessen neu gebaut werden, wenn sie nicht aus dem Release uebernommen werden.
- Mindestvoraussetzung: Windows mit .NET Framework 4.8 (Windows 10 1903+ / Windows 11).

## Alternativen

| Alternative | Bewertung |
|---|---|
| 4.0 beibehalten | EOL, Build nur mit VS 2019/Workaround |
| 4.7 beibehalten | aktiv, aber zusaetzliches Targeting-Pack noetig |
| 4.8.1 | aktiv, aber neuere Mindestvoraussetzung |
| .NET 8/9 | groesserer Port, nicht fuer diese Aenderung |
