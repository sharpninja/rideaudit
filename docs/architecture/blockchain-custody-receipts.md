# Public blockchain for custody receipts

Status: Proposed default (configurable). Research date: 2026-09-27.

## Recommendation

**Primary receipt path:** Bitcoin via OpenTimestamps (OTS)
- Public immutable ledger with long evidentiary track record
- Merkle aggregation keeps per-receipt cost near zero
- Portable `.ots` proof verifiable against Bitcoin headers without trusting RideAudit servers
- Aligns with FR-RIDE chain configurability and fail-closed admission

**Secondary / fast-confirm path (optional dual-anchor):** Ethereum L2 such as Base or Polygon PoS
- Faster user-visible confirmation for operator UI
- Still public and independently checkable on a block explorer
- Use for interim confirmation only when dual-anchor is enabled; Bitcoin OTS remains the long-term custody anchor unless jurisdiction config says otherwise

## Why not a single private chain

Court and opposing counsel need independent verification. A private or permissioned ledger fails the public immutable requirement in FR-18 / NFR-12.

## Configuration knobs

- `chain_id` / profile: `btc-ots` | `eth-l2-base` | `eth-l2-polygon` | `dual-btc-ots+l2`
- Confirmation policy: OTS upgrade complete vs L2 N-block confirmations
- Fee sponsor: calendar/server aggregation vs per-driver gas wallet
- Offline policy: local pending receipt, not admitted until configured confirmation succeeds

## Open questions (from requirements)

- Production fee sponsorship and congestion policy
- Jurisdiction-specific dual-anchor requirements
- Disclosure risk of public keys / hashes on-chain

## References

- https://opentimestamps.org/
- IETF draft SCITT time-anchor (Bitcoin/OTS)
- US FRE 901/902 style electronic-record authentication practice (counsel review required; not legal advice)
