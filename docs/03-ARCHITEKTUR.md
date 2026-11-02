# 03 - Architektur und Refactoring

Stand: 2026-11-02

## 1. Ist-Zustand

Der Updater ist eine WinForms-Anwendung mit stark vermischter UI- und Fachlogik.

Probleme:

- `Form1`-Konstruktor fuehrt Netzwerkaufrufe aus (`Form1.cs:33`).
- Update-Logik, Download, Entpacken und UI-Fortschritt sind in `DownloadFile` gekoppelt (`Form1.cs:328`).
- Fehler und Hinweise laufen ueber `MessageBox` (`Form1.cs:191`, `:661`).
- Self-Update und Launcher-Update laufen ungefragt beim Start (`Form1.cs:908`, `:1041`).
- Daten liegen in parallelen Arrays und werden ueber Indizes verdrahtet (`Form1.cs:17`-`:27`).
- URLs sind hart auf `UndertakerBen/PorBraveUpd` verdrahtet.
- Keine Tests.
- `Thread.Sleep` und synchrone Netzwerkaufrufe blockieren die UI.

## 2. Zielarchitektur

```text
Program.Main(args)
  |
  +-- CommandLineOptions.Parse(args)
  |
  +-- CliRunner (headless)  oder  Form1 (interaktiv)
          |
          +-- UpdateService
          |     +-- EditionCatalog
          |     +-- BraveUpdateClient (Omaha/HTTP)
          |     +-- InstallerExtractor (7zr)
          |     +-- EditionInstaller (Dateien, Version.log, Launcher)
          |     +-- IProgress<UpdateProgress>
          |     +-- ILogger
          |
          +-- BraveProcessGuard
          +-- LauncherService
          +-- SelfUpdateService (optional, standardmaessig aus)
```

### Zentrale Bausteine

| Baustein | Aufgabe |
|---|---|
| `CommandLineOptions` | Parsen, Validieren, Help/Version |
| `CliRunner` | Nicht-interaktiver Ablauf, Exit-Codes |
| `UpdateService` | Orchestrierung Check/Update |
| `EditionCatalog` | Kanal/Architektur -> App-ID, Verzeichnis, Launcher |
| `BraveUpdateClient` | Omaha-Anfrage, XML-Antwort, Download-URL |
| `InstallerExtractor` | `7zr.exe` Aufrufe, Fehlerbehandlung |
| `EditionInstaller` | Dateien verschieben, `Version.log`, Launcher kopieren |
| `BraveProcessGuard` | Laufende Brave-Prozesse erkennen/warten |
| `LauncherService` | Passende Edition starten |
| `ILogger` | stdout/stderr/Datei, keine MessageBox |
| `IProgress<UpdateProgress>` | Fortschritt fuer GUI und CLI |
| `SelfUpdateService` | Optional, im MVP aus |

## 3. Datenmodell

Statt paralleler Arrays:

```csharp
sealed class Edition
{
    public string Channel;          // Nightly, Dev, Beta, Stable
    public string Arch;             // x86, x64
    public string AppId;
    public string Ap;
    public string InstallDir;       // z. B. "Brave Stable x64"
    public string EntpDir;          // z. B. "Stable64"
    public string Ring;
    public string IconIndex;
    public bool IsInstalled;
    public string InstalledVersion;
    public string AvailableVersion;
}
```

`EditionCatalog` liefert alle Editionen und die Auswahl fuer `existing` bzw. `all`.

## 4. Ablauf

### Interaktiv

1. `Form1` startet.
2. `UpdateService.CheckAsync()` holt Versionen.
3. GUI zeigt Status.
4. Button klickt `UpdateService.UpdateAsync(edition, options)`.
5. Fortschritt kommt ueber `IProgress<UpdateProgress>`.
6. Fehler werden als Dialog oder Log ausgegeben.

### Headless

1. `Program.Main` parst Argumente.
2. `CliRunner` prueft Brave-Prozess.
3. `UpdateService` ermittelt Scope (`existing`/`all`).
4. Fuer jede Edition: Check, ggf. Download/Install.
5. Optional `LauncherService.Launch`.
6. Exit-Code setzen.
7. Bei `--quit-after` oder `--quiet`: Prozess beenden.

## 5. Fehlerbehandlung

Eigene Exception-Typen:

- `InvalidArgumentsException` -> Exit 2
- `BraveRunningException` -> Exit 3
- `NetworkException` -> Exit 4
- `LaunchException` -> Exit 6
- `UpdateException` -> Exit 1

Im GUI-Modus werden diese in Dialoge uebersetzt; im CLI-Modus in Log + Exit-Code.

## 6. Refactoring-Schritte

1. `EditionCatalog` aus den Arrays extrahieren; Verhalten unveraendert.
2. `BraveUpdateClient` aus `Form1` herausloesen.
3. `DownloadFile` in Download, Extract, Install aufteilen.
4. Fortschritt ueber `IProgress` statt direkter Control-Zugriffe.
5. `Notify`-Abstraktion einfuehren: GUI-Dialog oder Log.
6. `CommandLineOptions` + `CliRunner` ergaenzen.
7. `--launch-brave` und `--quit-after` implementieren.
8. Self-Update optional machen und URLs konfigurierbar.
9. Interaktiven Modus wieder an den neuen Kern anbinden.
10. Tests und Doku aktualisieren.

Nach jedem Schritt bauen und einen Update-Test durchfuehren.

## 7. Kompatibilitaet

- Der interaktive Modus muss sich fuer Nutzer unveraendert anfuehlen.
- `Version.log`-Format bleibt kompatibel: `Version|Ring|Arch`.
- Verzeichnisnamen bleiben kompatibel.
- Launcher-EXEs bleiben unveraendert nutzbar.
- Bestehende Installationen muessen ohne Migration weiter funktionieren.

## 8. Self-Update

Im MVP:

- Self-Update standardmaessig deaktiviert.
- Keine automatische Pruefung gegen Upstream.
- Keine automatische Ersetzung der eigenen EXE.

Fuer ein Release:

- URLs auf den eigenen Fork umstellen.
- `Version.txt` und Release-Asset-Namen konsistent halten.
- Optional Hashpruefung ergaenzen.
- Optional `--self-update` als Opt-in.

## 9. Sicherheit

- Keine Downloads ohne HTTPS.
- Optional: Authenticode-Signatur der Brave-Installer pruefen.
- Optional: SHA256 des Self-Update-Archivs pruefen.
- Keine Elevation erforderlich.
- Keine Secrets im Log.
- Keine Ausfuehrung von ungeprueftem Code aus dem Netz.

## 10. Nicht-Ziele

- Vollstaendiger Rewrite.
- Umstieg auf .NET 8/9.
- Neues GUI-Design.
- Aenderung der Launcher-Funktionalitaet.
- Parallele Downloads.
