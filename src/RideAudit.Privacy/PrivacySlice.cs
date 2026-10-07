namespace RideAudit.Privacy;

public static class PrivacySlice
{
    public const string Phase = "S7";
    public const string State = "implemented";

    // FR-014/202/203/208 kill-list invent removed from checklist. Access-log helper remains for driver self-access (not RBAC invent).
    public static readonly string[] Checklist =
    [
        "FR-RIDE-010 DSAR access and deletion",
        "FR-RIDE-077 unmasked precise location (replaces killed FR-014/202 mask/RBAC invent)",
        "FR-RIDE-078 driver not third-party retention (replaces killed FR-208)",
        "FR-RIDE-205 multi-year history index",
        "FR-RIDE-207 portable audit ZIP"
    ];
}