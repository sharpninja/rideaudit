using RideAudit.Contracts;

namespace RideAudit.Privacy;

public static class PrivacySlice
{
    public const string Phase = "S7";
    public const string State = "deferred";

    public static readonly string[] Checklist =
    [
        "FR-RIDE-010 DSAR access and deletion",
        "FR-RIDE-014 role-based access beyond the location-policy stub",
        "FR-RIDE-202 geolocation masking",
        "FR-RIDE-205 multi-year history scale",
        "FR-RIDE-207 portable audit ZIP",
        "FR-RIDE-208 per-state retention",
        "FR-RIDE-210 legal hold suspends deletion"
    ];
}

public static class DeferredDsar
{
    public static void Delete(string subjectId)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Subject id is required.");
        throw new RideAuditException(ErrorCodes.NotImplemented, "DSAR deletion is deferred. No data was deleted.");
    }
}
