# 09 - Future PAF Build

Status: **documented option, not implemented.**

The current PortableApps integration is metadata-only. It is not a complete
PortableApps.com Format (PAF) package. This document describes a clean PAF build
for a later release.

## 1. Naming

The package ships **Portable Brave Updater**, not Brave itself. The directory
and launcher name should reflect that and avoid the misleading `BravePortable`
name.

Recommended names:

| Item | Value |
|---|---|
| Product name | `Portable Brave Updater` |
| AppID | `PortableBraveUpdater` |
| Package directory | `PortableBraveUpdater` |
| Root launcher | `PortableBraveUpdater.exe` |
| Release asset | `PortableBraveUpdater_<version>.paf.exe` |
| Online installer asset | `PortableBraveUpdater_<version>_online.paf.exe` |

`BravePortable` should remain the existing development/live folder until a
migration is planned and tested. It should not be used for the future PAF
package.

## 2. What the PAF package contains

PAF should package the updater and its metadata. Brave is managed data, not the
application being shipped.

Recommended target layout:

```text
PortableBraveUpdater\
  PortableBraveUpdater.exe          PAF launcher
  App\
    AppInfo\
      appinfo.ini
      installer.ini
      appicon*.ico
      appicon_*.png
      Launcher\
        PortableBraveUpdater.ini
    PortableBraveUpdater\
      Portable Brave Updater.exe
      Portable Brave Updater.exe.config
      Bin\
        7zr.exe
        Launcher\*.exe
    Brave\                           optional bundled payload
  Data\
    profile\                         Brave user profile
    Brave\                           optional managed Brave installation
    settings\                        updater settings and logs
  Other\
    help.html
    Source\
  README.md
```

The exact Brave location is an open decision:

- `Data\Brave` preserves the managed installation across PAF upgrades.
- `App\Brave` is more conventional for shipped payload but must be preserved by
  `installer.ini` or it will be replaced on upgrade.
- Bundling Brave makes the installer large. An updater-only or online package
  is smaller but does not provide a ready-to-run browser immediately.

## 3. PAF build pieces

### 3.1 appinfo.ini

Keep the current entry model:

- `Portable Brave Updater`
- `Brave`
- `Brave (newest stable)`

Use `DescriptionN` for the Brave entries:

- `Brave`: `stable, x64`
- `Brave (newest stable)`: `Update Brave, then start Brave`

Do not derive a Brave version into the package metadata; that creates
unnecessary coupling. `PackageVersion` remains for installer/update purposes;
`DisplayVersion` is optional and intentionally omitted in the current metadata.

### 3.2 Launcher configuration

Add `App\AppInfo\Launcher\PortableBraveUpdater.ini`.

Minimum areas to define:

- `ProgramExecutable`
- `DirectoryMoveOK=yes`
- `SupportsUNC=yes`
- data directory mapping for the Brave profile
- settings/log locations
- registry use, if any, with cleanup rules
- process tracking for Brave and the updater

The PAF launcher should own process tracking and cleanup. This removes the
current metadata-only limitation where the Platform cannot reliably detect that
Brave is still running.

### 3.3 Updater path refactor

The updater currently assumes paths relative to the app root:

- `Brave\`
- `profile\`
- `Bin\`
- root launcher files

For PAF, introduce a path layer:

- app directory: `App\PortableBraveUpdater`
- updater binary: `App\PortableBraveUpdater\Portable Brave Updater.exe`
- Brave payload: `App\Brave` or `Data\Brave`
- profile: `Data\profile`
- settings/logs: `Data\settings`

The updater should use PAF launcher environment variables where available
(`%PAL:AppDir%`, `%PAL:DataDir%`, `%PAL:PackageDir%`) or a small path-config
file. Avoid junctions as the primary solution.

### 3.4 installer.ini

Add `App\AppInfo\installer.ini` for:

- directories that must be preserved on upgrade
- optional download/extraction rules if the package is updater-only or online
- additional install size

### 3.5 Icons

Current generated ICO files contain 16, 24, 32, 48, 64, 128 and 256 pixel
32-bit frames. Full PAF 3.9 compliance also asks for 8-bit 16, 32 and 48 pixel
frames. Add those frames in a future PAF icon generator.

### 3.6 Build with PortableApps.com Installer

Use the installed PortableApps.com Installer:

```text
P:\PA\PortableApps\PortableApps.comInstaller\PortableApps.comInstaller.exe
```

Version observed locally: `3.9.22.0`, PAF format `3.9`.

The installer already handles PortableApps detection:

- checks `%USERPROFILE%\PortableApps`
- scans drives for `PortableApps\PortableApps.com\PortableAppsPlatform.exe`
- verifies product and company metadata
- falls back to a folder choice
- supports automated options such as `/AUTOCLOSE=true`, `/HIDEINSTALLER=true`
  and `/SILENT=true`

A custom installer would duplicate this logic and remain non-standard.

## 4. Self-update policy

An app that updates itself is a poor fit for PAF lifecycle management. If the
updater replaces its own EXE outside the installer, the installed file set,
`appinfo.ini`, and uninstall metadata can drift.

Recommended PAF policy:

- PAF packages are updated through the PAF installer / PortableApps Platform.
- The updater continues to update **Brave**, not itself, in PAF mode.
- Disable or hide the updater's self-update path when running as PAF.
- Keep self-update available only for the current ZIP/7z development layout if
  needed.

This is a deliberate behavior difference, not a bug.

## 5. Other portable launchers

PortableApps.com is only one launcher ecosystem. The updater should not become
hard-coupled to PAF.

Recommended abstraction:

- Core updater remains launcher-agnostic.
- A launcher adapter describes:
  - app name
  - install directory
  - launch commands
  - icon source
  - descriptions
  - data directory
  - process-tracking behavior
  - update/install strategy
- Current metadata-only PAF is one adapter.
- A future full PAF build is another adapter.
- Generic folder/shortcut use remains the baseline.

Candidate launchers to evaluate later:

| Launcher | Route |
|---|---|
| PortableApps.com Platform | Full PAF package |
| Generic portable folder | Direct EXE + shortcuts |
| SyMenu | Verify PAF/import support and shortcut model |
| LiberKey | Verify own app format/import model |
| PStart | Shortcuts and menu entries |
| ASuite | Shortcuts and menu entries |
| CodySafe | Verify own app format |
| WinPenPack | Verify own app format |

Do not claim support for a launcher until its install/update model has been
tested.

## 6. Complexity

| Work item | Complexity | Risk |
|---|---|---|
| Rename package/folder/AppID | Low | Migration and stale shortcuts |
| Add PAF launcher config | Medium | Process tracking and path mapping |
| Refactor updater paths | Medium/High | Every update/launch path must be tested |
| Move profile to `Data` | Medium | Profile migration and backups |
| Preserve managed Brave across upgrades | Medium/High | Installer rules and update logic |
| Add full PAF icon frames | Low | Regenerate and verify |
| Build `.paf.exe` | Medium | Installer source layout, testing |
| Disable self-update in PAF | Low/Medium | Behavior split between layouts |
| Multi-launcher adapters | Medium | Scope creep and untested claims |

A PAF wrapper around the current layout is possible, but it still requires the
path refactor. It is not only a packaging change.

## 7. Migration outline

1. Back up the current `BravePortable` directory.
2. Create a new `PortableBraveUpdater` staging directory.
3. Move Brave payload and profile into the chosen PAF paths.
4. Add launcher, AppInfo, installer files and icons.
5. Build the PAF installer.
6. Install to a test PortableApps instance.
7. Test:
   - Platform detection and install
   - Brave update
   - Brave launch and process tracking
   - PAF upgrade with profile and managed Brave preserved
   - uninstall behavior
8. Keep the old folder until the new installation is verified.
9. Clean stale favorites in `PortableAppsMenu.ini` after migration.

## 8. Open decisions

1. Updater-only package, online package, or bundled Brave?
2. Is the target private use or a public release?
3. Is PAF the only launcher target, or should adapters for SyMenu/LiberKey/etc.
   follow?
4. Should the managed Brave installation live in `Data\Brave` or `App\Brave`
   with installer preservation?
5. Migrate the existing live installation or perform a clean install?
6. Keep the folder name `PortableBraveUpdater` or choose another AppID?

## 9. References

- PAF appinfo: `P:\PA\PortableApps\PortableApps.comLauncher\App\Manual\_sources\ref\paf\appinfo.txt`
- PAF icons: `P:\PA\PortableApps\PortableApps.comLauncher\App\Manual\_sources\ref\paf\icons.txt`
- PortableApps.com Installer: `P:\PA\PortableApps\PortableApps.comInstaller`
- Current integration notes: `08-PORTABLEAPPS-INTEGRATION.md`
- PAF format: https://portableapps.com/development/portableapps.com_format
