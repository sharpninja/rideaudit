using RideAudit.Attest;
using RideAudit.Chain;
using RideAudit.Chain.EthL2;
using RideAudit.Chain.OpenTimestamps;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;
using RideAudit.Seal;
using RideAudit.Sec;
using RideAudit.Server.Identity;

namespace RideAudit.Server.Admission;

public sealed record AdmissionOutcome(
    string SubmissionId,
    CustodyState State,
    bool Admitted,
    bool CollectionComplete,
    string? RejectCode,
    string Message,
    IReadOnlyList<string> Checks,
    bool CiphertextStored,
    byte[] ReceiptCoreDigest,
    AnchorProofEnvelope? Anchor);

public sealed class SubmitSealedCommand
{
    public DriverPrincipal? Principal { get; init; }
    public string? RawBearerToken { get; init; }
    public string ClientIp { get; init; } = "local";
    public required string IdempotencyKey { get; init; }
    public required string ContentType { get; init; }
    public required byte[] EnvelopeBytes { get; init; }
    public required byte[] SubmittedReceiptBytes { get; init; }
    public required string AttestationToken { get; init; }
    public required string AttestationNonce { get; init; }
    public string BoundKeyId { get; init; } = "";
    public required string SessionId { get; init; }
    public required string VehicleId { get; init; }
}

public sealed class AbuseGuard
{
    private readonly Dictionary<string, Queue<DateTimeOffset>> _window = new(StringComparer.Ordinal);
    private readonly IClock _clock;

    public AbuseGuard(IClock clock) => _clock = clock;

    public int LimitPerMinute { get; set; } = 60;
    public int MaxPayloadBytes { get; set; } = 32 * 1024 * 1024;
    public bool ForceBackpressure { get; set; }

    public void Check(string bucket, int payloadBytes)
    {
        if (payloadBytes > MaxPayloadBytes)
            throw new RideAuditException(ErrorCodes.SizeLimit, "Sealed payload exceeds the configured size limit.");
        if (ForceBackpressure)
            throw new RideAuditException(ErrorCodes.RateLimited, "Admission backpressure rejected the request without admitting it.");

        var now = _clock.UtcNow;
        if (!_window.TryGetValue(bucket, out var hits))
        {
            hits = new Queue<DateTimeOffset>();
            _window[bucket] = hits;
        }
        while (hits.Count > 0 && now - hits.Peek() > TimeSpan.FromMinutes(1))
            hits.Dequeue();
        if (hits.Count >= LimitPerMinute)
            throw new RideAuditException(ErrorCodes.RateLimited, "Authenticated rate limit exceeded.");
        hits.Enqueue(now);
    }
}

public sealed class OperatorAlertSink
{
    public List<(string Code, string Message, string SubmissionId)> Alerts { get; } = new();

    public void Raise(string code, string message, string submissionId) =>
        Alerts.Add((code, message, submissionId));
}

public sealed class AttestationArchive
{
    private readonly Dictionary<string, string> _tokens = new(StringComparer.Ordinal);

    public void Put(string sealedRecordId, string token) => _tokens[sealedRecordId] = token;

    public bool ContainsToken(string token) => _tokens.Values.Contains(token, StringComparer.Ordinal);
}

/// <summary>
/// Fail-closed sealed admission. Public ingest must not open ciphertext.
/// </summary>
public sealed class AdmissionCoordinator
{
    private readonly DriverDirectory _identity;
    private readonly PlayIntegrityVerifier _play;
    private readonly BtcOtsAnchor _ots;
    private readonly AnchoringPolicy _anchoring;
    private readonly IEscrowDirectory _escrow;
    private readonly CustodyJournal _journal;
    private readonly AbuseGuard _abuse;
    private readonly OperatorAlertSink _alerts;
    private readonly InMemoryLogSink _logs;
    private readonly SecretRedactor _redactor;
    private readonly AppendOnlyAccessLog _access;
    private readonly IClock _clock;
    private readonly AttestationArchive _archive;
    private readonly string _policyVersion;
    private readonly IEthL2Client? _l2;
    private readonly Dictionary<string, ChunkUpload> _uploads = new(StringComparer.Ordinal);

    public AdmissionCoordinator(
        DriverDirectory identity,
        PlayIntegrityVerifier play,
        BtcOtsAnchor ots,
        AnchoringPolicy anchoring,
        IEscrowDirectory escrow,
        CustodyJournal journal,
        AbuseGuard abuse,
        OperatorAlertSink alerts,
        InMemoryLogSink logs,
        SecretRedactor redactor,
        AppendOnlyAccessLog access,
        IClock clock,
        AttestationArchive archive,
        string policyVersion,
        IEthL2Client? l2 = null)
    {
        _identity = identity;
        _play = play;
        _ots = ots;
        _anchoring = anchoring;
        _escrow = escrow;
        _journal = journal;
        _abuse = abuse;
        _alerts = alerts;
        _logs = logs;
        _redactor = redactor;
        _access = access;
        _clock = clock;
        _archive = archive;
        _policyVersion = policyVersion;
        _l2 = l2;
    }

    public IReadOnlyList<string> FailureAudit => _journal.FailureAudit;

    public AdmissionOutcome Submit(SubmitSealedCommand command)
    {
        _redactor.Track(command.RawBearerToken);
        _redactor.Track(command.AttestationToken);
        _logs.Write("submit session=" + command.SessionId + " bytes=" + command.EnvelopeBytes.Length);

        if (PlaintextDetector.ContentTypeIsPlain(command.ContentType)
            || (!SealedIngest.IsRecognizedSeal(command.EnvelopeBytes) && PlaintextDetector.BodyLooksLikeMedia(command.EnvelopeBytes)))
        {
            _journal.RememberFailure("plaintext refused; body not stored");
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "Public ingest accepts only sealed ciphertext.");
        }

        var bucket = (command.Principal?.TenantId ?? command.ClientIp) + "|" + command.ClientIp;
        _abuse.Check(bucket, command.EnvelopeBytes.Length);

        if (command.Principal is null)
            throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Idempotency key is required.");
        if (command.SubmittedReceiptBytes.Length == 0)
            throw new RideAuditException(ErrorCodes.ReceiptMissing, "Custody receipt core is required.");

        var bodyHash = Ids.Sha256(Concat(command.EnvelopeBytes, command.SubmittedReceiptBytes));
        var prior = _journal.FindIdempotency(command.Principal.TenantId, command.IdempotencyKey);
        if (prior is not null)
        {
            if (!prior.BodyHash.AsSpan().SequenceEqual(bodyHash))
                throw new RideAuditException(ErrorCodes.IdempotencyConflict, "Idempotency key was reused with a different body.");
            if (prior.State == CustodyState.Admitted)
                return prior.ToOutcome();
        }

        ParsedEnvelope parsed;
        try
        {
            parsed = SealedIngest.Parse(
                command.EnvelopeBytes,
                command.SubmittedReceiptBytes,
                _policyVersion,
                command.Principal.TenantId,
                command.SessionId,
                command.VehicleId);
        }
        catch (RideAuditException ex)
        {
            _journal.RememberFailure(ex.Code);
            throw;
        }

        if (!EnvelopeFormat.IsAcceptedScope(parsed.Header.KeyScope))
            throw new RideAuditException(ErrorCodes.KeyScopeRejected, "Receipt key scope is not session or sample.");
        if (!string.Equals(parsed.Header.PolicyVersion, _policyVersion, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.PolicyMismatch, "Receipt policy version is not the active admission policy.");
        if (!string.Equals(parsed.Header.TenantId, command.Principal.TenantId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Sealed envelope tenant does not match the caller.");
        if (!string.Equals(parsed.Header.SessionId, command.SessionId, StringComparison.Ordinal)
            || !string.Equals(parsed.Header.VehicleId, command.VehicleId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Envelope session or vehicle does not match the submission.");

        var session = _identity.RequireSession(command.Principal, command.SessionId, command.VehicleId);
        _identity.RequireVehicle(command.Principal, command.VehicleId);
        var profile = _identity.GetProfile(command.Principal, command.VehicleId);
        if (profile is null || !profile.Valid)
            throw new RideAuditException(ErrorCodes.ConfigProfileInvalid, "A valid configuration profile is required before admission.");

        if (!string.IsNullOrEmpty(command.BoundKeyId) && !string.Equals(command.BoundKeyId, parsed.Header.KeyId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.AttestationFailed, "Attestation key binding does not match the sealed key id.");

        var verdict = _play.Verify(command.AttestationToken, command.AttestationNonce, session.SessionId, parsed.Header.KeyId);
        if (!verdict.Success || verdict.Evidence is null || verdict.EvidenceHash is null)
        {
            _journal.RememberFailure(verdict.Code);
            throw new RideAuditException(verdict.Code, verdict.Message);
        }

        var expected = ReceiptCoreCodec.Build(new ReceiptCoreInput
        {
            PolicyVersion = _policyVersion,
            ContentHash = parsed.ContentHash,
            AlgorithmId = parsed.Header.AlgorithmId,
            KeyId = parsed.Header.KeyId,
            PublicKeyMaterial = Convert.FromBase64String(parsed.Header.PublicKeyB64),
            KeyScope = parsed.Header.KeyScope,
            ScopeBinding = parsed.Header.ScopeBinding,
            CollectorId = parsed.Header.CollectorId,
            DriverId = command.Principal.DriverId,
            VehicleId = command.VehicleId,
            SessionId = command.SessionId,
            TenantId = command.Principal.TenantId,
            CollectionUnixMillis = parsed.Header.CollectionUnixMillis,
            ProvenanceTag = parsed.Header.ProvenanceTag,
            AttestationEvidenceHash = verdict.EvidenceHash,
            PackageIdentity = verdict.Evidence.PackageName,
            SigningCertDigest = verdict.Evidence.CertDigest,
            Nonce = command.AttestationNonce,
            SealedRecordId = parsed.Header.SealedRecordId
        });

        if (!expected.Bytes.AsSpan().SequenceEqual(command.SubmittedReceiptBytes))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Submitted receipt core does not match the sealed payload.");

        new AlgorithmRegistry(RideAuditPolicy.AlgorithmId).EnsureKnown(parsed.Header.AlgorithmId);

        if (!_escrow.IsEscrowed(parsed.Header.KeyId, command.Principal.TenantId))
            throw new RideAuditException(ErrorCodes.EscrowUnavailable, "Collection key is not escrowed for this tenant.");

        if (_journal.IsReplay(command.Principal.TenantId, Ids.Hex(parsed.ContentHash), command.AttestationNonce, command.IdempotencyKey))
            throw new RideAuditException(ErrorCodes.DuplicateReplay, "Duplicate or replayed sealed submission was rejected.");

        var record = prior ?? _journal.CreatePending(
            command.Principal.TenantId,
            command.Principal.DriverId,
            command.IdempotencyKey,
            bodyHash,
            command.EnvelopeBytes,
            parsed.Ciphertext,
            expected.Bytes,
            expected.Digest,
            command.AttestationNonce,
            Ids.Hex(parsed.ContentHash),
            parsed.Header.SealedRecordId);

        if (!record.Ciphertext.AsSpan().SequenceEqual(parsed.Ciphertext))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Retry refused because it would rewrite sealed ciphertext.");

        _identity.Database.SubmissionCiphertexts.RemoveAll(c => c.AsSpan().SequenceEqual(record.Ciphertext));
        _identity.Database.SubmissionCiphertexts.Add(record.Ciphertext.ToArray());
        _identity.Database.SubmissionReceipts.RemoveAll(c => c.AsSpan().SequenceEqual(record.ReceiptCoreBytes));
        _identity.Database.SubmissionReceipts.Add(record.ReceiptCoreBytes.ToArray());
        _archive.Put(parsed.Header.SealedRecordId, command.AttestationToken);

        var anchorRequest = new AnchorRequest(expected.Bytes, expected.Digest, _policyVersion, profile.ChainProfile);
        AnchorAttempt anchored;
        if (ChainProfileIds.IsPrimaryOts(profile.ChainProfile))
            anchored = _anchoring.Anchor(_ots, anchorRequest);
        else if (profile.ChainProfile is ChainProfileIds.EthL2Base or ChainProfileIds.EthL2Polygon)
            anchored = _anchoring.Anchor(new EthL2Anchor(profile.ChainProfile, _l2), anchorRequest);
        else if (profile.ChainProfile == ChainProfileIds.DualBtcOtsL2)
            anchored = _anchoring.Anchor(new DualProfileAnchor(_ots, new EthL2Anchor(ChainProfileIds.EthL2Base, _l2)), anchorRequest);
        else
            throw new RideAuditException(ErrorCodes.ChainProfileUnsupported, "Chain profile is not configured.");

        record.Anchor = anchored.Envelope;
        var verification = AnchorProofVerifier.Verify(record.ReceiptCoreBytes, record.Ciphertext, anchored.Envelope);
        var policyConfirmed = anchored.Confirmed && verification.Confirmed;
        if (!policyConfirmed)
        {
            record.State = CustodyState.Quarantined;
            record.CollectionComplete = false;
            var code = anchored.FailureCode ?? ErrorCodes.ChainUnconfirmed;
            var message = anchored.OperatorMessage ?? string.Join("; ", verification.Mismatches);
            record.Audit.Add(code + ": " + message);
            _journal.RememberFailure(code + " submission=" + record.SubmissionId);
            _alerts.Raise(code, message, record.SubmissionId);
            _logs.Write("chain failure " + code + " submission=" + record.SubmissionId);
            throw new RideAuditException(code, message) { SubmissionId = record.SubmissionId };
        }

        record.State = CustodyState.Admitted;
        record.CollectionComplete = true;
        record.Audit.Add("admitted");
        _journal.MarkAdmitted(record);
        _logs.Write("admitted submission=" + record.SubmissionId);
        return record.ToOutcome();
    }

    public AdmissionOutcome GetStatus(DriverPrincipal? principal, string submissionId)
    {
        if (principal is null)
            throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");
        var record = _journal.Find(submissionId);
        if (record is null)
            throw new RideAuditException(ErrorCodes.SubmissionNotFound, "Submission was not found.");
        if (record.TenantId != principal.TenantId)
        {
            _access.Append(new AccessLogEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), principal.DriverId, principal.TenantId, "get-admission", submissionId, false));
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Submission belongs to another tenant.");
        }
        _access.Append(new AccessLogEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), principal.DriverId, principal.TenantId, "get-admission", submissionId, true));
        return record.ToOutcome();
    }

    public (bool Complete, AdmissionOutcome? Decision) UploadChunk(
        DriverPrincipal? principal,
        string? rawToken,
        string clientIp,
        string uploadId,
        int index,
        int total,
        byte[] chunk,
        byte[] expectedHash,
        string contentType,
        string sessionId,
        string vehicleId,
        string idempotencyKey,
        byte[]? receiptBytes,
        string? attestationToken,
        string? attestationNonce,
        string? boundKeyId)
    {
        if (principal is null)
            throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");
        if (PlaintextDetector.ContentTypeIsPlain(contentType))
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "Chunk content type is not a sealed envelope.");
        if (index < 0 || total < 1 || index >= total)
            throw new RideAuditException(ErrorCodes.ChunkOutOfOrder, "Chunk index is outside the declared upload.");
        if (!Ids.Sha256(chunk).AsSpan().SequenceEqual(expectedHash))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Chunk content hash does not match.");
        if (index == 0 && PlaintextDetector.BodyLooksLikeMedia(chunk) && !SealedIngest.IsRecognizedSeal(chunk))
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "First chunk looks like plaintext media.");

        var key = principal.TenantId + "|" + uploadId;
        if (!_uploads.TryGetValue(key, out var upload))
        {
            upload = new ChunkUpload(total, sessionId, vehicleId, idempotencyKey, contentType);
            _uploads[key] = upload;
        }
        if (upload.Total != total)
            throw new RideAuditException(ErrorCodes.ChunkOutOfOrder, "Chunk upload declared a conflicting total.");
        if (upload.Chunks.TryGetValue(index, out var existing))
        {
            if (!existing.AsSpan().SequenceEqual(chunk))
                throw new RideAuditException(ErrorCodes.ChunkOutOfOrder, "Chunk index was already stored with different bytes.");
        }
        else
        {
            upload.Chunks[index] = chunk.ToArray();
        }
        if (receiptBytes is { Length: > 0 })
        {
            upload.ReceiptBytes = receiptBytes;
            upload.AttestationToken = attestationToken;
            upload.AttestationNonce = attestationNonce;
            upload.BoundKeyId = boundKeyId;
        }
        if (upload.Chunks.Count < upload.Total)
            return (false, null);

        var assembled = new byte[upload.Chunks.Values.Sum(c => c.Length)];
        var offset = 0;
        for (var i = 0; i < upload.Total; i++)
        {
            var part = upload.Chunks[i];
            part.CopyTo(assembled, offset);
            offset += part.Length;
        }
        var decision = Submit(new SubmitSealedCommand
        {
            Principal = principal,
            RawBearerToken = rawToken,
            ClientIp = clientIp,
            IdempotencyKey = idempotencyKey,
            ContentType = contentType,
            EnvelopeBytes = assembled,
            SubmittedReceiptBytes = upload.ReceiptBytes ?? throw new RideAuditException(ErrorCodes.ReceiptMissing, "Chunked upload is missing a receipt."),
            AttestationToken = upload.AttestationToken ?? "",
            AttestationNonce = upload.AttestationNonce ?? "",
            BoundKeyId = upload.BoundKeyId ?? "",
            SessionId = sessionId,
            VehicleId = vehicleId
        });
        return (true, decision);
    }

    private static byte[] Concat(byte[] left, byte[] right)
    {
        var combined = new byte[left.Length + right.Length];
        left.CopyTo(combined, 0);
        right.CopyTo(combined, left.Length);
        return combined;
    }

    private sealed class ChunkUpload
    {
        public ChunkUpload(int total, string sessionId, string vehicleId, string idempotencyKey, string contentType)
        {
            Total = total;
            SessionId = sessionId;
            VehicleId = vehicleId;
            IdempotencyKey = idempotencyKey;
            ContentType = contentType;
        }

        public int Total { get; }
        public string SessionId { get; }
        public string VehicleId { get; }
        public string IdempotencyKey { get; }
        public string ContentType { get; }
        public Dictionary<int, byte[]> Chunks { get; } = new();
        public byte[]? ReceiptBytes { get; set; }
        public string? AttestationToken { get; set; }
        public string? AttestationNonce { get; set; }
        public string? BoundKeyId { get; set; }
    }
}
