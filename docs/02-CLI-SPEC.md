# 02 - CLI Specification

Last updated: 2026-11-02  
Status: implemented for the MVP

## 1. Goal

The updater should be usable without mouse interaction:

- as an automated update job,
- as a pre-launcher that updates and then starts Brave,
- as a check-only tool for monitoring.

The existing GUI remains available for normal users.

## 2. Principles

- GNU/POSIX style: long options with `--`, no `/` switches.
- Option names and values are compared case-insensitively.
- `--option=value` and `--option value` are equivalent.
- `--` separates updater options from arguments passed to Brave.
- No interactive dialogs in non-interactive mode.
- Clear exit codes.
- Idempotence: a second run without a new version changes nothing.
- `--check` and `--dry-run` do not download installers.

## 3. Syntax

```text
Portable Brave Updater.exe [options] [-- Brave arguments...]
```

### Options

| Option | Value | Default | Meaning |
|---|---|---|---|
| `--update` | `existing` or `all` | `existing` | Update scope |
| `--launch-brave` | optional `channel:arch` | off | Start Brave after success |
| `--quit-after` | - | off | Exit after completion |
| `--if-new` | - | on | Update only when a newer version exists |
| `--force` | - | off | Reinstall even if the same version is installed |
| `--check` | - | off | Check only, do not install |
| `--dry-run` | - | off | Show plan, do not install |
| `--quiet` | - | off | No GUI/dialogs, log to stdout/stderr |
| `--log` | path | off | Also write to a log file |
| `--wait-for-brave` | - | off | Wait while Brave is running |
| `--timeout` | seconds | `300` | Maximum wait for `--wait-for-brave` |
| `--help` | - | - | Print help, exit 0 |
| `--version` | - | - | Print version, exit 0 |

### Accepted variants

Accepted:

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

Not accepted:

```text
/updateall
/autoclose
--update
--timeout
```

`--update` without a value is a syntax error (exit 2). If `--update` is not specified, `existing` applies.

## 4. Semantics

### 4.1 `--update=existing` (default)

- Finds all installed editions.
- An edition is considered installed if `Brave <Edition>\updates\Version.log` or `Brave <Edition>\Brave.exe` exists.
- The current Brave version is checked for each installed edition.
- If a newer version is available, it is downloaded and installed.
- Missing editions are **not** installed.
- If no edition is installed: no action, exit 0, log message.

Recommended for scheduled tasks and recurring update jobs.

### 4.2 `--update=all`

- Ensures and updates all supported editions.
- On 64-bit Windows: Nightly, Dev, Beta and Stable for both x86 and x64.
- On 32-bit Windows: only x86 editions.
- Missing editions are installed; existing editions are updated.
- Warning: this can download several hundred MB up to more than 1 GB.

Recommended for first-time installation or "keep everything current".

### 4.3 `--if-new` and `--force`

- Default is `--if-new`: an edition is updated only if the server-reported version is newer than the local `Version.log`.
- `--force`: reinstalls even if the same version is already installed. Equivalent to the previous "ignore version check" option.
- `--if-new` and `--force` together: syntax error (exit 2).

### 4.4 `--launch-brave`

Starts the matching Brave edition after a successful update. Without an update, it starts the only installed edition.

Selection order:

1. Explicit `--launch-brave=channel:arch`.
2. Exactly one edition was updated in this run.
3. Exactly one edition is installed.
4. Otherwise: no unique selection. Do not start, exit 6, log a hint to use `--launch-brave=channel:arch`.

Start order:

1. Preferred: `Brave <Channel> <Arch> Launcher.exe` in the updater directory.
2. If `Profile.txt` exists for the edition: start the launcher without arguments.
3. If `Profile.txt` is missing: start `Brave.exe` directly with `--user-data-dir="<Edition>\profile"`.
4. If the launcher is missing: start `Brave.exe` directly.

The updater does not wait for Brave. It returns after `Process.Start`.

Arguments after `--` are passed to the started process:

```powershell
Portable Brave Updater.exe --update=existing --launch-brave=stable:x64 -- --incognito https://example.org
```

`--launch-brave` cannot be combined with `--check` or `--dry-run` (exit 2).

### 4.5 `--quit-after`

- Exits the updater after the operation, including on errors.
- With `--quiet` or `--check`, the process is already non-interactive and exits; `--quit-after` is then redundant but allowed.
- Without `--quit-after` and without `--quiet`, the GUI may remain visible after the update.

### 4.6 `--check`

- Checks versions and installation status only.
- No download, no installation.
- Exit 5 if at least one action would be possible.
- Exit 0 if everything is current.
- Exit 4 on network/server error.
- With `--update=all`, missing editions are also reported as installable.

### 4.7 `--dry-run`

- Runs the version check.
- Prints which editions would be updated or installed.
- No download, no installation.
- Exit codes as with `--check`.

### 4.8 `--quiet`

- No windows, no message boxes.
- Progress and results on stdout.
- Errors on stderr.
- Implies non-interactive mode and automatic exit.
- `--log` remains available.

### 4.9 `--log <path>`

- Writes log lines with timestamps to the given file.
- The directory must exist; otherwise exit 2.
- Errors are always also written to stderr.
- Recommended: `--log "logs\updater.log"`.

### 4.10 `--wait-for-brave` and `--timeout`

- If a `Brave` process is running:
  - Interactive: existing message box.
  - With `--quiet` without `--wait-for-brave`: exit 3 immediately.
  - With `--wait-for-brave`: wait until Brave exits, up to `--timeout` seconds.
- Timeout expired: exit 3.
- `--timeout` without `--wait-for-brave`: syntax error (exit 2).

## 5. Exit codes

| Code | Meaning |
|---|---|
| 0 | Success or no action required |
| 1 | General error |
| 2 | Invalid arguments / syntax error |
| 3 | Brave still running / wait timed out |
| 4 | Network, server or version-check error |
| 5 | Update or installation available (`--check`/`--dry-run`) |
| 6 | Launch failed or edition not unique |
| 7 | Required runtime files are missing (`Bin\7zr.exe`, launcher) |

## 6. Output

Example stdout:

```text
[2026-11-02 12:00:00] Update scope: existing
[2026-11-02 12:00:01] Installed: Brave Stable x64 1.98.63
[2026-11-02 12:00:02] Available: Brave Stable x64 1.98.70
[2026-11-02 12:00:02] Action: update Stable:x64
[2026-11-02 12:02:10] Updated: Brave Stable x64 1.98.70
[2026-11-02 12:02:10] Launch: Brave Stable x64 Launcher.exe
```

Error example:

```text
[2026-11-02 12:00:00] ERROR: Network error while checking Stable:x64
```

## 7. Examples

```powershell
# Default: update installed editions, then exit
Portable Brave Updater.exe --update=existing --quit-after

# Install/update all editions, quiet, with log
Portable Brave Updater.exe --update=all --quiet --log "logs\update-all.log"

# Check only, for monitoring
Portable Brave Updater.exe --check --quiet

# Update and start Stable x64
Portable Brave Updater.exe --update=existing --launch-brave=stable:x64

# Automatically start the only installed edition
Portable Brave Updater.exe --update=existing --launch-brave --quit-after

# Pass Brave arguments
Portable Brave Updater.exe --update=existing --launch-brave=stable:x64 -- --incognito
```

## 8. Acceptance criteria

- `--update=existing` does not install a missing edition.
- `--update=all` installs missing editions and updates existing ones.
- `--update all` behaves like `--update=all`.
- Without `--update`, `existing` applies.
- `--check` and `--dry-run` do not download installers.
- `--quiet` produces no message box and no visible GUI.
- `--quit-after` exits the process after completion.
- `--launch-brave` starts the launcher when exactly one edition is installed.
- With multiple installed editions and no explicit selection: exit 6.
- `--launch-brave=stable:x64` starts that edition specifically.
- `--` arguments are passed to Brave.
- Exit codes match the table.
- The interactive GUI mode works unchanged.

## 9. Non-goals (MVP)

- Register Windows autostart.
- GUI redesign.
- Rewrite the launchers.
- Self-update in headless mode.
- JSON output.
- Parallel updates of multiple editions.
- Signature verification of Brave installers (later).

## 10. Future extensions

- `--edition=stable:x64` to update one specific edition.
- `--json` for monitoring.
- `--register-autostart` / `--unregister-autostart`.
- `--verify-signature` for downloaded installers.
- `--no-self-update` as an explicit option if self-update is re-enabled.
