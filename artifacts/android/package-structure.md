# Kotlin package layout

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

Gradle module: `:app` under `artifacts/android/app/`.

### Role-oriented stubs (current skeleton)

- `DualPhoneSession` models Bluetooth discovery, `PhoneRole` (Driver / Passenger), clock mastership on the driver phone, and passenger join for video sync / compositing / telematics overlay.
- `SealedSubmissionUploader` is invoked from the **driver** phone after seal orchestration.
- Production code should split Bluetooth transport and composite pipeline into `collect/` and `composite/` packages; stubs remain flat under `org.rideaudit.app` for this skeleton.
