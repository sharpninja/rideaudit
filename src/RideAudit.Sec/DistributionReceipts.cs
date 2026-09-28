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
}
