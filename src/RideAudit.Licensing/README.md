# RideAudit.Licensing

GPL-2.0-or-later notices and the client distribution scaffold for FR-RIDE-031.

`Distribution/client-distribution-manifest.json` records:

- Public source repository URL: https://github.com/sharpninja/rideaudit
- Google Play `published: false`
- `receiptPath: null`

That file is not a Play Store receipt. `PublicationClaimGuard` rejects a claim of publication when no receipt file exists, and it rejects a road-ready claim.

Signed Windows, Linux, and macOS release builds were not produced in the cloud run that added this package.
