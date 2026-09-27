# RideAudit Android Client Artifact

**Artifact ID:** ART-RIDE-ANDROID-001  
**Kind:** android-client  
**Version:** 0.1.0  
**License:** GPL-2.0

## Purpose

This package scaffolds the RideAudit Android client used by rideshare drivers to collect dual-phone video and telematics, seal ciphertext at the point of capture, and upload sealed submissions to the public server API. It is a design and source skeleton, not a Play Store release build.

## Dual-phone collection (Bluetooth driver-rider pairing)

Phones find each other via **Bluetooth discovery** and form a **driver-rider pairing**. Typical mounts place both devices behind the headrest (see `artifacts/hardware/headrest-phone-mount/`):

- **Driver phone (coordinator):** session clock master; admits, starts, and stops the session; orchestrates seal admission; coordinates sealed submission to the public API.
- **Passenger (rider) phone:** video sync to the driver clock; joins and composites streams; adds realtime telematics including spider-graph overlay.
- Composite output is first-class evidence with custody metadata.
- Pairing and session control do **not** use Lyft private APIs.

## Play Integrity

Before session key generation, the client must obtain a Play Integrity verdict. Failed, missing, or unallowlisted attestation causes fail-closed behavior: no keygen, no seal, no upload. Attestation results bind into the custody receipt.

## Seal-at-collect

Plaintext video and sensor samples never leave the device. Encryption and custody-receipt construction happen at collection time (per-session or per-sample keys). The upload path accepts only sealed ciphertext plus CustodyReceipt metadata. The driver phone coordinates seal admission and upload.

## License (GPL-2.0)

```
RideAudit Android client artifact
Copyright (C) 2026 RideAudit contributors

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
```

## Status

Skeleton only. Gradle files and Kotlin stubs illustrate module boundaries. Play Store publishing is **not** done. See FR-RIDE-031 for the publishing requirement track.

## Related docs

- [architecture.md](architecture.md)
- [package-structure.md](package-structure.md)
- [play-integrity.md](play-integrity.md)
- [permissions.md](permissions.md)
- [ARTIFACT.yaml](ARTIFACT.yaml)
