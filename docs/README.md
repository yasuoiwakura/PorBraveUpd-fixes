# PorBraveUpd-fixes - Projektdokumentation

Stand: 2026-11-02  
Basis: Fork `yasuoiwakura/PorBraveUpd-fixes`, Commit `85811f7`  
Upstream: `UndertakerBen/PorBraveUpd`

## Ziel

Der Fork soll kleine, klar abgegrenzte Verbesserungen am Portable Brave Updater erhalten. Im Fokus steht eine nicht-interaktive CLI, mit der der Updater als Vor-Launcher oder als automatisierter Update-Job verwendet werden kann.

Geplante Kernfunktionen:

- `--update=existing` als Default: nur bereits installierte Editionen aktualisieren.
- `--update=all`: alle unterstuetzten Editionen installieren bzw. aktualisieren.
- `--launch-brave`: nach dem Update die passende Brave-Edition starten.
- `--quit-after`: Updater nach Abschluss beenden.
- `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave`.
- Self-Update fuer den Prototyp deaktiviert; fuer ein Release auf Fork-URLs umgestellt.

## Entscheidungen

| Thema | Entscheidung |
|---|---|
| Sprache | C# beibehalten |
| Ziel-Framework | .NET Framework 4.8 |
| Launcher | ebenfalls auf .NET Framework 4.8 retargeten |
| CLI-Stil | GNU/POSIX-Long-Options mit `--`, keine `/`-Switches |
| Update-Scope | `--update=existing` (Default), `--update=all` |
| Launch | `--launch-brave`; bevorzugt vorhandener Launcher |
| Self-Update | Prototyp deaktiviert, Release auf Fork-URLs |
| Build | Visual Studio 2022 Build Tools + MSBuild |
| Dokumentation | Spec-Driven Development: Analyse, Build, CLI-Spec, Architektur, Testplan, Release/Lizenz, ADRs |

## Dokumente

| Datei | Inhalt |
|---|---|
| `00-ANALYSE.md` | Bestandsaufnahme, Machbarkeit, Lizenz, Empfehlung |
| `01-BUILD.md` | Toolchain, Installation, Retargeting, Build, CI |
| `02-CLI-SPEC.md` | Verbindliche CLI-Spezifikation inkl. Exit-Codes |
| `03-ARCHITEKTUR.md` | Ist-Zustand, Zielarchitektur, Refactoring-Plan |
| `04-TESTPLAN.md` | Testmatrix, Testfaelle, Abnahmekriterien |
| `05-RELEASE-LIZENZ.md` | MIT, Marken, 7-Zip, Release-Checkliste |
| `06-BEST-PRACTICES.md` | CLI-, Build-, Security- und Testing-Best-Practices |
| `adr/0001-csharp-behalten.md` | ADR: keine Neuimplementierung |
| `adr/0002-zielsystem-net48.md` | ADR: .NET Framework 4.8 statt 4.0/4.7 |
| `adr/0003-cli-konventionen.md` | ADR: Long-Options und Semantik |

## Status

- Analyse: erledigt
- Dokumentation: erledigt
- Implementierung: nicht begonnen
- Baseline-Build und Smoke-Test: offen
