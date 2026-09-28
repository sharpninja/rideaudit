namespace RideAudit.Server.Counsel;

public static class CounselSlice
{
    public const string Phase = "S5";
    public const string State = "implemented";

    public static readonly string[] Checklist =
    [
        "FR-RIDE-007 coverage matrix",
        "FR-RIDE-008 online-hours policy audit",
        "FR-RIDE-009 time-window incident report",
        "FR-RIDE-020 court-review statements on the verification report",
        "FR-RIDE-021 hash verification without decryption",
        "FR-RIDE-037 per-record provenance",
        "FR-RIDE-038 counsel multi-driver bundle"
    ];
}
