# Mermaid review workflow (desktop court / counsel)

**Artifact:** ART-RIDE-UX-REVIEW-001  
**UI:** Avalonia UI 12 (Windows, Linux, macOS)  
**Backend:** gRPC on .NET 10 (containers)  
**Related:** SB-R-01 .. SB-R-06, WF-R-01 .. WF-R-08  
**Chain default:** Bitcoin OpenTimestamps (`btc-ots`)

Sequential path: open admitted RideBundle, load sealed packages (no decrypt), independent verification gate, fail-closed vs pass, escrow / CourtRelease (M-of-N), decrypt to expiring working copy, synchronized timeline playback, coverage/gaps, export disclosure, close ViewerSession.

```mermaid
sequenceDiagram
    autonumber
    actor Counsel as Counsel / Auditor
    participant App as Avalonia review app
    participant Gate as Verification gate
    participant OTS as Bitcoin OTS
    participant Escrow as Escrow M-of-N
    participant API as gRPC .NET 10 services
    participant WC as Expiring working copy

    Counsel->>App: Open case / RideBundle (WF-R-01)
    App->>API: Fetch admitted submission metadata
    API-->>App: Bundle index (sealed refs only)
    App->>App: Start ViewerSession (FR-RIDE-052)
    App->>App: Load sealed packages list (WF-R-02)
    Note over App: No decrypt yet

    App->>Gate: Run independent verification
    Gate->>OTS: Verify .ots vs Bitcoin headers
    Gate->>Gate: Check payload hashes
    Gate->>Gate: Check Play Integrity / signing cert + nonce/key binding
    Gate->>Gate: Check escrow release authorization present/valid
    Gate-->>App: VerificationReport draft (WF-R-03)

    alt Any check FAIL / missing / inconsistent
        App-->>Counsel: Fail-closed block (WF-R-04)
        Note over App: No decrypt, no plaintext display
        App->>App: Persist VerificationReport FAIL + audit event
    else All checks PASS
        Counsel->>App: Request CourtRelease attach (WF-R-05)
        App->>Escrow: M-of-N dual-control release
        Escrow-->>App: CourtRelease + EscrowRelease (quorum OK)
        App->>WC: Decrypt sealed DEK to expiring working copy
        Note over WC: expires_at enforced; sealed blob unchanged

        Counsel->>App: Synchronized timeline playback (WF-R-06)
        App->>App: Composite + spider + GPS/OBD if present
        App->>App: Apply SyncClockOffset
        App-->>Counsel: Coverage / gaps panel (Unverified Lyft caveats)

        Counsel->>App: Export disclosure pack (WF-R-08)
        App-->>Counsel: Sealed + .ots + attestation + VerificationReport
        Note over Counsel: Opposing counsel can verify OTS independently (WF-R-07)

        Counsel->>App: Close ViewerSession
        App->>App: Append access/render log; purge expired WC
    end
```

## Notes

- Fail-closed verify-before-decrypt is mandatory (FR-RIDE-050, NFR-22).
- Primary custody receipt path is Bitcoin OpenTimestamps. Optional L2 dual-anchor is interim only when configured (`docs/architecture/blockchain-custody-receipts.md`).
- Multi-driver bundles keep **per-record** VerificationReport rows; custody is never merged into one composite receipt (FR-RIDE-038).
- Avalonia UI 12 desktop shell; backend calls are gRPC to .NET 10 container services. No Lyft private APIs.
