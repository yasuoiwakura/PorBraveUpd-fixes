# 06 - Best Practices

Stand: 2026-11-02

## 1. CLI-Design

- Long-Options mit `--`, keine `/`-Switches.
- `--help` und `--version` immer unterstuetzen.
- `--option=value` und `--option value` akzeptieren.
- Optionsnamen und Werte case-insensitiv vergleichen (Windows-freundlich).
- `--` als Trenner fuer Pass-through-Argumente.
- Keine interaktiven Prompts im nicht-interaktiven Modus.
- Klare Exit-Codes; `0` fuer Erfolg/keine Aktion.
- Fortschritt auf stdout, Fehler auf stderr.
- Idempotente Operationen: zweiter Lauf ohne neue Version aendert nichts.
- `--check` und `--dry-run` duerfen keine Downloads ausloesen.
- `--quiet` darf keine MessageBox und kein Fenster zeigen.
- Unbekannte Optionen: Fehler + Hilfehinweis, Exit 2.

### Parser

Fuer dieses Projekt wird ein **handgeschriebener Parser** empfohlen:

- Keine zusaetzliche NuGet-Abhaengigkeit.
- Volle Kontrolle ueber Windows-Schreibweisen.
- Leicht testbar.
- Kein Risiko durch .NET-Framework-Kompatibilitaet externer Pakete.

`System.CommandLine` ist eine Alternative, bringt aber fuer diese kleine CLI unnoetige Komplexitaet.

## 2. .NET-Framework-Best-Practices

- Ziel **.NET Framework 4.8**.
- Kein Target auf .NET Framework 4.0 (EOL).
- Kein Target auf 4.7, wenn 4.8 ohne Aufwand moeglich ist.
- `AnyCPU` beibehalten; x86/x64 werden ueber die Editionen abgedeckt.
- Alte `WebClient`/`WebRequest`-APIs im MVP beibehalten, spaeter ggf. `HttpClient`.
- `Thread.Sleep` durch `Task.Delay` ersetzen.
- `async`/`await` konsequent nutzen; keine blockierenden Netzwerkaufrufe im UI-Thread.
- `ConfigureAwait(false)` im Kern, UI-Marshalling ueber `IProgress`.
- `CancellationToken` fuer abbrechbare Operationen vorsehen.

## 3. Architektur

- UI und Fachlogik trennen.
- `Form1` wird Adapter, nicht Ort der Geschaeftslogik.
- `UpdateService` kennt keine Controls.
- Fortschritt ueber `IProgress<UpdateProgress>`.
- Fehler ueber Exceptions + zentrale Abbildung auf Exit-Codes.
- Keine `MessageBox` im Kern.
- Keine parallelen Updates; ein `SemaphoreSlim` oder eine sequenzielle Schleife.
- Editionen als Objekte statt paralleler Arrays.
- Konfiguration (URLs, Self-Update) zentral und aenderbar halten.

## 4. Build und CI

- Toolchain dokumentieren und im Repo festhalten.
- Buildscript (`build.ps1`) fuer reproduzierbare Builds.
- CI auf `windows-latest` mit MSBuild.
- Release-Artefakte mit Versionsnummer und Checksum.
- Keine Build-Artefakte ins Repo committen.
- `Bin/`-Assets entweder aus Release beziehen oder im Build erzeugen.
- NuGet-Restore explizit ausfuehren.
- Alte Analyzer entfernen, wenn sie den Build stoeren.

## 5. Sicherheit

- Self-Update standardmaessig aus.
- Wenn Self-Update aktiv: HTTPS, SHA256, optional Signatur.
- Brave-Installer optional per Authenticode pruefen.
- Keine Ausfuehrung bei Hash-Mismatch.
- Keine Admin-Rechte anfordern.
- Keine Secrets oder Tokens loggen.
- Downloads nur von erwarteten Hosts.
- Release-EXE optional signieren, um SmartScreen-Warnungen zu reduzieren.

## 6. Testing

- Parser-Tests ohne Netzwerk.
- Exit-Code-Tests.
- `--check`/`--dry-run` ohne Download testen.
- Integrationstests auf einer VM oder in einem Testverzeichnis.
- GUI-Regressionstests manuell.
- Fehlerfaelle aktiv testen: Netzwerk, fehlende Dateien, laufender Brave.
- Testdaten isolieren; keine produktive Installation gefaehrden.

## 7. Logging

- Einheitliches Format mit Zeitstempel.
- Level: INFO, WARN, ERROR.
- `--quiet`: keine nicht-essenziellen Ausgaben.
- `--log`: zusaetzliche Datei, Verzeichnis muss existieren.
- Fehler immer auf stderr.
- Keine personenbezogenen Daten loggen.

## 8. Dokumentation

- Spec vor Implementierung.
- ADRs fuer Architekturentscheidungen.
- Testplan mit Abnahmekriterien.
- Release-Checkliste.
- Aenderungen im `CHANGELOG.md` festhalten.
- README um CLI-Beispiele ergaenzen, sobald implementiert.

## 9. Referenzen

- .NET Framework Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- VS Build Tools Komponenten: https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools
- Microsoft.NETFramework.ReferenceAssemblies: https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies
- 7-Zip License: https://7-zip.org/license.txt
- MIT License: https://opensource.org/license/mit
