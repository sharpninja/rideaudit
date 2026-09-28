namespace RideAudit.Contracts;

/// <summary>
/// FR-RIDE-060 / FR-RIDE-062. Conformance binds to gRPC protos when the OpenAPI companion disagrees.
/// </summary>
public static class ContractAuthority
{
    public const string Authoritative = "grpc-protobuf";
    public const string OpenApiRole = "non-authoritative-companion";
    public const string AuthoritativePath = "src/RideAudit.Protos";
    public const string CompanionPath = "artifacts/server-api/openapi.yaml";
    public const string ContractVersion = "0.2.0";

    public static string BindConformanceSource() => Authoritative;
}

public static class LicenseMetadata
{
    public const string Spdx = "GPL-2.0-only";
    public const string Notice =
        "Copyright (C) 2026 RideAudit contributors. Licensed under GPL-2.0-only. " +
        "Source commit is the git revision that contains this file.";
}

public static class RideAuditPolicy
{
    public const string Version = "policy-2026-09-28";
    public const string SchemaVersion = "rideaudit-receipt-core-1";
    public const string AlgorithmId = "aes-256-gcm-sha256-v1";
    public const string SealedContentType = "application/vnd.rideaudit.sealed+octet-stream";
    public const string ProvenanceTag = "rideaudit.collection.sealed";
    public const string PlayProvider = "play_integrity";
    public const string MeetsDeviceIntegrity = "MEETS_DEVICE_INTEGRITY";
}

public static class ChainProfileIds
{
    public const string BtcOts = "btc-ots";
    public const string EthL2Base = "eth-l2-base";
    public const string EthL2Polygon = "eth-l2-polygon";
    public const string DualBtcOtsL2 = "dual-btc-ots+l2";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        BtcOts, EthL2Base, EthL2Polygon, DualBtcOtsL2
    };

    public static bool IsPrimaryOts(string profile) =>
        string.Equals(profile, BtcOts, StringComparison.Ordinal);
}

public static class FixtureChainIds
{
    public const string BtcOts = "fixture-btc-ots";
    public const string EthL2Base = "fixture-eth-l2-base";
    public const string EthL2Polygon = "fixture-eth-l2-polygon";
    public const string Dual = "fixture-dual-btc-ots+l2";
}

public static class ProofSources
{
    public const string DocumentedFixture = "documented-fixture";
    public const string DocumentedFixtureL2 = "documented-fixture-l2";
    public const string DocumentedFixtureDual = "documented-fixture-dual";
    public const string OpenTimestampsCalendar = "opentimestamps-calendar";
    public const string EthL2Rpc = "eth-l2-rpc";
    public const string Unconfigured = "unconfigured";
    public const string DualIncomplete = "dual-incomplete";
}

public enum CustodyState
{
    LocalSealedPending = 1,
    Confirmed = 2,
    Rejected = 3,
    Quarantined = 4,
    Admitted = 5
}

public static class CustodyStateNames
{
    public static string ToWire(CustodyState state) => state switch
    {
        CustodyState.LocalSealedPending => "local-sealed-pending",
        CustodyState.Confirmed => "confirmed",
        CustodyState.Rejected => "rejected",
        CustodyState.Quarantined => "quarantined",
        CustodyState.Admitted => "admitted",
        _ => "unspecified"
    };
}
