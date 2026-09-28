using Google.Protobuf;
using System.Text.Json;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Server.Admission;

public sealed class CustodyRecord
{
    public required string SubmissionId { get; init; }
    public required string TenantId { get; init; }
    public required string DriverId { get; init; }
    public required string IdempotencyKey { get; init; }
    public required byte[] BodyHash { get; init; }
    public required byte[] EnvelopeBytes { get; init; }
    public required string SealedRecordId { get; init; }
    public required byte[] Ciphertext { get; init; }
    public required byte[] ReceiptCoreBytes { get; init; }
    public required byte[] ReceiptCoreDigest { get; init; }
    public required string Nonce { get; init; }
    public required string ContentHashHex { get; init; }
    public CustodyState State { get; set; }
    public bool CollectionComplete { get; set; }
    public AnchorProofEnvelope? Anchor { get; set; }
    public List<string> Audit { get; } = new();

    public AdmissionOutcome ToOutcome() => new(
        SubmissionId,
        State,
        State == CustodyState.Admitted,
        CollectionComplete,
        State == CustodyState.Admitted ? null : Audit.LastOrDefault(),
        State == CustodyState.Admitted ? "admitted" : "not admitted",
        State == CustodyState.Admitted
            ? new[] { "receipt", "hash", "chain", "attestation", "nonce-binding", "package", "escrow", "profile", "tenant" }
            : Array.Empty<string>(),
        true,
        ReceiptCoreDigest,
        Anchor);
}

public sealed class CustodyJournal
{
    private readonly Dictionary<string, CustodyRecord> _byId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _idempotency = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _reserved = new(StringComparer.Ordinal);
    private readonly List<string> _failures = new();

    public IReadOnlyList<string> FailureAudit => _failures;

    public void RememberFailure(string line) => _failures.Add(line);

    public CustodyRecord? Find(string submissionId) =>
        _byId.TryGetValue(submissionId, out var record) ? record : null;

    public CustodyRecord? FindIdempotency(string tenantId, string key) =>
        _idempotency.TryGetValue(tenantId + "|" + key, out var id) ? _byId[id] : null;

    public bool IsReplay(string tenantId, string contentHashHex, string nonce, string idempotencyKey)
    {
        if (_reserved.TryGetValue(tenantId + "|h|" + contentHashHex, out var hashKey) && hashKey != idempotencyKey)
            return true;
        if (_reserved.TryGetValue(tenantId + "|n|" + nonce, out var nonceKey) && nonceKey != idempotencyKey)
            return true;
        return false;
    }

    public CustodyRecord? FindBySealedRecord(string sealedRecordId) =>
        _byId.Values.FirstOrDefault(record => record.SealedRecordId == sealedRecordId);

    public IReadOnlyList<CustodyRecord> Snapshot() => _byId.Values.ToArray();

    public CustodyRecord CreatePending(
        string tenantId,
        string driverId,
        string idempotencyKey,
        byte[] bodyHash,
        byte[] envelopeBytes,
        byte[] ciphertext,
        byte[] receiptCoreBytes,
        byte[] digest,
        string nonce,
        string contentHashHex,
        string sealedRecordId)
    {
        var record = new CustodyRecord
        {
            SubmissionId = Ids.New("sub-"),
            TenantId = tenantId,
            DriverId = driverId,
            IdempotencyKey = idempotencyKey,
            BodyHash = bodyHash.ToArray(),
            EnvelopeBytes = envelopeBytes.ToArray(),
            SealedRecordId = sealedRecordId,
            Ciphertext = ciphertext.ToArray(),
            ReceiptCoreBytes = receiptCoreBytes.ToArray(),
            ReceiptCoreDigest = digest.ToArray(),
            Nonce = nonce,
            ContentHashHex = contentHashHex,
            State = CustodyState.LocalSealedPending,
            CollectionComplete = false
        };
        record.Audit.Add("local-sealed-pending");
        _byId[record.SubmissionId] = record;
        _idempotency[tenantId + "|" + idempotencyKey] = record.SubmissionId;
        _reserved[tenantId + "|h|" + contentHashHex] = idempotencyKey;
        _reserved[tenantId + "|n|" + nonce] = idempotencyKey;
        return record;
    }

    public void MarkAdmitted(CustodyRecord record)
    {
        if (record.State != CustodyState.Admitted)
            throw new InvalidOperationException("Admission requires the admitted state.");
        _reserved[record.TenantId + "|h|" + record.ContentHashHex] = record.IdempotencyKey;
        _reserved[record.TenantId + "|n|" + record.Nonce] = record.IdempotencyKey;
    }

    public byte[] Export()
    {
        var dto = _byId.Values.Select(record => new JournalDto
        {
            SubmissionId = record.SubmissionId,
            TenantId = record.TenantId,
            DriverId = record.DriverId,
            IdempotencyKey = record.IdempotencyKey,
            BodyHash = Convert.ToBase64String(record.BodyHash),
            EnvelopeBytes = Convert.ToBase64String(record.EnvelopeBytes),
            SealedRecordId = record.SealedRecordId,
            Ciphertext = Convert.ToBase64String(record.Ciphertext),
            ReceiptCoreBytes = Convert.ToBase64String(record.ReceiptCoreBytes),
            ReceiptCoreDigest = Convert.ToBase64String(record.ReceiptCoreDigest),
            Nonce = record.Nonce,
            ContentHashHex = record.ContentHashHex,
            State = record.State.ToString(),
            CollectionComplete = record.CollectionComplete,
            Anchor = record.Anchor?.ToByteArray() is { } bytes ? Convert.ToBase64String(bytes) : null,
            Audit = record.Audit.ToArray()
        }).ToArray();
        return JsonSerializer.SerializeToUtf8Bytes(dto);
    }

    public void Import(byte[] utf8)
    {
        var dto = JsonSerializer.Deserialize<JournalDto[]>(utf8) ?? Array.Empty<JournalDto>();
        _byId.Clear();
        _idempotency.Clear();
        _reserved.Clear();
        foreach (var item in dto)
        {
            var record = new CustodyRecord
            {
                SubmissionId = item.SubmissionId,
                TenantId = item.TenantId,
                DriverId = item.DriverId,
                IdempotencyKey = item.IdempotencyKey,
                BodyHash = Convert.FromBase64String(item.BodyHash),
                EnvelopeBytes = Convert.FromBase64String(item.EnvelopeBytes),
                SealedRecordId = item.SealedRecordId,
                Ciphertext = Convert.FromBase64String(item.Ciphertext),
                ReceiptCoreBytes = Convert.FromBase64String(item.ReceiptCoreBytes),
                ReceiptCoreDigest = Convert.FromBase64String(item.ReceiptCoreDigest),
                Nonce = item.Nonce,
                ContentHashHex = item.ContentHashHex,
                State = Enum.Parse<CustodyState>(item.State),
                CollectionComplete = item.CollectionComplete,
                Anchor = item.Anchor is null ? null : AnchorProofEnvelope.Parser.ParseFrom(Convert.FromBase64String(item.Anchor))
            };
            record.Audit.AddRange(item.Audit);
            _byId[record.SubmissionId] = record;
            _idempotency[record.TenantId + "|" + record.IdempotencyKey] = record.SubmissionId;
            _reserved[record.TenantId + "|h|" + record.ContentHashHex] = record.IdempotencyKey;
            _reserved[record.TenantId + "|n|" + record.Nonce] = record.IdempotencyKey;
        }
    }

    private sealed class JournalDto
    {
        public string SubmissionId { get; set; } = "";
        public string TenantId { get; set; } = "";
        public string DriverId { get; set; } = "";
        public string IdempotencyKey { get; set; } = "";
        public string BodyHash { get; set; } = "";
        public string EnvelopeBytes { get; set; } = "";
        public string SealedRecordId { get; set; } = "";
        public string Ciphertext { get; set; } = "";
        public string ReceiptCoreBytes { get; set; } = "";
        public string ReceiptCoreDigest { get; set; } = "";
        public string Nonce { get; set; } = "";
        public string ContentHashHex { get; set; } = "";
        public string State { get; set; } = "";
        public bool CollectionComplete { get; set; }
        public string? Anchor { get; set; }
        public string[] Audit { get; set; } = Array.Empty<string>();
    }
}
