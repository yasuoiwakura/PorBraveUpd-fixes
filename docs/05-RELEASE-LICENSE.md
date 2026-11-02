# 05 - Release and License

Last updated: 2026-11-02

## 1. MIT license

Upstream is licensed under MIT (Copyright (c) 2020 UndertakerBen). MIT allows:

- use and copying
- modification and merging
- publication and distribution
- sublicensing
- sale, including commercial use

Condition: the copyright notice and MIT license text must remain in all copies or substantial portions.

### Obligations for the fork

- Include `LICENSE` unchanged.
- Add your own copyright, e.g. `Copyright (c) 2026 yasuoiwakura`.
- Point to the MIT license in the release package and repository.
- Make clear that this is an unofficial fork.

## 2. Trademarks and Brave

MIT does not grant trademark rights.

- "Brave", Brave logos and related icons are trademarks of their respective owners.
- The existing README disclaimer must remain.
- Do not imply an official connection to Brave.
- For broad distribution, consider replacing Brave icons or clearly marking the project as an unofficial fork.

## 3. 7-Zip / 7zr.exe

The release contains `Bin/7zr.exe`.

- `7zr.exe` from the LZMA SDK is public domain.
- `7zr.exe` from the 7-Zip distribution is under GNU LGPL; license information must be reproduced when redistributed.
- Recommendation: use `7zr.exe` from the LZMA SDK and include a short third-party notice.
- Keep the 7-Zip license text in `THIRD-PARTY-NOTICES.txt`.

## 4. Brave installers and terms of service

The updater downloads Brave installers from Brave servers at runtime. This is existing upstream behavior.

For public distribution:

- Review Brave's terms of service.
- Do not redistribute Brave installers in your own release unless explicitly permitted.
- Do not imply official approval.

This is not legal advice.

## 5. Self-update and update URLs

The GitHub URLs are centralized in the code and point to the fork:

- `Form1.cs`: `RepositoryUrl` = `https://github.com/yasuoiwakura/PorBraveUpd-fixes`
- `Form1.cs`: `RawMasterUrl` = `https://raw.githubusercontent.com/yasuoiwakura/PorBraveUpd-fixes/master`

Used for:

- `Version.txt` (self-update and update button)
- `Launcher/Version.txt`
- `Launcher/Launcher.7z`
- release asset download

Important:

- Self-update is still active but points to the fork.
- The fork's updater and `Version.txt` are at `1.3.3.7`; a 1.3.3.7 build therefore does not offer a self-update.
- Older 1.3.0.0 builds can update only after the matching release asset exists.
- Before bumping the version, publish `Portable.Brave.Updater.v<Version>.7z` under the fork; otherwise the self-update download fails.
- Self-update can be disabled later if desired.

For a release:

- Keep release asset names consistent: `Portable.Brave.Updater.v<Version>.7z`.
- Update `Version.txt` in the repository root.
- Update `Launcher/Version.txt` if launchers are rebuilt.
- Optionally add hash verification to self-update.

## 6. Versioning

Keep these consistent:

| Location | Meaning |
|---|---|
| `Properties/AssemblyInfo.cs` | File version of the updater EXE |
| `Version.txt` | Version read by self-update |
| `Launcher/Version.txt` | Launcher update version, currently `1.3.3.7` |
| `Launcher/Launcher.7z` | Launcher archive used by `CheckLauncher()` |
| Release tag | e.g. `v1.3.3.7` |
| Release asset | `Portable.Brave.Updater.v1.3.3.7.7z` |
| Dev artifact | Timestamped Actions artifact, not self-update capable |

Recommendation: keep semantic versioning or the existing four-part .NET version.

Important:

- Updater and launchers are currently at `1.3.3.7`.
- Before a stable release, set `Version.txt` and `AssemblyFileVersion` of the updater to the same new version.
- Dev artifacts must never change `Version.txt`.

## 7. Release checklist

### Before the build

- [ ] Target framework is .NET Framework 4.8.
- [ ] All launchers are retargeted to 4.8 or intentionally taken from a release.
- [ ] Self-update is disabled or points to fork URLs.
- [ ] Version numbers are consistent.
- [ ] `LICENSE` and copyright are current.
- [ ] README disclaimer exists.

### Build and test

- [ ] Main project builds in Release.
- [ ] Launchers build or release launchers are used.
- [ ] `Launcher/Launcher.7z` was rebuilt with the new launchers.
- [ ] `Launcher/Version.txt` matches the launcher version (`1.3.3.7`).
- [ ] `Bin/7zr.exe` exists.
- [ ] Smoke test: `--help`, `--version`, `--check`.
- [ ] Update test of one edition.
- [ ] GUI regression test.
- [ ] Relevant tests in `04-TEST-PLAN.md` pass.

### Package

- [ ] `Portable Brave Updater.exe`
- [ ] `Bin/7zr.exe`
- [ ] `Bin/Launcher/*.exe`
- [ ] `LICENSE`
- [ ] `README.md`
- [ ] `THIRD-PARTY-NOTICES.txt`
- [ ] Optional: `CHANGELOG.md`

### Publication

- [ ] Generate SHA256 checksums.
- [ ] Optional: sign the EXE with Authenticode.
- [ ] Create GitHub release with tag and asset.
- [ ] Tag points to `master`; `Version.txt` on `master` is current.
- [ ] Create release notes under `docs/RELEASE-NOTES-<Version>.md`.
- [ ] Update `Version.txt` only for stable releases.
- [ ] Publish dev builds only as timestamped Actions artifacts; leave `Version.txt` unchanged.
- [ ] Test the self-update path if enabled.

## 8. Security

- No unverified self-replacement.
- If self-update is enabled:
  - enforce HTTPS,
  - verify SHA256,
  - optionally verify signature,
  - do not execute on hash mismatch.
- Optionally verify Brave installers via Authenticode.
- Do not request admin rights.
- Do not log secrets.

## 9. Third-party licenses

| Component | License | Notice |
|---|---|---|
| PorBraveUpd | MIT | `LICENSE` |
| 7zr.exe | LZMA SDK public domain or 7-Zip LGPL | Third-party notice |
| Brave | Brave trademarks and terms | Disclaimer, no official connection |
| .NET Framework | Microsoft | Windows runtime component |

## 10. References

- MIT License: https://opensource.org/license/mit
- 7-Zip License: https://7-zip.org/license.txt
- .NET Framework Support: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
