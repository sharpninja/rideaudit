using RideAudit.Contracts;
using RideAudit.TestSupport;

namespace RideAudit.Protos.Tests;

/// <summary>
/// TEST-RIDE-037 companion-authority partition.
/// FR-RIDE-060, FR-RIDE-062, TR-RIDE-GPL-005, TR-RIDE-SERVER-010.
/// UC-RIDE-029, UC-RIDE-031.
/// AC-RIDE-060-001, AC-RIDE-060-002, AC-RIDE-062-001, AC-RIDE-062-002.
/// </summary>
public class TestRide037CompanionAuthority
{
    private static string Root => ServerWorld.RepoRoot();

    [Fact]
    [Trait("TEST", "TEST-RIDE-037")]
    [Trait("FR", "FR-RIDE-062")]
    [Trait("AC", "AC-RIDE-062-001")]
    public void OpenApi_and_artifact_metadata_mark_the_companion_non_authoritative()
    {
        var readme = File.ReadAllText(Path.Combine(Root, "artifacts/server-api/README.md"));
        var artifact = File.ReadAllText(Path.Combine(Root, "artifacts/server-api/ARTIFACT.yaml"));
        var openapi = File.ReadAllText(Path.Combine(Root, "artifacts/server-api/openapi.yaml"));

        Assert.Contains("non-authoritative", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("src/RideAudit.Protos", readme, StringComparison.Ordinal);
        Assert.Contains("companionAuthority: non-authoritative", artifact, StringComparison.Ordinal);
        Assert.Contains("authoritativeContract: src/RideAudit.Protos/", artifact, StringComparison.Ordinal);
        Assert.Contains("x-rideaudit-contract-authority: non-authoritative-companion", openapi, StringComparison.Ordinal);
        Assert.Contains("NON-AUTHORITATIVE COMPANION", openapi, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-037")]
    [Trait("FR", "FR-RIDE-060")]
    [Trait("AC", "AC-RIDE-060-001")]
    public void Protos_publish_gpl_notice_and_grpc_authority()
    {
        var protoRoot = Path.Combine(Root, "src/RideAudit.Protos/Protos");
        var protos = Directory.GetFiles(protoRoot, "*.proto", SearchOption.AllDirectories);
        Assert.NotEmpty(protos);
        foreach (var proto in protos)
        {
            var text = File.ReadAllText(proto);
            Assert.Contains("SPDX-License-Identifier: GPL-2.0-only", text, StringComparison.Ordinal);
            Assert.Contains("CONTRACT_AUTHORITY: grpc-protobuf", text, StringComparison.Ordinal);
        }

        var admission = File.ReadAllText(Path.Combine(protoRoot, "rideaudit/admission/v1/admission.proto"));
        Assert.Contains("service Admission", admission, StringComparison.Ordinal);
        Assert.DoesNotContain("lyft.com", string.Join('\n', protos.Select(File.ReadAllText)), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-037")]
    [Trait("FR", "FR-RIDE-062")]
    [Trait("TR", "TR-RIDE-SERVER-010")]
    [Trait("AC", "AC-RIDE-062-002")]
    public void Conformance_binds_to_grpc_when_the_companion_disagrees()
    {
        Assert.Equal("grpc-protobuf", ContractAuthority.BindConformanceSource());
        Assert.Equal("non-authoritative-companion", ContractAuthority.OpenApiRole);
        var openapi = File.ReadAllText(Path.Combine(Root, "artifacts/server-api/openapi.yaml"));
        Assert.Contains("/v1/submissions", openapi, StringComparison.Ordinal);
        var proto = File.ReadAllText(Path.Combine(Root, "src/RideAudit.Protos/Protos/rideaudit/admission/v1/admission.proto"));
        Assert.Contains("rpc SubmitSealed", proto, StringComparison.Ordinal);
        Assert.DoesNotContain("rpc SubmitSealed", openapi, StringComparison.Ordinal);
        Assert.NotEqual("openapi", ContractAuthority.BindConformanceSource());
    }
}

/// <summary>
/// TEST-RIDE-020 licensing and notice partition.
/// FR-RIDE-029, FR-RIDE-030, TR-RIDE-GPL-001, TR-RIDE-GPL-002.
/// Play/source publication (FR-RIDE-031 / TR-RIDE-GPL-003) is outside this server partition.
/// </summary>
public class TestRide020LicensingPartition
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-020")]
    [Trait("FR", "FR-RIDE-029")]
    [Trait("AC", "AC-RIDE-029-001")]
    public void License_is_gpl_2_and_not_replaced_by_mit_or_apache()
    {
        var root = ServerWorld.RepoRoot();
        var license = File.ReadAllText(Path.Combine(root, "LICENSE"));
        var notice = File.ReadAllText(Path.Combine(root, "NOTICE"));
        var props = File.ReadAllText(Path.Combine(root, "Directory.Build.props"));
        Assert.Contains("GNU GENERAL PUBLIC LICENSE", license, StringComparison.Ordinal);
        Assert.Contains("Version 2", license, StringComparison.Ordinal);
        Assert.Contains("GPL-2.0-only", notice, StringComparison.Ordinal);
        Assert.Contains("GPL-2.0-only", props, StringComparison.Ordinal);
        Assert.DoesNotContain("Apache-2.0", props, StringComparison.Ordinal);
        Assert.DoesNotContain(">MIT<", props, StringComparison.Ordinal);

        foreach (var project in Directory.GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(project);
            Assert.DoesNotContain("Apache-2.0", text, StringComparison.Ordinal);
            Assert.DoesNotContain("PackageLicenseExpression>MIT<", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-020")]
    [Trait("FR", "FR-RIDE-030")]
    [Trait("AC", "AC-RIDE-030-001")]
    [Trait("AC", "AC-RIDE-030-002")]
    public void Notices_state_sealed_payloads_stay_access_controlled()
    {
        var notice = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "NOTICE"));
        Assert.Contains("Sealed evidence payloads", notice, StringComparison.Ordinal);
        Assert.Contains("not open-source", notice, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(LicenseMetadata.Spdx, "GPL-2.0-only");
    }
}
