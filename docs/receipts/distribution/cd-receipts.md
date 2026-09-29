# Server distribution receipts

FR-RIDE-217. TEST-RIDE-020 server portion. FR-RIDE-063 Octopus CD. GPL-2.0-only.

Operator direction: Use Octopus Deploy. Build containers and deploy to PAYTON-DESKTOP. If you are out of licenses on the default container, create a new Octopus container on PAYTON-DESKTOP. Do not use GHCR.

The Octopus PAYTON-DESKTOP row is a live release (`20260929T015822Z-octopus-payton-desktop.md`). Historical Dev/Staging/Prod rows remain placeholders. None of these rows is a GHCR green. Play publication is `not-claimed`. A row is not a store publication.

`RideAudit.Sec.DistributionReceipts.ServerPortions` still returns the historical Dev/Staging/Prod `not-run` rows. Those names are not the product CD path. They were never GHCR greens. Keep them until the type is updated; a code mismatch is not a live Octopus green.

| Environment | Status | Play publication | SPDX |
| --- | --- | --- | --- |
| Dev (historical placeholder; not GHCR) | not-run | not-claimed | GPL-2.0-only |
| Staging (historical placeholder; not GHCR) | not-run | not-claimed | GPL-2.0-only |
| Prod (historical placeholder; not GHCR) | not-run | not-claimed | GPL-2.0-only |
| Octopus to PAYTON-DESKTOP (FR-RIDE-063) | live-octopus-desktop; see `20260929T015822Z-octopus-payton-desktop.md` (new container `octopus-rideaudit`, `Releases-2` / `Deployments-2`, probe `:28080` HTTP 200). Not GHCR. | not-claimed | GPL-2.0-only |
| PAYTON-LEGION2 Docker Desktop | engine-http-500 | not-claimed | GPL-2.0-only |
| PAYTON-OMARCHY lab images (prior interim, not Octopus CD) | loopback compose `127.0.0.1:18080`; see `legion2-omarchy-20260928.md` | not-claimed | GPL-2.0-only |
| ngrok canonical tunnel (FR-RIDE-064) | live to `192.168.0.149:28080`; see `20260929T030643Z-ngrok-desktop-28080.md`. Prior interim was `:18080`. Not GHCR. | not-claimed | GPL-2.0-only |

Android client publication, store listing, and bracket hardware distribution are outside this server receipt.
