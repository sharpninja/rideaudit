package org.rideaudit.app

/**
 * Dual-phone capture session with Bluetooth discovery and driver-rider roles.
 * License: GPL-2.0
 *
 * Driver phone: session clock master; admit/start/stop; seal admission; submission coordination.
 * Passenger phone: video sync, stream compositing, realtime telematics (spider-graph overlay).
 * No Lyft private APIs.
 */

enum class PhoneRole {
    DRIVER,
    PASSENGER,
}

data class PeerPhone(
    val bluetoothAddress: String,
    val displayName: String,
    val role: PhoneRole,
)

class DualPhoneSession(
    val sessionId: String,
    val localRole: PhoneRole,
) {
    var running: Boolean = false
        private set
    var peer: PeerPhone? = null
        private set

    /** Bluetooth discovery and role agreement (skeleton). */
    fun discoverAndPair(preferredLocalRole: PhoneRole): PeerPhone {
        // Skeleton: scan, advertise RideAudit session service, agree roles.
        val discovered = PeerPhone(
            bluetoothAddress = "00:00:00:00:00:00",
            displayName = "peer-skeleton",
            role = if (preferredLocalRole == PhoneRole.DRIVER) {
                PhoneRole.PASSENGER
            } else {
                PhoneRole.DRIVER
            },
        )
        peer = discovered
        return discovered
    }

    /**
     * Driver phone admits passenger and starts capture as clock master.
     * Passenger phone joins for video sync / composite / telematics overlay.
     */
    fun start(attestation: AppAttestationStub) {
        require(attestation.verdictOk) { "Play Integrity required before capture" }
        require(peer != null) { "Bluetooth peer required before start" }
        if (localRole == PhoneRole.DRIVER) {
            // Clock master: distribute session timeline; admit passenger stream.
        } else {
            // Passenger: sync to driver clock; composite; spider-graph overlay.
        }
        running = true
    }

    fun stop() {
        // Driver coordinates stop; passenger ends sync/composite.
        running = false
    }

    fun isClockMaster(): Boolean = localRole == PhoneRole.DRIVER
}
