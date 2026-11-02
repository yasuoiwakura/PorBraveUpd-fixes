# PorBraveUpd-fixes - Project Documentation

Last updated: 2026-11-02  
Base: fork `yasuoiwakura/PorBraveUpd-fixes`  
Upstream: `UndertakerBen/PorBraveUpd`

## Goal

This fork keeps small, well-scoped improvements to the Portable Brave Updater. The main focus is a non-interactive CLI that can be used as a pre-launcher or as an automated update job.

Planned core features:

- `--update=existing` as default: update only installed editions.
- `--update=all`: install and update all supported editions.
- `--launch-brave`: start the matching Brave edition after updating.
- `--quit-after`: exit the updater after completion.
- `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave`.
- Self-update points to fork URLs; release assets are provided by the fork release.

## Decisions

| Topic | Decision |
|---|---|
| Language | Keep C# |
| Target framework | .NET Framework 4.8 |
| Launchers | Retarget to .NET Framework 4.8 as well |
| CLI style | GNU/POSIX long options with `--`, no `/` switches |
| Update scope | `--update=existing` (default), `--update=all` |
| Launch | `--launch-brave`; prefer the existing launcher |
| Self-update | Fork URLs; release assets provided by the fork |
| Build | Visual Studio 2022 Build Tools + MSBuild |
| Documentation | Spec-driven: analysis, build, CLI spec, architecture, test plan, release/license, ADRs |

## Documents

| File | Content |
|---|---|
| `00-ANALYSIS.md` | Current state, feasibility, license, recommendation |
| `01-BUILD.md` | Toolchain, installation, retargeting, build, CI |
| `02-CLI-SPEC.md` | Authoritative CLI specification including exit codes |
| `03-ARCHITECTURE.md` | Current state, target architecture, refactoring plan |
| `04-TEST-PLAN.md` | Test matrix, test cases, acceptance criteria |
| `05-RELEASE-LICENSE.md` | MIT, trademarks, 7-Zip, release checklist |
| `06-BEST-PRACTICES.md` | CLI, build, security and testing best practices |
| `07-INSTALLATION-LOG.md` | Build environment state and exact commands |
| `RELEASES.md` | Release index and note on milestones |
| `RELEASE-NOTES-1.3.3.37.md` | Release notes for 1.3.3.37 |
| `RELEASE-NOTES-1.3.3.7.md` | Release notes for 1.3.3.7 |
| `../CHANGELOG.md` | Release history |
| `adr/0001-keep-csharp.md` | ADR: no rewrite |
| `adr/0002-target-net48.md` | ADR: .NET Framework 4.8 instead of 4.0/4.7 |
| `adr/0003-cli-conventions.md` | ADR: long options and semantics |

## Status

- Analysis: done
- Documentation: done (English)
- Build environment: configured (2026-11-02)
- Retargeting to .NET Framework 4.8: done (2026-11-02)
- GitHub URLs switched to fork: done (2026-11-02)
- Launchers at 1.3.3.7 and `Launcher.7z` rebuilt: done (2026-11-02)
- Updater version: 1.3.3.37
- Build: successful (2026-11-02)
- Local release package: `dist/Portable.Brave.Updater.v1.3.3.37.7z`
- GitHub Actions: `build`, `ci`, `release`; dev builds are artifacts
- CLI implementation: done (MVP)
- CLI smoke tests: `--help`, `--version`, invalid arguments, `--check`, `--dry-run`, `--quiet`, `--log`, `--wait-for-brave` (2026-11-02)
- Baseline smoke test with a real update: open
