package org.rideaudit.app

/**
 * Uploads sealed ciphertext + CustodyReceipt metadata only.
 * License: GPL-2.0
 *
 * Invoked from the driver phone after seal admission orchestration.
 * Never accepts plaintext video or sensor payloads (FR-RIDE-035).
 */
class SealedSubmissionUploader(
    private val baseUrl: String,
    private val bearerToken: String,
) {
    fun submitSealed(
        ciphertext: ByteArray,
        receipt: CustodyReceiptStub,
    ): String {
        require(ciphertext.isNotEmpty()) { "empty ciphertext" }
        require(receipt.contentHash.isNotBlank()) { "receipt required" }
        // Skeleton: POST /v1/submissions then optional /chunks for resumable upload.
        return "submission-id-placeholder"
    }

    fun pollAdmissionStatus(submissionId: String): String {
        // Skeleton: GET /v1/submissions/{id}/admission-status
        return "pending"
    }
}
