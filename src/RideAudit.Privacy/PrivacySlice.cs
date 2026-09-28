namespace RideAudit.Privacy;

public static class PrivacySlice
{
    public const string Phase = "S7";
    public const string State = "implemented";

    public static readonly string[] Checklist =
    [
        "FR-RIDE-010 DSAR access and deletion",
        "FR-RIDE-014 role-based access",
        "FR-RIDE-202 geolocation masking",
        "FR-RIDE-203 append-only access logs",
        "FR-RIDE-205 multi-year history index",
        "FR-RIDE-207 portable audit ZIP",
        "FR-RIDE-208 per-state retention",
        "FR-RIDE-210 legal hold suspends deletion"
    ];
}
