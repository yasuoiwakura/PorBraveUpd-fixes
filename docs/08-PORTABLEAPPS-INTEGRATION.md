# 08 - PortableApps.com Integration (Planned Feature)

Status: planned, not implemented  
Last updated: 2026-11-02

## 1. Summary

BravePortable is currently a **non-PAF "Other App"** inside the PortableApps.com Platform. It is not in PortableApps.com Format (PAF).

The planned feature is deliberately small:

- keep the existing non-PAF folder layout
- do not build a wrapper executable
- do not convert to full PAF
- optionally support update-on-start through a `.pap.ini` file
- document the manual install/refresh steps for other users

The goal is a clean, low-maintenance integration without adding another process that fights with taskbar or app-running detection.

## 2. Current local state

Inspection findings from `P:\PA\PortableApps\BravePortable`:

- No `App\AppInfo\appinfo.ini`
- No `App\AppInfo\appicon.ico` or `appicon_*.png`
- No `App\AppInfo\Launcher\*.ini`
- No `Data\`, `Other\` or `help.html`
- No root `BravePortable.exe`
- Root executables exist:
  - `Brave Launcher.exe` (v1.3.3.7)
  - `Portable Brave Updater.exe` (v1.3.3.37)
  - `Portable Brave Updater.exe.org` (v1.3.0.0 legacy copy)
- Browser payload: `Brave\Brave.exe` (v156.1.98.63)
- Version folder: `Brave\156.1.98.63\`
- Version record: `Brave\updates\Version.log` = `156.1.98.63|Stable|x64`
- Profile: `profile\`, referenced by `Brave\Profile.txt` = `--user-data-dir="profile"`

The PortableApps.com Platform menu configuration contains residual Brave records:

```ini
[AppsFavorite]
braveportable\brave-portable.exe=1
braveportable\brave launcher.exe=1

[TimesRun]
braveportable\brave launcher.exe=121
```

These are per-executable menu statistics, **not** PAF app registration. `brave-portable.exe` does not exist; the entry is stale.

Conclusion: the Platform currently recognizes BravePortable as an "Other App" through root-level EXEs. No PAF metadata or icon is present.

## 3. Official PortableApps.com behavior

Relevant official rules:

- PAF apps are discovered through `App\AppInfo\appinfo.ini`.
- The Platform reads at least `Name`, `AppID` and `Start` from that file.
- The Platform menu icon for PAF apps is `App\AppInfo\appicon_16.png`.
- Non-PAF "Other Apps" are discovered through EXEs in the **first-level app folder only**.
- The Platform does **not** recursively scan subdirectories for non-PAF apps.
- The Platform does **not** treat `.lnk` shortcuts as apps.
- Manually added apps require **Apps > Refresh App Icons** or a Platform restart.
- There is no documented external CLI/API to trigger that refresh.
- A custom `AppID` that is not in the PortableApps.com App Store is **not** updated by the Platform updater.

Sources:

- PAF 3.9: https://portableapps.com/development/portableapps.com_format
- appinfo.ini: https://portableapps.com/manuals/PortableApps.comLauncher/ref/paf/appinfo.html
- Adding Other Apps: https://portableapps.com/support/platform#addingotherapps
- Root-only EXE scanning: https://portableapps.com/node/71581
- No external refresh: https://portableapps.com/node/57529

## 4. Can Brave update itself in this layout?

No reliable self-update was found in this portable extraction.

Inspection of `Brave\` and `Brave\156.1.98.63\` found:

- no `BraveUpdate.exe` or `BraveSoftwareUpdate.exe`
- no Omaha/GoogleUpdate components
- no Brave/Google update services
- no Brave/Google scheduled tasks
- no update-related strings in `Brave.exe`

Normal Brave installations use a separate updater component. This portable package does not include or register it. The custom Portable Brave Updater is therefore the intended update path.

## 5. Options considered

| Option | Description | Assessment |
|---|---|---|
| A. Other App + `Brave Launcher.exe` | Keep current non-PAF layout; Platform launches the existing Brave launcher | Simplest; recommended default |
| B. Other App + updater + `.pap.ini` | Platform launches `Portable Brave Updater.exe` with update/launch parameters | Optional update-on-start; no wrapper; recommended as optional mode |
| C. Metadata-only PAF | Add `App\AppInfo\appinfo.ini` and icons in place, keep binaries/data in root | Cosmetic only; not PAF-compliant; not planned |
| D. Full PAF | Move binaries to `App\`, profile to `Data\`, add root launcher, help, metadata | Not planned; conflicts with updater path layout |
| E. Wrapper `BravePortable.exe` | Extra executable that updates then starts Brave | Rejected; taskbar/process-tracking complexity |

## 6. Planned feature

### 6.1 Goal

Provide a documented, low-risk PortableApps.com integration for BravePortable without PAF conversion and without a wrapper executable.

### 6.2 Default mode

- The user copies the BravePortable folder to `P:\PA\PortableApps\BravePortable`.
- The Platform detects the root `Brave Launcher.exe` as an Other App after refresh/restart.
- No `.ini` or `.ico` is required for this basic recognition.
- `Brave Launcher.exe` remains the default Platform entry.
- The updater remains available as a separate root executable.

### 6.3 Optional update-on-start mode

Add next to `Portable Brave Updater.exe`:

```text
Portable Brave Updater.exe.pap.ini
```

Content:

```ini
[PortableApps.comPlatform]
Parameters=--update=existing --launch-brave --quit-after
```

For multiple installed editions, use an explicit edition:

```ini
[PortableApps.comPlatform]
Parameters=--update=existing --launch-brave=stable:x64 --quit-after
```

Encoding must be ANSI or UTF-16 LE with BOM.

Result:

1. The Platform starts the updater.
2. The updater updates installed Brave editions.
3. The updater starts Brave.
4. The updater exits; Brave continues running.

Known limitation: the Platform sees the updater process as ended. It does not track the child Brave process. This is already true for the current `Brave Launcher.exe`, which also exits after starting Brave.

### 6.4 Duplicate menu entries

Because both `Brave Launcher.exe` and `Portable Brave Updater.exe` are root EXEs, the Platform may show two entries.

Planned handling:

- keep both files on disk
- hide the unwanted entry in the Platform menu if desired
- do not remove `Brave Launcher.exe`; the updater uses it for single-folder launches

### 6.5 Planned deployment helper

A later optional helper:

```text
tools/Deploy-BravePortable.ps1
```

Planned behavior:

- copy/update the updater package into `P:\PA\PortableApps\BravePortable`
- preserve `Brave\`, `profile\`, `Bin\` and `Version.log`
- optionally create/update `Portable Brave Updater.exe.pap.ini`
- validate the updater and launcher paths
- print the manual Platform refresh instruction

The helper is documented but not implemented yet.

### 6.6 Acceptance criteria

- BravePortable remains a non-PAF app.
- No wrapper executable is added.
- Copying the folder and refreshing the Platform shows the app.
- No `.ini`/`.ico` is required for basic Other App recognition.
- Optional `.pap.ini` starts the updater with the documented parameters and launches Brave.
- `profile\` and `Brave\` are not moved or modified by the integration.
- The Platform is not modified programmatically.
- Documentation explains that the Platform does not auto-update custom `AppID`s.

### 6.7 Non-goals

- Full PAF conversion
- PortableApps.com App Store publication
- Platform-managed updates for a custom AppID
- Platform backup of `profile\`
- Wrapper executable
- Automatic Platform refresh

## 7. Rejected alternatives

### 7.1 Wrapper executable

Rejected because:

- A persistent wrapper adds a second process and can interfere with taskbar behavior.
- A wrapper that exits makes the Platform believe the app has closed.
- The Platform does not track the child Brave process for non-PAF apps.
- The existing `Brave Launcher.exe` already exits after starting Brave, so a wrapper does not solve process tracking.
- A clean solution would require PAF plus the PortableApps.com Launcher (`WaitForProgram`), which conflicts with the current updater layout.

### 7.2 Metadata-only PAF

Rejected for now because:

- It is cosmetic: the Platform would read `appinfo.ini` and show a name/icon.
- It is not PAF-compliant: binaries are outside `App\` and data is outside `Data\`.
- It creates false expectations about Platform backup and Platform updates.
- It adds maintenance without solving updates.

### 7.3 Full PAF

Rejected for now because:

- `Brave\` would need to move under `App\`.
- `profile\` (about 5.66 GiB) would need to move under `Data\`.
- The updater expects `Brave\`, `Bin\` and `profile\` relative to the app root.
- Moving the layout would require updater path changes or junctions.
- The Platform would still not update a custom `AppID`.
- The effort and risk are disproportionate for a small launcher.

## 8. Learned insights

- The current Platform recognition is based on root-level EXEs, not `appinfo.ini`.
- No `.ini` or `.ico` was required for the current non-PAF recognition.
- The Brave entries in `PortableAppsMenu.ini` are historical favorites/run statistics, not app registration.
- The Platform scans only the first level for non-PAF apps and ignores `.lnk` files.
- A custom `AppID` does not receive Platform App Store updates.
- PAF only provides app-data backup when data is under `Data\`.
- The Platform refresh cannot be triggered externally in a documented way.
- Brave in this portable extraction has no updater components and cannot reliably update itself.
- The custom updater is required for Brave updates in this layout.
- PAF would mainly be a one-time installation/menu helper for other users, not an update mechanism.
- A wrapper executable is not worth the taskbar/process-tracking complexity.

## 9. Open decisions

1. Should the default Platform entry be `Brave Launcher.exe` (fast start) or `Portable Brave Updater.exe` (update-on-start)?
2. Should the optional `.pap.ini` be included by default in the package or only documented?
3. Should duplicate Platform entries be hidden manually, or should the launcher file be moved later with updater changes?
4. Should the deployment helper be implemented now or only documented?

## 10. References

- PortableApps.com Format 3.9: https://portableapps.com/development/portableapps.com_format
- PAF appinfo.ini: https://portableapps.com/manuals/PortableApps.comLauncher/ref/paf/appinfo.html
- PAF layout: https://portableapps.com/manuals/PortableApps.comLauncher/ref/paf/layout.html
- Adding Other Apps: https://portableapps.com/support/platform#addingotherapps
- Root-only EXE scanning: https://portableapps.com/node/71581
- No external Platform refresh: https://portableapps.com/node/57529
- PortableApps.com Platform support: https://portableapps.com/support/platform
