# 00 - Analyse

Stand: 2026-11-02

## 1. Bestandsaufnahme des Forks

Der Fork ist technisch nicht leer. Er enthaelt den vollstaendigen Upstream-Stand, aber keine eigenen Commits.

- Alle Branches (`master`, `dev`, `feat-1400-init`) zeigen auf `85811f7`.
- Letzter Upstream-Commit: 2022.
- Upstream hat 6 offene Issues, u. a. zu SSL/TLS und fehlgeschlagenen Updates.
- Der Fork ist oeffentlich und MIT-lizenziert.

## 2. Technischer Aufbau

| Komponente | Technik |
|---|---|
| Haupt-Updater | C# / .NET Framework 4.7 / WinForms |
| Solution | VS-2019-Format, `Portable Brave Updater.sln` |
| Launcher | 9 separate C#-Projekte, jeweils .NET Framework 4.0 |
| Update-Protokoll | Brave/Omaha `update2` per HTTPS-POST und XML |
| Entpacken | externes `Bin/7zr.exe` |
| Shortcuts | COM `IWshRuntimeLibrary` / `WScript.Shell` |
| Release | `.7z` mit `Bin/7zr.exe`, `Bin/Launcher/*.exe` und Updater-EXE |
| Lizenz | MIT, Copyright 2020 UndertakerBen |

Wichtige Code-Stellen:

- `Program.cs:15`: `Main()` ohne Kommandozeilenargumente.
- `Form1.cs:33`: Konstruktor fuehrt Netzwerkaufrufe und UI-Setup aus.
- `Form1.cs:328`: `DownloadFile` ist eng an WinForms und Fortschritts-Controls gekoppelt.
- `Form1.cs:908`: Self-Update-Pruefung und UI.
- `Form1.cs:1041`: Launcher-Update-Pruefung.
- `Form1.cs:991`, `:1013`, `:1046`, `:1059`: URLs zeigen fest auf `UndertakerBen/PorBraveUpd`.
- `Form1.cs:173`, `:191`: laufender Brave blockiert per MessageBox.
- `Portable Brave Updater.csproj:11`: Ziel .NET Framework 4.7.
- Launcher-CSProjects: Ziel .NET Framework 4.0.

## 3. Funktionstest des Upstream-Protokolls

Der Brave-Update-Endpunkt wurde am 2026-11-02 mit der gleichen Omaha-Anfrage wie im Code geprueft:

- HTTP 200
- Stable x64: Version `156.1.98.63`
- Download-URLs und Manifest wurden geliefert.

Damit ist die Kernlogik grundsaetzlich noch funktionsfaehig. Das Entpacken und Installieren wurde dabei nicht getestet; ein Baseline-Smoke-Test bleibt erforderlich.

## 4. Build-Komplexitaet unter Windows 11

Bewertung: **mittel**, gut beherrschbar fuer einen DevOps-Hintergrund.

Positive Punkte:

- Reines .NET-Framework-/WinForms-Projekt, keine modernen SDK-Abhaengigkeiten.
- NuGet wird nur fuer alte Analyzer benoetigt; keine Runtime-Pakete.
- Build mit MSBuild ist einfach.

Stolpersteine:

1. Visual Studio 2022 kann .NET Framework 4.0 nicht out-of-the-box bauen. Die Launcher zielen aber auf 4.0.
2. Das Hauptprojekt zielt auf 4.7; VS 2022 installiert standardmaessig 4.7.2/4.8. Entweder 4.7-Targeting-Pack nachinstallieren oder auf 4.8 retargeten.
3. `Bin/` fehlt im Repo. Fuer einen lauffaehigen Test muessen `7zr.exe` und die Launcher-EXEs aus dem offiziellen Release uebernommen oder gebaut werden.
4. COM-Referenz `IWshRuntimeLibrary` benoetigt `tlbimp.exe` aus dem Windows SDK.
5. Die alten FxCop-Analyzer in `packages.config` können bei modernen Toolchains Warnungen erzeugen; notfalls entfernen.

Empfohlene Loesung:

- Alle Projekte auf **.NET Framework 4.8** retargeten.
- Visual Studio 2022 Build Tools oder Community mit .NET-Desktop-Workload und Windows SDK installieren.
- Fuer den ersten Prototyp `Bin/` aus dem offiziellen Release-`.7z` uebernehmen.

## 5. Neuimplementierung oder C# beibehalten?

**Empfehlung: C# beibehalten.**

Begruendung:

- Die geplante Aenderung ist klein.
- Ein Rewrite muesste WinForms-GUI, Omaha-Protokoll, 7z-Entpacken, COM-Shortcuts, Launcher-Profile und Self-Update nachbilden.
- Ein Rewrite in Go, Rust oder Python bringt fuer diese Aenderung keinen Nutzen und erhoeht das Risiko.
- MIT erlaubt die Weiterentwicklung.
- .NET Framework 4.8 ist auf Windows 11 Bestandteil des Betriebssystems und wird gemaess Windows-Lebenszyklus unterstuetzt.

Optionale spaetere Modernisierung: .NET 8/9 WinForms. Das ist ein separates Projekt und nicht Teil dieser Aenderung.

## 6. Lizenz

MIT erlaubt ausdruecklich:

- Nutzung, Kopie, Aenderung, Zusammenfuehrung
- Veroeffentlichung und Verbreitung
- Unterlizenzierung und Verkauf, auch kommerziell

Bedingung: Copyright-Hinweis und MIT-Lizenztext muessen in Kopien bzw. wesentlichen Teilen enthalten bleiben. Der Fork darf neue Versionen veroeffentlichen, solange `LICENSE` mitgeliefert wird. Eigenes Copyright kann ergaenzt werden.

Nicht durch MIT abgedeckt:

- Brave-Name und Brave-Logos (Marken).
- Braves Nutzungsbedingungen fuer automatisierte Installer-Downloads.

Der bestehende README-Disclaimer sollte beibehalten werden.

## 7. Wichtigste Risiken

| Risiko | Auswirkung | Gegenmassnahme |
|---|---|---|
| Self-Update zeigt auf Upstream | Fork wird durch Original ersetzt | URLs aendern oder Self-Update deaktivieren |
| UI-Logik im Kern | Headless-Betrieb schwierig | Update-Kern extrahieren |
| MessageBoxen | Automatisierung blockiert | `Notify`-Abstraktion, `--quiet` |
| Keine Tests | Regressionen | Parser-Tests + VM-Smoke-Tests |
| .NET 4.0 Launcher | Build mit VS 2022 scheitert | Retarget auf 4.8 |
| Fehlendes `Bin/` | EXE startet, Update scheitert | Release-Assets uebernehmen |
| Unsigned EXE | SmartScreen-Warnung | Code Signing optional |
| Self-Update ohne Hashpruefung | Supply-Chain-Risiko | deaktivieren oder Hash/Signatur pruefen |

## 8. Entscheidungen aus der Abstimmung

1. Launch-Parameter heisst `--launch-brave`.
2. Update-Scope: `--update=existing` als Default, `--update=all` als Alternative. `--update all` wird als tolerante Schreibweise akzeptiert.
3. Launch: Wenn nur eine Edition installiert ist, diese starten. Bevorzugt wird der vorhandene Launcher.
4. Self-Update: im Prototyp deaktiviert, fuer ein Release auf Fork-URLs umgestellt.

## 9. Referenzen

- .NET Framework Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- .NET Framework System Requirements: https://learn.microsoft.com/en-us/dotnet/framework/get-started/system-requirements
- VS Build Tools Workload-IDs: https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools
- 7-Zip Lizenz: https://7-zip.org/license.txt
- MIT Lizenz: https://opensource.org/license/mit
