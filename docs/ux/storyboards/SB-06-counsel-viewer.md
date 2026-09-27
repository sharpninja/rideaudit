# SB-06 Counsel viewer overview

**Artifact:** ART-RIDE-UX-001  
**FR links:** FR-RIDE-046, FR-RIDE-017, FR-RIDE-018, FR-RIDE-045  
**Surfaces:** Desktop court/counsel viewer (not Android primary); referenced from mobile submit success

## Goal

Counsel and auditors review sealed dual-phone evidence with provenance: seal metadata, Play Integrity attestation summary, Bitcoin OTS custody receipt, SyncClockOffset, composite with spider-graph overlay, and escrow/decryption path status (no casual plaintext).

## Actors

- Counsel / Auditor
- Desktop viewer application
- Escrow / dual-control release process (legal process; not in-app casual decrypt)

## Beats

1. **Open submission**  
   Viewer loads admitted submission by id. Shows driver/passenger roles, vehicle id, session clock epoch, and package list.

2. **Provenance panel**  
   For each package: content hash, seal timestamp, attestation result, OTS proof status (verify against Bitcoin headers), collector_id. Failures flagged; incomplete proofs not presented as court-ready.

3. **Composite review**  
   Playback of sealed composite (after lawful decrypt path) with telematics overlay visible as captured. Spider graph timeline scrubbable against shared clock.

4. **Coverage / gaps**  
   Coverage matrix and Unverified caveats from requirements remain visible where Lyft-native signals are absent. No invented Lyft APIs.

5. **Export for disclosure**  
   Export sealed package + portable `.ots` + attestation summary for opposing counsel independent check.

## Success criteria

- Viewer surfaces dual-phone roles and composite as first-class evidence.
- Public OTS verification path is explainable without trusting RideAudit servers alone.
- Decryption follows escrow/legal process; viewer does not imply casual plaintext access.

## Notes

UI here is an overview storyboard for desktop counsel tooling. Detailed wireframes in this ART focus on Android capture; counsel screens are narrative until a dedicated desktop ART exists.
