# 03 - Architecture and Refactoring

Last updated: 2026-11-02

## 1. Current state

The updater is a WinForms application with tightly mixed UI and business logic.

Problems:

- `Form1` constructor performs network calls.
- Update logic, download, extraction and UI progress are coupled in `DownloadFile`.
- Errors and hints go through `MessageBox`.
- Self-update and launcher update run automatically at startup.
- Data is stored in parallel arrays wired by indexes.
- URLs were hard-coded to `UndertakerBen/PorBraveUpd`; since 2026-11-02 they are centralized as `RepositoryUrl`/`RawMasterUrl` and point to the fork.
- No tests.
- `Thread.Sleep` and synchronous network calls block the UI.

## 2. Target architecture

```text
Program.Main(args)
  |
  +-- CommandLineOptions.Parse(args)
  |
  +-- CliRunner (headless)  or  Form1 (interactive)
          |
          +-- UpdateService
          |     +-- EditionCatalog
          |     +-- BraveUpdateClient (Omaha/HTTP)
          |     +-- InstallerExtractor (7zr)
          |     +-- EditionInstaller (files, Version.log, launcher)
          |     +-- IProgress<UpdateProgress>
          |     +-- ILogger
          |
          +-- BraveProcessGuard
          +-- LauncherService
          +-- SelfUpdateService (optional, disabled by default)
```

### Core components

| Component | Responsibility |
|---|---|
| `CommandLineOptions` | Parse, validate, help/version |
| `CliRunner` | Non-interactive flow, exit codes |
| `UpdateService` | Orchestrate check/update |
| `EditionCatalog` | Channel/architecture -> app ID, directory, launcher |
| `BraveUpdateClient` | Omaha request, XML response, download URL |
| `InstallerExtractor` | `7zr.exe` calls, error handling |
| `EditionInstaller` | Move files, `Version.log`, copy launcher |
| `BraveProcessGuard` | Detect/wait for running Brave processes |
| `LauncherService` | Start the matching edition |
| `ILogger` | stdout/stderr/file, no MessageBox |
| `IProgress<UpdateProgress>` | Progress for GUI and CLI |
| `SelfUpdateService` | Optional, disabled in the MVP |

## 3. Data model

Instead of parallel arrays:

```csharp
sealed class Edition
{
    public string Channel;          // Nightly, Dev, Beta, Stable
    public string Arch;             // x86, x64
    public string AppId;
    public string Ap;
    public string InstallDir;       // e.g. "Brave Stable x64"
    public string EntpDir;          // e.g. "Stable64"
    public string Ring;
    public string IconIndex;
    public bool IsInstalled;
    public string InstalledVersion;
    public string AvailableVersion;
}
```

`EditionCatalog` returns all editions and the selection for `existing` or `all`.

## 4. Flow

### Interactive

1. `Form1` starts.
2. `UpdateService.CheckAsync()` fetches versions.
3. The GUI displays status.
4. A button click calls `UpdateService.UpdateAsync(edition, options)`.
5. Progress is reported via `IProgress<UpdateProgress>`.
6. Errors are shown as dialogs or logs.

### Headless

1. `Program.Main` parses arguments.
2. `CliRunner` checks the Brave process.
3. `UpdateService` determines the scope (`existing`/`all`).
4. For each edition: check, then download/install if needed.
5. Optional `LauncherService.Launch`.
6. Set the exit code.
7. Exit on `--quit-after` or `--quiet`.

## 5. Error handling

Custom exception types:

- `InvalidArgumentsException` -> exit 2
- `BraveRunningException` -> exit 3
- `NetworkException` -> exit 4
- `LaunchException` -> exit 6
- `UpdateException` -> exit 1

In GUI mode these are translated into dialogs; in CLI mode into logs + exit codes.

## 6. Refactoring steps

1. Extract `EditionCatalog` from the arrays; keep behavior unchanged.
2. Extract `BraveUpdateClient` from `Form1`.
3. Split `DownloadFile` into download, extract and install.
4. Report progress via `IProgress` instead of direct control access.
5. Introduce a `Notify` abstraction: GUI dialog or log.
6. Add `CommandLineOptions` + `CliRunner`.
7. Implement `--launch-brave` and `--quit-after`.
8. Make self-update optional and URLs configurable.
9. Reconnect the interactive mode to the new core.
10. Update tests and documentation.

Build and run an update test after each step.

## 7. Compatibility

- Interactive mode must feel unchanged for users.
- `Version.log` format remains compatible: `Version|Ring|Arch`.
- Directory names remain compatible.
- Launcher EXEs remain usable.
- Existing installations must continue to work without migration.

## 8. Self-update

Current state:

- Self-update is active but points to the fork.
- No check against upstream.
- As long as `Version.txt` and the EXE version match, no self-update is offered.
- Release assets are provided by the fork release; publish the matching asset before bumping `Version.txt`.

For a release:

- Provide `Portable.Brave.Updater.v<Version>.7z`.
- Keep `Version.txt` and release asset names consistent.
- Optionally add hash verification.
- Optionally disable self-update or offer `--self-update` as an opt-in.

## 9. Security

- No downloads without HTTPS.
- Optional: verify Authenticode signature of Brave installers.
- Optional: verify SHA256 of the self-update archive.
- No elevation required.
- No secrets in logs.
- Do not execute unverified code from the network.

## 10. Non-goals

- Full rewrite.
- Migration to .NET 8/9.
- New GUI design.
- Changing launcher functionality.
- Parallel downloads.
