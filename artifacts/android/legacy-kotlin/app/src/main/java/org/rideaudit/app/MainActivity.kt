package org.rideaudit.app

/**
 * RideAudit entry activity (skeleton).
 * License: GPL-2.0
 *
 * Bootstraps Bluetooth driver-rider pairing, Play Integrity gate, and session setup.
 * Driver phone coordinates; passenger phone composites. Not a production UI.
 */
class MainActivity {
    fun onSessionStartRequested(
        driverToken: String,
        vehicleId: String,
        localRole: PhoneRole,
    ) {
        val gate = PlayIntegrityGate()
        val attestation = gate.requestAttestation(nonce = gate.newNonce())
        if (!gate.isAllowlisted(attestation)) {
            // Fail-closed: do not start capture or keygen.
            return
        }
        val session = DualPhoneSession(sessionId = "pending", localRole = localRole)
        session.discoverAndPair(preferredLocalRole = localRole)
        session.start(attestation = attestation)
        // Driver phone orchestrates seal admission and upload after segments complete.
    }
}
