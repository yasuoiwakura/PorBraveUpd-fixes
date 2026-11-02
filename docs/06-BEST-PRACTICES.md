# 06 - Best Practices

Last updated: 2026-11-02

## 1. CLI design

- Long options with `--`, no `/` switches.
- Always support `--help` and `--version`.
- Accept `--option=value` and `--option value`.
- Compare option names and values case-insensitively (Windows-friendly).
- Use `--` as a separator for pass-through arguments.
- No interactive prompts in non-interactive mode.
- Clear exit codes; `0` for success/no action.
- Progress on stdout, errors on stderr.
- Idempotent operations: a second run without a new version changes nothing.
- `--check` and `--dry-run` must not trigger downloads.
- `--quiet` must not show a message box or window.
- Unknown options: error + help hint, exit 2.

### Parser

A **hand-written parser** is recommended for this project:

- No additional NuGet dependency.
- Full control over Windows-style spellings.
- Easy to test.
- No .NET Framework compatibility risk from external packages.

`System.CommandLine` is an alternative but adds unnecessary complexity for this small CLI.

## 2. .NET Framework best practices

- Target **.NET Framework 4.8**.
- Do not target .NET Framework 4.0 (EOL).
- Do not target 4.7 if 4.8 is possible without extra effort.
- Keep `AnyCPU`; x86/x64 are covered by editions.
- Keep the old `WebClient`/`WebRequest` APIs in the MVP; consider `HttpClient` later.
- Replace `Thread.Sleep` with `Task.Delay`.
- Use `async`/`await` consistently; no blocking network calls on the UI thread.
- Use `ConfigureAwait(false)` in the core; marshal UI updates through `IProgress`.
- Provide `CancellationToken` for cancellable operations.

## 3. Architecture

- Separate UI and business logic.
- `Form1` becomes an adapter, not the home of business logic.
- `UpdateService` knows no controls.
- Report progress via `IProgress<UpdateProgress>`.
- Handle errors via exceptions + central mapping to exit codes.
- No `MessageBox` in the core.
- No parallel updates; use a `SemaphoreSlim` or a sequential loop.
- Model editions as objects instead of parallel arrays.
- Keep configuration (URLs, self-update) centralized and changeable.

## 4. Build and CI

- Document the toolchain and keep it in the repository.
- Use `build.ps1` for reproducible builds.
- Run CI on `windows-latest` with MSBuild.
- Release artifacts include version and checksum.
- Dev builds are timestamped CI artifacts; never change `Version.txt` for them.
- Do not commit build artifacts.
- Obtain `Bin/` assets from a release or generate them during the build.
- Run NuGet restore explicitly.
- Remove old analyzers if they break the build.

## 5. Security

- Control self-update deliberately; it currently points to the fork, while hash verification is still outstanding.
- If self-update is active: HTTPS, SHA256, optional signature.
- Optionally verify Brave installers via Authenticode.
- Do not execute on hash mismatch.
- Do not request admin rights.
- Do not log secrets or tokens.
- Download only from expected hosts.
- Optionally sign the release EXE to reduce SmartScreen warnings.

## 6. Testing

- Parser tests without network.
- Exit-code tests.
- Test `--check`/`--dry-run` without downloads.
- Integration tests on a VM or in a test directory.
- Manual GUI regression tests.
- Actively test error cases: network, missing files, running Brave.
- Isolate test data; do not endanger a production installation.

## 7. Logging

- Consistent format with timestamps.
- Levels: INFO, WARN, ERROR.
- `--quiet`: no non-essential output.
- `--log`: additional file; directory must exist.
- Errors always on stderr.
- Do not log personal data.

## 8. Documentation

- Spec before implementation.
- ADRs for architecture decisions.
- Test plan with acceptance criteria.
- Release checklist.
- Record changes in `CHANGELOG.md`.
- Add CLI examples to README once implemented.

## 9. References

- .NET Framework Support Policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- VS Build Tools components: https://learn.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools
- Microsoft.NETFramework.ReferenceAssemblies: https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies
- 7-Zip License: https://7-zip.org/license.txt
- MIT License: https://opensource.org/license/mit
