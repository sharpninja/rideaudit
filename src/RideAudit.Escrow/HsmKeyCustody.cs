using System.Security.Cryptography;
using RideAudit.Contracts;
using RideAudit.Seal;

namespace RideAudit.Escrow;

public sealed record EscrowPackageDescription(
    string KeyId,
    string TenantId,
    string SealedRecordId,
    int ThresholdM,
    int TotalN,
    IReadOnlyList<string> CustodianIds,
    string SecretKind);

public sealed record HsmAuditEntry(long AtUnixMillis, string Action, string KeyId, string Actor, string Detail);

public sealed class ExpiringWorkingCopy
{
    private readonly IClock _clock;
    private byte[] _plaintext;
    private bool _wiped;

    public ExpiringWorkingCopy(byte[] plaintext, DateTimeOffset expires, IClock clock, string releaseId, string sealedRecordId)
    {
        _plaintext = plaintext;
        Expires = expires;
        _clock = clock;
        ReleaseId = releaseId;
        SealedRecordId = sealedRecordId;
    }

    public DateTimeOffset Expires { get; }
    public string ReleaseId { get; }
    public string SealedRecordId { get; }
    public bool MinimumScope { get; init; } = true;

    public byte[] ReadPlaintext()
    {
        if (_wiped || _clock.UtcNow >= Expires)
        {
            Wipe();
            throw new RideAuditException(ErrorCodes.WorkingCopyExpired, "Authorized working copy has expired.");
        }
        return _plaintext.ToArray();
    }

    public void Wipe()
    {
        if (_wiped)
            return;
        CryptographicOperations.ZeroMemory(_plaintext);
        _wiped = true;
    }
}

public sealed class CourtRelease
{
    public required string ReleaseId { get; init; }
    public required string KeyId { get; init; }
    public required string TenantId { get; init; }
    public required string CaseId { get; init; }
    public required string LegalProcessReference { get; init; }
    public required string Purpose { get; init; }
    public required string RequesterId { get; init; }
    public required int ThresholdM { get; init; }
    public HashSet<string> Approvers { get; } = new(StringComparer.Ordinal);
    public string State { get; set; } = "pending";
}

public sealed record ReleaseLogEntry(
    long AtUnixMillis,
    string ReleaseId,
    string KeyId,
    string CaseId,
    IReadOnlyList<string> Approvers,
    string Purpose,
    string Action);

/// <summary>
/// In-process HSM/KMS boundary. Shares never enter the application database.
/// Logical custodians are distinct ids; production must map them to separate HSMs.
/// </summary>
public sealed class HsmKeyCustody : ICollectionKeySource, IEscrowSink, IEscrowDirectory
{
    private readonly ShamirSecretSharing _shamir = new();
    private readonly IClock _clock;
    private readonly Dictionary<string, EscrowPackage> _packages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CourtRelease> _releases = new(StringComparer.Ordinal);
    private readonly List<HsmAuditEntry> _audit = new();
    private readonly List<ReleaseLogEntry> _releaseLog = new();

    public HsmKeyCustody(IClock clock) => _clock = clock;

    public int GenerateCount { get; private set; }
    public int WorkingCopyOpens { get; private set; }
    public IReadOnlyList<HsmAuditEntry> AuditLog => _audit;
    public IReadOnlyList<ReleaseLogEntry> ReleaseLog => _releaseLog;

    public CollectionKeyMaterial Create(string keyScope, string scopeBinding)
    {
        if (keyScope is not ("session" or "sample"))
            throw new RideAuditException(ErrorCodes.KeyScopeRejected, "HSM refused a shared or empty key scope.");
        GenerateCount++;
        using var rsa = RSA.Create(2048);
        var dek = RandomNumberGenerator.GetBytes(32);
        var publicKey = rsa.ExportSubjectPublicKeyInfo();
        var privateKey = rsa.ExportPkcs8PrivateKey();
        var keyId = "key-" + Ids.Hex(Ids.Sha256(publicKey))[..32];
        _audit.Add(new HsmAuditEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), "generate", keyId, "hsm", "scope=" + keyScope + " binding=" + scopeBinding));
        return new CollectionKeyMaterial
        {
            KeyId = keyId,
            PublicKeySpki = publicKey,
            Dek = dek,
            PrivateKeyPkcs8 = privateKey
        };
    }

    public void EscrowCollectionSecret(EscrowSecret secret)
    {
        if (secret.CustodianIds.Distinct(StringComparer.Ordinal).Count() != secret.TotalN)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Escrow requires distinct custodians equal to N.");
        if (secret.ThresholdM < 2 || secret.TotalN < secret.ThresholdM)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Escrow quorum must be M-of-N with M >= 2.");

        var blob = Encode(secret.Dek, secret.PrivateKeyPkcs8);
        var secretHash = Ids.Sha256(blob);
        var shares = _shamir.Split(blob, secret.ThresholdM, secret.TotalN);
        CryptographicOperations.ZeroMemory(blob);
        CryptographicOperations.ZeroMemory(secret.Dek);
        CryptographicOperations.ZeroMemory(secret.PrivateKeyPkcs8);
        var stored = new List<StoredShare>();
        for (var i = 0; i < shares.Count; i++)
            stored.Add(new StoredShare(secret.CustodianIds[i], shares[i]));

        _packages[secret.KeyId] = new EscrowPackage(
            secret.KeyId,
            secret.TenantId,
            secret.SealedRecordId,
            secret.ThresholdM,
            secret.TotalN,
            secret.CustodianIds.ToArray(),
            secretHash,
            stored);
        _audit.Add(new HsmAuditEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), "escrow", secret.KeyId, "hsm", "m=" + secret.ThresholdM + " n=" + secret.TotalN));
    }

    public bool IsEscrowed(string keyId, string tenantId) =>
        _packages.TryGetValue(keyId, out var package) && package.TenantId == tenantId;

    /// <summary>
/// Returns copies of share payloads so tests can prove they are absent from the application database.
/// Not an admission or counsel RPC.
/// </summary>
public IReadOnlyList<byte[]> CopySharePayloads(string keyId)
    {
        var package = Require(keyId);
        return package.Shares.Select(share => share.Share.Data.ToArray()).ToArray();
    }

    public EscrowPackageDescription Describe(string keyId)
    {
        var package = Require(keyId);
        return new EscrowPackageDescription(package.KeyId, package.TenantId, package.SealedRecordId, package.ThresholdM, package.TotalN, package.CustodianIds, "wrapped-dek-and-private-key-shares");
    }

    public CourtRelease RequestRelease(string keyId, string tenantId, string caseId, string legalProcess, string purpose, string requesterId)
    {
        var package = Require(keyId);
        if (package.TenantId != tenantId)
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Escrow package is not in this tenant.");
        if (string.IsNullOrWhiteSpace(caseId) || string.IsNullOrWhiteSpace(legalProcess) || string.IsNullOrWhiteSpace(purpose))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Court release requires case id, legal process, and purpose.");
        if (package.CustodianIds.Contains(requesterId, StringComparer.Ordinal))
            throw new RideAuditException(ErrorCodes.AuthForbidden, "Requester cannot be a share custodian.");

        var release = new CourtRelease
        {
            ReleaseId = Ids.New("rel-"),
            KeyId = keyId,
            TenantId = tenantId,
            CaseId = caseId,
            LegalProcessReference = legalProcess,
            Purpose = purpose,
            RequesterId = requesterId,
            ThresholdM = package.ThresholdM
        };
        _releases[release.ReleaseId] = release;
        AppendLog(release, "request");
        _audit.Add(new HsmAuditEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), "release-request", keyId, requesterId, caseId));
        return release;
    }

    public CourtRelease Approve(string releaseId, string custodianId, string statement)
    {
        if (!_releases.TryGetValue(releaseId, out var release))
            throw new RideAuditException(ErrorCodes.SubmissionNotFound, "Release was not found.");
        if (string.IsNullOrWhiteSpace(statement))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Custodian approval statement is required.");
        var package = Require(release.KeyId);
        if (!package.CustodianIds.Contains(custodianId, StringComparer.Ordinal))
            throw new RideAuditException(ErrorCodes.AuthForbidden, "Approver is not a registered custodian for this key.");
        if (custodianId == release.RequesterId)
            throw new RideAuditException(ErrorCodes.AuthForbidden, "Requester cannot approve their own release.");
        release.Approvers.Add(custodianId);
        if (release.Approvers.Count >= release.ThresholdM)
            release.State = "quorum";
        AppendLog(release, "approve:" + custodianId);
        _audit.Add(new HsmAuditEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), "approve", release.KeyId, custodianId, releaseId));
        return release;
    }

    public ExpiringWorkingCopy OpenWorkingCopy(string releaseId, byte[] envelope, TimeSpan ttl)
    {
        if (!_releases.TryGetValue(releaseId, out var release) || release.State != "quorum")
            throw new RideAuditException(ErrorCodes.EscrowQuorum, "Working copy requires M-of-N custodian approvals.");
        var package = Require(release.KeyId);
        var shares = package.Shares.Where(s => release.Approvers.Contains(s.CustodianId)).Take(package.ThresholdM).Select(s => s.Share).ToArray();
        var secret = _shamir.Combine(shares, package.ThresholdM);
        try
        {
            if (!Ids.Sha256(secret).AsSpan().SequenceEqual(package.SecretHash))
                throw new RideAuditException(ErrorCodes.EscrowIntegrity, "Reconstructed escrow secret failed its integrity check.");
            var (dek, privateKey) = Decode(secret);
            CryptographicOperations.ZeroMemory(privateKey);
            WorkingCopyOpens++;
            var plaintext = AuthorizedDecryptor.Open(envelope, dek);
            CryptographicOperations.ZeroMemory(dek);
            release.State = "released";
            AppendLog(release, "open-working-copy");
            return new ExpiringWorkingCopy(plaintext, _clock.UtcNow.Add(ttl), _clock, releaseId, package.SealedRecordId);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secret);
        }
    }

    public bool ReleaseLogHasRemovalApi()
    {
        var methods = typeof(HsmKeyCustody).GetMethods().Select(m => m.Name);
        return methods.Any(name => name is "RemoveRelease" or "ClearReleaseLog" or "DeleteShare");
    }

    private void AppendLog(CourtRelease release, string action)
    {
        _releaseLog.Add(new ReleaseLogEntry(
            _clock.UtcNow.ToUnixTimeMilliseconds(),
            release.ReleaseId,
            release.KeyId,
            release.CaseId,
            release.Approvers.Order(StringComparer.Ordinal).ToArray(),
            release.Purpose,
            action));
    }

    private EscrowPackage Require(string keyId) =>
        _packages.TryGetValue(keyId, out var package)
            ? package
            : throw new RideAuditException(ErrorCodes.EscrowUnavailable, "Key is not escrowed.");

    private static byte[] Encode(byte[] dek, byte[] privateKey)
    {
        var blob = new byte[4 + dek.Length + privateKey.Length];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(blob, dek.Length);
        dek.CopyTo(blob, 4);
        privateKey.CopyTo(blob, 4 + dek.Length);
        return blob;
    }

    private static (byte[] Dek, byte[] PrivateKey) Decode(byte[] secret)
    {
        var dekLength = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(secret);
        if (dekLength <= 0 || 4 + dekLength > secret.Length)
            throw new RideAuditException(ErrorCodes.EscrowIntegrity, "Escrow blob is malformed.");
        return (secret[4..(4 + dekLength)], secret[(4 + dekLength)..]);
    }

    private sealed class StoredShare
    {
        public StoredShare(string custodianId, ShamirShare share)
        {
            CustodianId = custodianId;
            Share = share;
        }

        public string CustodianId { get; }
        public ShamirShare Share { get; }
    }

    private sealed class EscrowPackage
    {
        public EscrowPackage(
            string keyId,
            string tenantId,
            string sealedRecordId,
            int thresholdM,
            int totalN,
            IReadOnlyList<string> custodianIds,
            byte[] secretHash,
            List<StoredShare> shares)
        {
            KeyId = keyId;
            TenantId = tenantId;
            SealedRecordId = sealedRecordId;
            ThresholdM = thresholdM;
            TotalN = totalN;
            CustodianIds = custodianIds;
            SecretHash = secretHash;
            Shares = shares;
        }

        public string KeyId { get; }
        public string TenantId { get; }
        public string SealedRecordId { get; }
        public int ThresholdM { get; }
        public int TotalN { get; }
        public IReadOnlyList<string> CustodianIds { get; }
        public byte[] SecretHash { get; }
        public List<StoredShare> Shares { get; }
    }
}
