# ADR 0002 - Target .NET Framework 4.8

- Status: accepted
- Date: 2026-11-02

## Context

The main project originally targeted .NET Framework 4.7 and the nine launchers targeted .NET Framework 4.0. Visual Studio 2022 cannot build 4.0 out of the box. .NET Framework 4.0 has been out of support since 2016. .NET Framework 4.8 is active and part of Windows 11.

## Decision

Retarget the main project and launchers to **.NET Framework 4.8**.

## Rationale

- 4.8 is actively supported and available on Windows 11.
- VS 2022 can build 4.8 without a legacy targeting pack.
- No Visual Studio 2019 and no 4.0 targeting pack are required.
- Low code risk: the APIs used are available in 4.8.
- 4.8.1 is also supported, but 4.8 is the broader portable baseline.

## Consequences

- `Portable Brave Updater.csproj`: `TargetFrameworkVersion` set to `v4.8`.
- All launcher CSProjects: `TargetFrameworkVersion` set to `v4.8`.
- `app.config` optionally updated to 4.8.
- Launchers must be rebuilt unless taken from a release.
- Minimum requirement: Windows with .NET Framework 4.8 (Windows 10 1903+ / Windows 11).

## Alternatives

| Alternative | Assessment |
|---|---|
| Keep 4.0 | EOL, build only with VS 2019/workarounds |
| Keep 4.7 | Active, but requires an additional targeting pack |
| 4.8.1 | Active, but a newer minimum requirement |
| .NET 8/9 | Larger port, not needed for this change |
