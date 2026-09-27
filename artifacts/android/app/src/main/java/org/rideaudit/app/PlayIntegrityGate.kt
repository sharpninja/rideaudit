package org.rideaudit.app

/**
 * Play Integrity gate: attestation must succeed before keygen.
 * License: GPL-2.0
 *
 * Fail-closed on missing, failed, or unallowlisted verdicts (FR-RIDE-025/026/215).
 */
data class AppAttestationStub(
    val token: String,
    val nonce: String,
    val obtainedAtEpochMs: Long,
    val verdictOk: Boolean,
)

class PlayIntegrityGate {
    fun newNonce(): String = "nonce-skeleton"

    fun requestAttestation(nonce: String): AppAttestationStub {
        // Skeleton: integrate Play Integrity API here.
        return AppAttestationStub(
            token = "",
            nonce = nonce,
            obtainedAtEpochMs = System.currentTimeMillis(),
            verdictOk = false,
        )
    }

    fun isAllowlisted(attestation: AppAttestationStub): Boolean {
        if (!attestation.verdictOk) return false
        if (attestation.token.isBlank()) return false
        return true
    }
}
