namespace RideAudit.Ingest;

/// <summary>
/// S6 scaffold. No undocumented Lyft private API is referenced or called.
/// </summary>
public static class IngestSlice
{
    public const string Phase = "S6";
    public const string State = "deferred";
    public const string Constraint = "Accept only a driver-provided privacy-export ZIP in a future slice. Do not call undocumented Lyft private APIs.";

    public static readonly string[] Checklist =
    [
        "FR-RIDE-001 privacy-export ZIP",
        "FR-RIDE-002 Smooth Cruiser scores",
        "FR-RIDE-003 trip-level records",
        "FR-RIDE-004 optional Concierge or Business API only when partnership is enabled",
        "FR-RIDE-005 third-party telematics",
        "FR-RIDE-006 provenance and consent ledger",
        "FR-RIDE-011 no undocumented Lyft private APIs",
        "FR-RIDE-012 admin partnership gates",
        "FR-RIDE-013 hash and version raw imports",
        "FR-RIDE-204 Concierge resilience",
        "FR-RIDE-206 no false Smooth Cruiser labeling",
        "FR-RIDE-209 in-product API gap notice"
    ];
}
