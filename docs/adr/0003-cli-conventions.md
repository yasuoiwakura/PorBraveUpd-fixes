# ADR 0003 - CLI conventions

- Status: accepted
- Date: 2026-11-02

## Context

The updater previously had no command-line arguments. A CLI is needed for automation and launcher operation. We considered Windows `/` switches and GNU/POSIX long options.

## Decision

Use **GNU/POSIX long options with `--`**.

- `--update=existing` (default)
- `--update=all`
- `--launch-brave`
- `--quit-after`
- `--check`
- `--dry-run`
- `--quiet`
- `--log`
- `--wait-for-brave`
- `--timeout`
- `--help`
- `--version`

`--update all` and `--update existing` are accepted as tolerant spellings.

## Rationale

- `--` is cross-platform and tool-friendly.
- `/` conflicts with Windows paths and is unusual in many CLI ecosystems.
- Long, descriptive names are self-explanatory.
- `--launch-brave` is clearer than `--autostart`.
- `--quit-after` is clearer than `--autoclose`.
- `--if-new` is the safe default; `--force` is explicit.

## Consequences

- The parser must support `--option=value` and `--option value`.
- `--` separates updater and Brave arguments.
- Unknown options result in exit 2.
- Documentation must include examples.
- Windows users used to `/` are directed to `--`.

## Alternatives

| Alternative | Assessment |
|---|---|
| `/updateall` | Windows-typical but collision-prone and not best practice |
| Short options `-u`, `-l` | Optional later; not needed for the MVP |
| Environment variables only | Poor discoverability, poor testability |
| JSON configuration file | Unwieldy for scheduled tasks |
