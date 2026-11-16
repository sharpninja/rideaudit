namespace RideAudit.Ingest;

/// <summary>
/// S6 ingest. Raw driver files are hashed, versioned, and stored only as ciphertext.
/// </summary>
public static class IngestSlice
{
    public const string Phase = "S6";
    public const string State = "implemented";
    public const string Constraint = "Accept a driver-provided privacy-export ZIP and third-party files the driver supplies. Do not call undocumented Lyft private APIs.";
}

public static class ProvenanceTags
{
    public const string PrivacyExport = "lyft_privacy_export";
    public const string Manual = "in_app_manual";
    public const string ThirdParty = "third_party_telematics";
    public const string Unverified = "unverified";
}

public static class ApiGapNotice
{
    public const string Text =
        "No documented public driver telematics API supplies Smooth Cruiser scores, IMU streams, or high-rate GPS. " +
        "Missing signals stay missing. Coarse location is not a Smooth Cruiser score. " +
        "Research date 2026-09-27.";
}
