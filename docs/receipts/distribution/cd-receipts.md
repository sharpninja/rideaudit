# Server distribution receipts

FR-RIDE-217. TEST-RIDE-020 server portion. FR-RIDE-063 Octopus CD. GPL-2.0-only.

Operator direction: Use Octopus Deploy. Build containers and deploy to PAYTON-DESKTOP. If you are out of licenses on the default container, create a new Octopus container on PAYTON-DESKTOP. Do not use GHCR.

These rows are honest placeholders. No Octopus RideAudit release executed in the environment that added them. They are not an Octopus green and not a GHCR green. Play publication is `not-claimed`. A row is not a store publication.

`RideAudit.Sec.DistributionReceipts.ServerPortions` still returns the historical Dev/Staging/Prod `not-run` rows. Those names are not the product CD path. They were never GHCR greens. Keep them until the type is updated; a code mismatch is not a live Octopus green.

| Environment | Status | Play publication | SPDX |
| --- | --- | --- | --- |
| Dev (historical placeholder; not GHCR) | not-run | not-claimed | GPL-2.0-only |
| Staging (historical placeholder; not GHCR) | not-run | not-claimed | GPL-2.0-only |
| Prod (historical placeholder; not GHCR) | not-run | not-claimed | GPL-2.0-only |
| Octopus to PAYTON-DESKTOP (FR-RIDE-063) | not-run | not-claimed | GPL-2.0-only |
| PAYTON-LEGION2 Docker Desktop | engine-http-500 | not-claimed | GPL-2.0-only |
| PAYTON-OMARCHY lab images (interim, not Octopus CD) | built-not-started or loopback compose; see `legion2-omarchy-20260928.md` and ngrok receipts | not-claimed | GPL-2.0-only |

Android client publication, store listing, and bracket hardware distribution are outside this server receipt.
