# Legal-hold kill complete (surgical)

Written: 2026-10-07 10:09 CT (America/Chicago).
Accuracy rule: removed only clear legal-hold text; no invented replacement AC/UC stories.

## MCP SoT
### FR-RIDE-210
- BEFORE: present (Legal hold suspends deletion)
- AFTER: not_found (deleted)
### FR-RIDE-010
- BEFORE AC: AC-001 access export; AC-002 "Deletion honors legal holds and documents custody impact."
- AFTER AC: AC-001 only (AC-002 removed entire; no replacement AC invented)
### TR-RIDE-PRIV-003
- BEFORE description: ...subject to legal holds and custody documentation.
- AFTER description: ...subject to custody documentation. (removed "legal holds and ")
- BEFORE AC-002: Deletion respects legal holds. REMOVED. Kept AC-001 Access export available.
### TR-RIDE-STORE-003
- BEFORE description: ...with legal-hold suspension of deletion.
- AFTER description: Configurable retention by jurisdiction.
- BEFORE AC-002: Legal hold suspends deletion. REMOVED. Kept AC-001 Per-jurisdiction retention timers.
### TEST-RIDE-010
- BEFORE title: DSAR access deletion and legal hold
- AFTER title: DSAR access deletion
- BEFORE description: Access export works; deletion blocked under legal hold and allowed when clear.
- AFTER description: Access export works. (deletion clause was entirely legal-hold framed; removed; no invented deletion replacement)

## UC-RIDE-008 (earlier this session)
- Realizes FR-RIDE-010 only; Driver-only actors; legal-hold Goal/diagram/include removed.

## wiki.yaml child paths
- Rule: child path = existing title verbatim.
- BEFORE (after prior path-required state): children had no path.
- AFTER: Mobile Dual-Phone path=Mobile Dual-Phone; Review App path=Review App; parent path=Storyboards.
- generateDocument: success=true

## Disk patch log
- PRIV-003 description: removed "legal holds and "
- PRIV-003: removed AC-RIDE-PRIV-003-002
- STORE-003 description: removed legal-hold suspension clause
- STORE-003 notes: stripped dead FR refs
- STORE-003: removed AC-RIDE-STORE-003-002
- TEST-010 title: removed " and legal hold"
- TEST-010 description: removed legal-hold deletion clause; left "Access export works."
- wiki legal-hold/legal-hold hits: 4
-   F:\GitHub\rideaudit\docs\Project\wiki\azure\Functional-Requirements.md:245: Attach a GPL2 license/version and source-commit notice to shared software, schema, receipt, attestat
-   F:\GitHub\rideaudit\docs\Project\wiki\azure\Functional-Requirements.md:762: Video storage and bandwidth: define per-driver/session quotas and maximum composite/raw-stream sizes
-   F:\GitHub\rideaudit\docs\Project\wiki\github\Functional-Requirements.md:245: Attach a GPL2 license/version and source-commit notice to shared software, schema, receipt, attestat
-   F:\GitHub\rideaudit\docs\Project\wiki\github\Functional-Requirements.md:762: Video storage and bandwidth: define per-driver/session quotas and maximum composite/raw-stream sizes
- FR-RIDE-210 yaml hits: 0

## STOP / observe for Payton
- TEST-RIDE-010 now describes access export only; original deletion sentence was inseparable from legal-hold invent. No replacement deletion test text written.


## Residual FR phrase cleanup (same session)

### FR-RIDE-030
- BEFORE: ...access policy, and legal holds rather than...
- AFTER: ...access policy rather than... (removed ", and legal holds")
- ACs unchanged.

### FR-RIDE-219
- BEFORE: ...lifecycle deletion/legal holds, and separate...
- AFTER: ...lifecycle deletion, and separate... (removed "/legal holds")
- ACs unchanged.

generateDocument re-run: success. Wiki legal-hold hit count after: see above.


## FR-RIDE-030 description restore
ParamsYaml folded >- briefly corrupted description to literal '>-'. Restored with quoted description (legal-hold phrase still removed). DESC_OK=True

