# STOP: BDPv4 AGREE write blocked on createFr/createTr schema gap (priority)

Written: 2026-10-07 10:12 CT (America/Chicago) / 20261007T151201Z UTC.
Machine: PAYTON-LEGION2
Workspace: F:\GitHub\RideAudit
Author: Grok Bot executor
Plugin: F:\github\mcpserver-grok-plugin (MCP_AGENT_NAME=GrokCode)

## Payton AGREE

Payton AGREED BDPv4 candidates (A+B). Source draft:
`docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md`

Accuracy addendum (priority above all): write descriptions verbatim from draft; do not invent AC / mask / RBAC / legal-hold; if create API forces fields not in draft, STOP and report schema gap.

## Outcome

**NO MCP rows created.** All four proposed ids remain `not_found`:

| Id | get result |
| --- | --- |
| FR-RIDE-077 | not_found (req-20261007T151156Z-0843) |
| FR-RIDE-078 | not_found (req-20261007T151157Z-4a26) |
| TR-RIDE-VIDEO-007 | not_found (req-20261007T151158Z-1830) |
| TR-RIDE-PRIV-004 | not_found (req-20261007T151159Z-a95c) |

No disk YAML projection sync. No feature code. No AC invented. No commit.

## Schema gap (exact)

`workflow.requirements.createFr` required fields per skill + live probe:

- `id` — present in draft (proposed)
- `title` — draft has Role column text (not labeled "title"); usable as title from draft Role with Payton confirmation preferred
- `description` — present (exact Candidate A/B sentences)
- `priority` — **ABSENT from draft** — **blocks write**
- `area` — derivable from id segment `RIDE`
- TR also requires `subarea` — derivable from id (`VIDEO` / `PRIV`)

Live probe (createFr FR-RIDE-077 without `priority`, description verbatim Candidate A, title = draft Role):

```
code: schema_validation_failed
message: YAML request failed schema validation.
errors:
- payload.params.priority is required.
requestId: req-20261007T151140Z-2739
```

Valid `priority` values observed on existing RIDE FRs: `critical`, `high`, `medium` (and skill lists status enum separately: pending/in_progress/completed/deferred). There is no MCP status value named AGREE/accepted; prior AGREE convention keeps `status: pending` and puts `approval: Payton AGREE <date>` in `notes`.

## Draft sentences ready to write (verbatim) once priority supplied

**Candidate A / FR-RIDE-077 description:**
Precise location and other capture data stay unmasked; accelerometer and precise location are embedded in the H.264 stream as real-time per-picture SEI for certifiable legal data.

**Candidate A / TR-RIDE-VIDEO-007 description (from Role; draft Role is the only TR body text):**
SEI NAL units carry real-time per-picture accelerometer and precise location matched to each picture

NOTE on TR-RIDE-VIDEO-007 body: the draft's Exact candidate sentence is shared under Candidate A for the FR. The TR "Role" column is the only TR-specific sentence in the draft. Per accuracy, TR description should use that Role text (or Payton must supply a separate TR sentence). Do not invent beyond Role.

**Candidate B / FR-RIDE-078 description:**
Retention does not treat the driver as a third party; do not apply California 30-day or 180-day location third-party deletion frames to driver-collected evidentiary data in their own vehicle.

**Candidate B / TR-RIDE-PRIV-004 description (from Role):**
retention timers must not apply CA third-party 30-day / location 180-day deletion frames to driver-owned vehicle evidentiary capture

## Titles proposed from draft Role (not invented outside draft)

| Id | Title from draft Role |
| --- | --- |
| FR-RIDE-077 | Functional rule: unmasked capture; accel + precise location in H.264 SEI per picture |
| TR-RIDE-VIDEO-007 | Technical: SEI NAL units carry real-time per-picture accelerometer and precise location matched to each picture |
| FR-RIDE-078 | Functional rule: driver-collected evidentiary retention is not third-party retention |
| TR-RIDE-PRIV-004 | Technical: retention timers must not apply CA third-party 30-day / location 180-day deletion frames to driver-owned vehicle evidentiary capture |

## Unblock

Payton (or parent with Payton authority) must supply `priority` for each of the four ids (or one shared priority). Optional: confirm Role→title and TR description = Role text. Then re-dispatch write.

## Disk paths not synced

- docs/Project/Functional-Requirements-Batch.yaml — untouched
- docs/Project/Technical-Requirements-Batch.yaml — untouched
- docs/Project/Requirements-Mappings-Batch.yaml — untouched
