namespace RideAudit.Sec;

/// <summary>
/// FR-RIDE-217 server portion of TEST-RIDE-020. These rows record that Dev, Staging,
/// and Prod continuous delivery did not run in this environment. They are not deployment receipts
/// and they do not claim a store publication.
/// </summary>
public sealed record CdEnvironmentReceipt(string Environment, string Status, string Detail, string PlayPublication, string Spdx);

public static class DistributionReceipts
{
    public const string NotRun = "not-run";
    public const string PlayNotClaimed = "not-claimed";

    public static IReadOnlyList<CdEnvironmentReceipt> ServerPortions { get; } =
    [
        new("Dev", NotRun, "No continuous-delivery runner executed in this environment. This record is not a deployment.", PlayNotClaimed, "GPL-2.0-only"),
        new("Staging", NotRun, "No continuous-delivery runner executed in this environment. This record is not a deployment.", PlayNotClaimed, "GPL-2.0-only"),
        new("Prod", NotRun, "No continuous-delivery runner executed in this environment. This record is not a deployment.", PlayNotClaimed, "GPL-2.0-only")
    ];

    /// <summary>
    /// Pointer to the Octopus LAB-OMARCHY receipt on file. Status is receipt-on-file.
    /// This constant is not a live probe and is not a GHCR row. Dev/Staging/Prod stay in <see cref="ServerPortions"/>.
    /// </summary>
    public static CdEnvironmentReceipt OctopusDesktopOnFile { get; } =
        new(
            "Octopus-LAB-OMARCHY",
            "receipt-on-file",
            "docs/receipts/distribution/20260929T015822Z-octopus-LAB-OMARCHY.md names octopus-rideaudit and LAB-OMARCHY. This constant is not a live probe and is not GHCR.",
            PlayNotClaimed,
            "GPL-2.0-only");
}
