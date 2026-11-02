# ADR 0001 - Keep C#, no rewrite

- Status: accepted
- Date: 2026-11-02

## Context

Portable Brave Updater is a Windows WinForms application written in C# (.NET Framework). Small CLI extensions are planned. We considered rebuilding the project in another language for these changes.

## Decision

The project remains in C#. It will be refactored selectively, not rewritten.

## Rationale

- The planned change is small.
- A rewrite would have to recreate WinForms, the Omaha protocol, 7z extraction, COM shortcuts, launcher profiles and self-update.
- C#/.NET Framework is natively available on Windows 11.
- MIT permits further development.
- A language switch provides no functional benefit but carries a high regression risk.

## Consequences

- Existing project files and the build toolchain remain relevant.
- The launchers' .NET Framework 4.0 target must be changed to 4.8.
- Refactoring is required to separate UI and update core.
- A later modernization to .NET 8/9 WinForms remains possible but is not part of this decision.

## Alternatives

| Alternative | Assessment |
|---|---|
| Go | GUI, COM, 7z and launchers would need to be rebuilt; high effort |
| Rust | Similar, with a higher entry barrier |
| Python | Runtime/packaging problems, AV false positives, no advantage |
| .NET 8 WinForms | Possible, but a larger port; not necessary for this change |
