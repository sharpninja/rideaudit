# Mermaid session flow (dual-phone RideAudit)

**Artifact:** ART-RIDE-UX-001  
**Related:** SB-01 .. SB-06, WF-01 .. WF-08  
**Chain default:** Bitcoin OpenTimestamps (`btc-ots`)

Sequence for one DualPhoneSession: Bluetooth pairing, driver coordination, passenger composite with telematics overlay, seal-at-collect, public sealed submit admission, and counsel viewer handoff.

```mermaid
sequenceDiagram
    autonumber
    actor Drv as Driver phone
    actor Pax as Passenger phone
    participant BT as Bluetooth link
    participant PI as Play Integrity
    participant OTS as Bitcoin OTS
    participant API as Public sealed API
    participant View as Counsel viewer

    Note over Drv,Pax: Role select (WF-01) GPL-2.0 client

    Drv->>PI: Attest collector
    Pax->>PI: Attest collector
    PI-->>Drv: OK / FAIL
    PI-->>Pax: OK / FAIL
    alt Integrity FAIL
        Drv-->>Drv: Fail-closed (WF-08)
        Pax-->>Pax: Fail-closed (WF-08)
    end

    Drv->>BT: Advertise RideAudit-Driver + invite
    Pax->>BT: Scan peers (WF-02)
    BT-->>Pax: Peer list
    Pax->>Drv: Pair request over BT
    Drv->>Pax: Confirm roles Driver/Passenger (WF-03)
    Pax->>Drv: Confirm complementary role
    Note over Drv,Pax: Session bound DualPhoneSession

    Drv->>Drv: Readiness gate (WF-04)
    Drv->>Pax: Publish clock epoch + START
    Pax->>Pax: SyncClockOffset, join streams (WF-05)
    Pax->>Pax: Realtime spider telematics overlay
    loop Active capture
        Drv->>Pax: Clock / health
        Pax->>Drv: Composite health / faults
    end

    Drv->>Pax: STOP
    par Seal-at-collect
        Drv->>Drv: Seal local packages (WF-06)
        Pax->>Pax: Seal composite + overlay (WF-06)
    end
    Drv->>OTS: Stamp custody receipt (btc-ots)
    Pax->>OTS: Stamp custody receipt (btc-ots)
    OTS-->>Drv: Proof pending / stamped
    OTS-->>Pax: Proof pending / stamped

    Drv->>Pax: Collect sealed artifacts for submit
    Drv->>API: Submit sealed ciphertext only (WF-07)
    API->>API: Verify seal + Integrity + receipt policy
    alt Admission OK
        API-->>Drv: Admitted submission id
        Drv-->>View: Handoff submission id (SB-06)
        View->>OTS: Independent verify .ots vs Bitcoin headers
    else Admission reject
        API-->>Drv: Reject reason (WF-08 fail-closed)
    end
```

## Notes

- Driver coordinates; passenger composites. Not a Lyft Bluetooth API.
- Seal-at-collect happens before upload. No decrypt at ingest.
- Primary custody receipt path is Bitcoin OpenTimestamps. Optional L2 fast-confirm only when dual-anchor config is enabled (`docs/architecture/blockchain-custody-receipts.md`).
