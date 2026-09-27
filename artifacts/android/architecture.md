# Android client architecture

License: GPL-2.0

## Dual-phone roles (Bluetooth pairing)

Phones discover each other with **Bluetooth discovery** and form a **driver-rider pairing**:

| Role | Device | Responsibilities |
|------|--------|------------------|
| **Driver phone** | Primary / coordinator | Session clock master; admit, start, and stop the session; seal admission orchestration; submission coordination to the public API; holds driver account token and vehicle binding. |
| **Passenger (rider) phone** | Secondary | Video sync to the driver clock; joining and compositing streams; realtime telematics including spider-graph overlay into the composite. |

No Lyft private APIs are used for pairing or telematics. Discovery and session control stay on-device over Bluetooth (plus local Wi-Fi Direct or similar only if later specified by an FR). Platform trip data still comes only from documented exports / permitted integrations elsewhere in the product.

## Module overview

| Module | Responsibility |
|--------|----------------|
| **collect** | Camera, location, IMU/sensor capture on each phone; Bluetooth discovery and driver-rider role assignment. |
| **seal** | Encrypt payloads at collection on the coordinating path; build CustodyReceipt; never persist uploadable plaintext. |
| **attest** | Play Integrity gate before keygen on both phones as required; bind verdict into receipt; fail-closed on failure. |
| **composite** | Passenger phone joins streams, aligns to driver clock, applies spider-graph telematics overlay; composite is first-class sealed evidence. |
| **upload** | Driver phone coordinates resumable sealed submission; chunks and admission-status polling. |
| **identity** | Driver account token and vehicle registration on the driver phone; configuration profile gate. |

## Data flow (happy path)

1. **collect** runs Bluetooth discovery; phones agree on Driver vs Passenger roles.
2. **identity** on the driver phone authenticates the driver and confirms registered vehicle + config profile.
3. **attest** runs Play Integrity (fail-closed) before keygen on devices that will seal.
4. Driver phone admits the passenger phone into the session and starts capture as **session clock master** (FR-RIDE-042).
5. Passenger phone syncs video to the driver clock, composites streams, and adds realtime telematics (spider-graph overlay) (FR-RIDE-041, FR-RIDE-043, FR-RIDE-044).
6. **seal** (orchestrated by the driver phone) encrypts composite (and optional raw streams); emits CustodyReceipt with attestation binding.
7. **upload** from the driver phone posts sealed blob reference + receipt metadata; never plaintext video/sensors.

## Fail-closed points

- Missing or failed Play Integrity: no keygen, no seal, no upload.
- Unregistered vehicle or missing config profile: session blocked on driver phone.
- Bluetooth pairing or role negotiation failure: session does not start.
- Attempt to enqueue plaintext media: rejected in seal/upload layers.

## Boundaries

- Court decryption and M-of-N escrow live off-device (server / escrow services).
- Blockchain receipt write is invoked after local seal when connectivity allows; offline policy is covered by FR-RIDE-019.
- Driver phone owns submission coordination; passenger phone does not independently admit sealed evidence to the public API.
