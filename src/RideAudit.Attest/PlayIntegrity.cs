using System.Text.Json;
using RideAudit.Contracts;

namespace RideAudit.Attest;

public sealed record AttestationEvidence(
    string Provider,
    string Nonce,
    long ObtainedUnixMillis,
    string PackageName,
    string CertDigest,
    string Verdict,
    bool DeviceCompromised,
    bool Sideloaded,
    bool ModifiedBinary,
    string BoundSessionId,
    string Token);

public sealed record AttestationDecision(
    bool Success,
    string Code,
    string Message,
    AttestationEvidence? Evidence,
    byte[]? EvidenceHash);

public sealed record AllowlistEntry(string PackageName, string CertDigest);

public sealed record AllowlistRotation(string Version, string Actor, long AtUnixMillis, int EntryCount);

public interface IPlayIntegrityTokenDecoder
{
    AttestationEvidence Decode(string token);
}

/// <summary>
/// Production default. A missing Google Play decoder fails closed (plan section 4.5).
/// </summary>
public sealed class FailClosedPlayIntegrityDecoder : IPlayIntegrityTokenDecoder
{
    public AttestationEvidence Decode(string token) =>
        throw new RideAuditException(ErrorCodes.AttestationFailed, "Play Integrity decoder is not configured.");
}

/// <summary>
/// Documented test double. Tokens are fixture.v1.* and are not Google Play responses.
/// </summary>
public sealed class FixturePlayIntegrityDecoder : IPlayIntegrityTokenDecoder
{
    public const string Prefix = "fixture.v1.";

    public AttestationEvidence Decode(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || !token.StartsWith(Prefix, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.AttestationFailed, "Attestation token is not a recognized fixture.");

        byte[] json;
        try
        {
            var body = token[Prefix.Length..].Replace('-', '+').Replace('_', '/');
            switch (body.Length % 4)
            {
                case 2: body += "=="; break;
                case 3: body += "="; break;
            }
            json = Convert.FromBase64String(body);
        }
        catch (FormatException)
        {
            throw new RideAuditException(ErrorCodes.AttestationFailed, "Fixture attestation token is malformed.");
        }

        var payload = JsonSerializer.Deserialize<FixturePayload>(json)
            ?? throw new RideAuditException(ErrorCodes.AttestationFailed, "Fixture attestation payload is empty.");

        return new AttestationEvidence(
            payload.Provider ?? "",
            payload.Nonce ?? "",
            payload.ObtainedUnixMillis,
            payload.PackageName ?? "",
            payload.CertDigest ?? "",
            payload.Verdict ?? "",
            payload.DeviceCompromised,
            payload.Sideloaded,
            payload.ModifiedBinary,
            payload.BoundSessionId ?? "",
            token);
    }

    public static string Issue(FixturePayload payload)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        return Prefix + Ids.Base64Url(json);
    }

    public sealed class FixturePayload
    {
        public string? Provider { get; set; }
        public string? Nonce { get; set; }
        public long ObtainedUnixMillis { get; set; }
        public string? PackageName { get; set; }
        public string? CertDigest { get; set; }
        public string? Verdict { get; set; }
        public bool DeviceCompromised { get; set; }
        public bool Sideloaded { get; set; }
        public bool ModifiedBinary { get; set; }
        public string? BoundSessionId { get; set; }
    }
}

public sealed class PackageAllowlist
{
    private readonly List<AllowlistEntry> _entries;
    private readonly List<AllowlistRotation> _history = new();

    public PackageAllowlist(string version, IEnumerable<AllowlistEntry> entries, string actor, long atUnixMillis)
    {
        Version = version;
        _entries = entries.ToList();
        _history.Add(new AllowlistRotation(version, actor, atUnixMillis, _entries.Count));
    }

    public string Version { get; private set; }
    public IReadOnlyList<AllowlistEntry> Entries => _entries;
    public IReadOnlyList<AllowlistRotation> History => _history;

    public bool Contains(string packageName, string certDigest) =>
        _entries.Any(e => e.PackageName == packageName && e.CertDigest == certDigest);

    public void Rotate(string version, IEnumerable<AllowlistEntry> entries, string actor, long atUnixMillis)
    {
        _entries.Clear();
        _entries.AddRange(entries);
        Version = version;
        _history.Add(new AllowlistRotation(version, actor, atUnixMillis, _entries.Count));
    }
}

public sealed class PlayIntegrityVerifier
{
    private readonly IPlayIntegrityTokenDecoder _decoder;
    private readonly PackageAllowlist _allowlist;
    private readonly IClock _clock;
    private readonly TimeSpan _maxAge;

    public PlayIntegrityVerifier(
        IPlayIntegrityTokenDecoder decoder,
        PackageAllowlist allowlist,
        IClock clock,
        TimeSpan? maxAge = null)
    {
        _decoder = decoder;
        _allowlist = allowlist;
        _clock = clock;
        _maxAge = maxAge ?? TimeSpan.FromMinutes(5);
    }

    public PackageAllowlist Allowlist => _allowlist;

    public static byte[] BindEvidence(string token, string keyId, string sessionId, string nonce)
    {
        var material = token + "\n" + keyId + "\n" + sessionId + "\n" + nonce;
        return Ids.Sha256Utf8(material);
    }

    public AttestationDecision Verify(string token, string expectedNonce, string expectedSessionId, string? boundKeyId)
    {
        AttestationEvidence evidence;
        try
        {
            evidence = _decoder.Decode(token);
        }
        catch (RideAuditException ex)
        {
            return new AttestationDecision(false, ex.Code, ex.Message, null, null);
        }

        if (!string.Equals(evidence.Provider, RideAuditPolicy.PlayProvider, StringComparison.Ordinal))
            return Fail("Attestation provider is not play_integrity.", evidence);

        if (evidence.DeviceCompromised || evidence.ModifiedBinary || evidence.Sideloaded)
            return Fail("Attestation rejected a compromised, modified, or sideloaded device.", evidence);

        if (!string.Equals(evidence.Verdict, RideAuditPolicy.MeetsDeviceIntegrity, StringComparison.Ordinal))
            return Fail("Play Integrity verdict is not MEETS_DEVICE_INTEGRITY.", evidence);

        if (!string.Equals(evidence.Nonce, expectedNonce, StringComparison.Ordinal))
            return Fail("Attestation nonce does not match the collection nonce.", evidence);

        if (!string.Equals(evidence.BoundSessionId, expectedSessionId, StringComparison.Ordinal))
            return Fail("Attestation session binding does not match.", evidence);

        var obtained = DateTimeOffset.FromUnixTimeMilliseconds(evidence.ObtainedUnixMillis);
        if (obtained > _clock.UtcNow.Add(TimeSpan.FromMinutes(1)) || _clock.UtcNow - obtained > _maxAge)
            return Fail("Attestation is stale or not yet valid.", evidence);

        if (!_allowlist.Contains(evidence.PackageName, evidence.CertDigest))
            return Fail("Package identity or signing certificate is not on the allowlist.", evidence);

        byte[]? hash = null;
        if (!string.IsNullOrEmpty(boundKeyId))
            hash = BindEvidence(token, boundKeyId, expectedSessionId, expectedNonce);

        return new AttestationDecision(true, "", "ok", evidence, hash);
    }

    private static AttestationDecision Fail(string message, AttestationEvidence evidence) =>
        new(false, ErrorCodes.AttestationFailed, message, evidence, null);
}
