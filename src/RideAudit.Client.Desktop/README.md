# RideAudit.Client.Desktop

Avalonia UI 12 court and counsel viewer for Windows, Linux, and macOS (FR-RIDE-049, FR-RIDE-057).

## Build and run

```bash
dotnet run --project src/RideAudit.Client.Desktop
```

`dotnet run` needs a desktop session. Headless verification of the shared views is in `tests/RideAudit.Client.Tests`.

## Behavior

The viewer loads sealed bundles, verifies custody (OTS-shaped proof, payload hash, Play attestation, nonce/key binding) and refuses decrypt or playback when any check fails. Decrypt uses a court-authorized M-of-N escrow release into an expiring working copy. A synchronized timeline then shows composite, spider-graph, telematics, and labels absent GPS or OBD2 tracks instead of inventing them.

This project does not call Lyft private APIs.

## What this build is not

- Not road-ready.
- Not a signed reproducible Windows, Linux, or macOS release. Those hosts were not all executed here.
- Not connected to a live escrow service. Tests supply an off-device quorum stub.

License: GPL-2.0-or-later. See the repository `LICENSE`.
