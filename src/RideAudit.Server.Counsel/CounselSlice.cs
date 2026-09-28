namespace RideAudit.Server.Counsel;

/// <summary>
/// S5 scaffold. Counsel bundle assembly and analysis UI reports are deferred.
/// </summary>
public static class CounselSlice
{
    public const string Phase = "S5";
    public const string State = "deferred";

    public static readonly string[] Checklist =
    [
        "FR-RIDE-007 coverage matrix",
        "FR-RIDE-008 online-hours policy audit",
        "FR-RIDE-009 time-window incident report",
        "FR-RIDE-020 court-review decryption path beyond the S4 document",
        "FR-RIDE-021 verification UI",
        "FR-RIDE-037 multi-driver per-record provenance bundle",
        "FR-RIDE-038 counsel multi-driver bundle",
        "TEST-RIDE-007 TEST-RIDE-008 TEST-RIDE-009 TEST-RIDE-016 UI partition TEST-RIDE-023"
    ];
}
