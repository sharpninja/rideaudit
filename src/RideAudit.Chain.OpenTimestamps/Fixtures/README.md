# Documented OpenTimestamps fixture

This fixture is the primary `btc-ots` path used when `RIDEAUDIT_OTS_CALENDAR=documented-fixture`.

It is not a calendar response from opentimestamps.org and it is not a Bitcoin transaction.

- `chainId` is `fixture-btc-ots`
- `transactionReference` is prefixed `fixture:` and is not a 64-character transaction id
- `blockHeight` is `9000000001`, chosen so it cannot be read as a Bitcoin block height
- `liveBitcoinMetadata` is false

A pending fixture proof does not satisfy admission. An unconfigured calendar fails closed and records no transaction metadata.
