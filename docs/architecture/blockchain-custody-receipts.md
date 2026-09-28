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
- `RIDEAUDIT_OTS_CALENDAR`: unset (fail-closed) | `documented-fixture` | `https://…` public calendar
- `RIDEAUDIT_L2_CALENDAR` / `RIDEAUDIT_L2_RPC`: unset (fail-closed) | `documented-fixture` | `https://…` JSON-RPC
- Confirmation policy: OTS upgrade complete vs L2 N-block confirmations
- Fee sponsor: calendar/server aggregation vs per-driver gas wallet
- Offline policy: local pending receipt, not admitted until configured confirmation succeeds

A live `https://` OpenTimestamps calendar POSTs the 32-byte receipt digest and stores the calendar body as pending proof. Pending proofs do not admit the record and do not write `transaction_reference` or `block_height`. This tree does not parse Bitcoin header linkage, so `live_bitcoin_metadata` stays false.

A live L2 RPC can probe `eth_blockNumber`. Commit without a signer fail-closes and refuses to invent a hash. Production still refuses `documented-fixture`.

## Implemented fixture profiles

Live Bitcoin, Base, and Polygon calendars are not configured in this tree. Admission fails closed for an unconfigured profile and writes no transaction id.

Development can opt in to documented fixtures:

- `btc-ots` with `RIDEAUDIT_OTS_CALENDAR=documented-fixture` records chain id `fixture-btc-ots` and a `fixture:` reference. `live_bitcoin_metadata` stays false.
- `eth-l2-base` and `eth-l2-polygon` with `RIDEAUDIT_L2_CALENDAR=documented-fixture` record `fixture-eth-l2-base` or `fixture-eth-l2-polygon` and a `fixture:not-an-ethereum-txid:` reference. See `src/RideAudit.Chain.EthL2/Fixtures/eth-l2-fixture.json`.
- `dual-btc-ots+l2` admits only when both fixture legs upgrade. The receipt transaction reference is `fixture:not-a-live-txid:rideaudit-dual-btc-ots-l2-fixture-001`. A failed leg copies no transaction metadata from the other leg.

Production refuses both fixture flags. These values are not live chain transactions.

## Open questions (from requirements)

- Production fee sponsorship and congestion policy
- Jurisdiction-specific dual-anchor requirements
- Disclosure risk of public keys / hashes on-chain

## References

- https://opentimestamps.org/
- IETF draft SCITT time-anchor (Bitcoin/OTS)
- US FRE 901/902 style electronic-record authentication practice (counsel review required; not legal advice)
