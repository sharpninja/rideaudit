# Package layout

The live client is C# / Avalonia UI 12:

```
src/RideAudit.Client.Android/     # Android host
src/RideAudit.Client.Desktop/     # Desktop court viewer host
src/RideAudit.Shared.Ui/          # Shared Avalonia views
src/RideAudit.Bt/                 # Bluetooth roles
src/RideAudit.Video/              # Composite and quotas
src/RideAudit.Client.Seal/        # Device seal-at-collect (not server RideAudit.Seal)
src/RideAudit.PlayIntegrity/      # Fail-closed attestation gate
src/RideAudit.Viewer/             # ViewerSession and VerificationReport
src/RideAudit.Client.Contracts/   # ISealedAdmissionClient over src/RideAudit.Protos
```

Historical Kotlin package (archived, not the target):

Root application package: `org.rideaudit.app`

```
org.rideaudit.app
├── MainActivity.kt                 # Entry UI / session bootstrap
├── collect/
│   └── DualPhoneSession.kt         # Bluetooth discovery, driver-rider roles, session lifecycle
├── seal/
│   └── SealAtCollect.kt            # Encrypt + CustodyReceipt at collection
├── attest/
│   └── PlayIntegrityGate.kt        # Attestation before keygen (fail-closed)
├── composite/                      # Passenger: stream join + spider-graph overlay
├── upload/
│   └── SealedSubmissionUploader.kt # Driver-coordinated resumable sealed upload
└── identity/                       # Driver token + vehicle binding
```

Gradle module: `:app` under `artifacts/android/legacy-kotlin/app/`.

### Role-oriented stubs (current skeleton)

- `DualPhoneSession` models Bluetooth discovery, `PhoneRole` (Driver / Passenger), clock mastership on the driver phone, and passenger join for video sync / compositing / telematics overlay.
- `SealedSubmissionUploader` is invoked from the **driver** phone after seal orchestration.
- Production code should split Bluetooth transport and composite pipeline into `collect/` and `composite/` packages; stubs remain flat under `org.rideaudit.app` for this skeleton.
