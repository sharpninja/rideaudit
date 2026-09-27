# SB-03 Passenger composite

**Artifact:** ART-RIDE-UX-001  
**FR links:** FR-RIDE-055, FR-RIDE-041, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045  
**Screens:** [WF-05](../assets/wireframes/WF-05-passenger-capture-spider.svg), [WF-08](../assets/wireframes/WF-08-fail-closed-errors.svg)

## Goal

Passenger phone syncs video to the driver-published session clock, joins dual streams into a composite, and overlays realtime telematics (including accelerometer spider graph) before seal-at-collect on the composite path.

## Actors

- Passenger (compositor phone)
- Driver phone (clock master, stream peer)
- On-device sensors (IMU / GPS as available)

## Beats

1. **Await start**  
   After pairing, passenger UI shows "Waiting for driver start", live SyncClockOffset, and camera preview. Start control is absent (driver-only).

   ![WF-05 Passenger capture and spider](../assets/wireframes/WF-05-passenger-capture-spider.svg)

   [Open WF-05-passenger-capture-spider.svg](../assets/wireframes/WF-05-passenger-capture-spider.svg)

2. **Sync and join**  
   On START, passenger locks to session clock, records SyncClockOffset, opens local camera stream, and joins/receives driver stream for composite layout (picture-in-picture or split).

   ![WF-05 Passenger capture and spider](../assets/wireframes/WF-05-passenger-capture-spider.svg)

   [Open WF-05-passenger-capture-spider.svg](../assets/wireframes/WF-05-passenger-capture-spider.svg)

3. **Realtime telematics overlay**  
   Spider graph (accel axes) and optional speed/heading chips render on the composite in realtime, timestamped to the shared clock. Overlay is part of first-class evidence, not a post-hoc edit.

   ![WF-05 Passenger capture and spider](../assets/wireframes/WF-05-passenger-capture-spider.svg)

   [Open WF-05-passenger-capture-spider.svg](../assets/wireframes/WF-05-passenger-capture-spider.svg)

4. **Active capture**  
   Capture screen (WF-05) shows composite preview, spider graph, recording indicator, clock offset, and link health. Passenger cannot stop the admitted session; Stop is driver-owned. Local fault (camera loss, desync beyond threshold) surfaces fail-closed and notifies coordinator.

   ![WF-05 Passenger capture and spider](../assets/wireframes/WF-05-passenger-capture-spider.svg)

   [Open WF-05-passenger-capture-spider.svg](../assets/wireframes/WF-05-passenger-capture-spider.svg)

   Fail-closed branch:

   ![WF-08 Fail-closed errors](../assets/wireframes/WF-08-fail-closed-errors.svg)

   [Open WF-08-fail-closed-errors.svg](../assets/wireframes/WF-08-fail-closed-errors.svg)

5. **STOP received**  
   Passenger finalizes composite buffer and enters seal progress for the composite package (SB-04).

   Seal progress:

   ![WF-06 Seal progress](../assets/wireframes/WF-06-seal-progress.svg)

   [Open WF-06-seal-progress.svg](../assets/wireframes/WF-06-seal-progress.svg)

## Success criteria

- SyncClockOffset recorded against driver clock.
- Streams joined and telematics overlaid before seal-at-collect.
- Composite sealed as first-class evidence (FR-RIDE-045).

## Notes

Spider graph is illustrative of IMU magnitude/direction over a short window; exact axes labeling is implementation-defined.
