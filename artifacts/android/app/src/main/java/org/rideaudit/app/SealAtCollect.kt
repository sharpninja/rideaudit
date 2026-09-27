package org.rideaudit.app

/**
 * Seal-at-collect: encrypt payloads and build CustodyReceipt at collection time.
 * License: GPL-2.0
 *
 * Plaintext video/sensors must never be queued for upload.
 */
data class CustodyReceiptStub(
    val sessionId: String,
    val contentHash: String,
    val attestationTokenHash: String,
    val sealedAtEpochMs: Long,
)

class SealAtCollect {
    fun seal(
        plaintext: ByteArray,
        sessionId: String,
        attestationTokenHash: String,
    ): Pair<ByteArray, CustodyReceiptStub> {
        require(plaintext.isNotEmpty()) { "refusing empty payload" }
        // Skeleton: real impl uses per-session/sample keys after Play Integrity.
        val ciphertext = plaintext.copyOf() // placeholder; replace with AEAD
        val receipt = CustodyReceiptStub(
            sessionId = sessionId,
            contentHash = "sha256:placeholder",
            attestationTokenHash = attestationTokenHash,
            sealedAtEpochMs = System.currentTimeMillis(),
        )
        // Zeroize policy for plaintext belongs in production crypto layer.
        return ciphertext to receipt
    }
}
