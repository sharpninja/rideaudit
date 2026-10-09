using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using RideAudit.Contracts;
using RideAudit.Ingest;
using RideAudit.Sec;
using RideAudit.Server.Counsel;

namespace RideAudit.Privacy;

/// <summary>Unmasked precise location presentation (FR-RIDE-077). Not a masking transform.</summary>
public sealed record PresentedLocation(string SampleId, string Latitude, string Longitude, bool Precise);

public sealed record AccessExport(string ExportId, string Status, byte[] ZipBytes);

public sealed record DeletionResult(string Status, bool Deleted, bool CustodyCiphertextRetained);

/// <summary>FR-RIDE-010: the subject's identity-side records (account, vehicles, profiles, sessions) for the access export, keyed by archive path.</summary>
public interface ISubjectAccountSource
{
    IReadOnlyDictionary<string, object> SubjectDatasets(string driverId);

    /// <summary>The subject's tenant id, used to attribute access-log entries to the subject.</summary>
    string? SubjectTenant(string driverId) => null;
}

/// <summary>Presents stored coordinates without masking (FR-RIDE-077).</summary>
public static class GeoPresent
{
    public static PresentedLocation Present(LocationRow row)
    {
        return new PresentedLocation(
            row.SampleId,
            row.Latitude.ToString("G17", CultureInfo.InvariantCulture),
            row.Longitude.ToString("G17", CultureInfo.InvariantCulture),
            true);
    }
}

public sealed class PrivacyDesk
{
    private readonly NormalizedStore _store;
    private readonly ImportKeyRing _keys;
    private readonly AppendOnlyAccessLog _access;
    private readonly IClock _clock;
    private readonly ISubmittedRecordSource _records;
    private readonly ISubjectAccountSource? _accounts;

    // Stored rows are exported as held. A non-finite number must not abort the whole export (FR-RIDE-010).
    private static readonly JsonSerializerOptions DatasetJson = new() { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };

    public PrivacyDesk(NormalizedStore store, ImportKeyRing keys, AppendOnlyAccessLog access, IClock clock, ISubmittedRecordSource records, ISubjectAccountSource? accounts = null)
    {
        _store = store;
        _keys = keys;
        _access = access;
        _clock = clock;
        _records = records;
        _accounts = accounts;
    }

    public AccessExport Export(string actorId, string subjectDriverId)
    {
        Authorize(actorId, subjectDriverId, "dsar-export");
        var imports = _store.Imports.Where(row => row.DriverId == subjectDriverId).ToList();
        var importIds = imports.Select(row => row.ImportId).ToHashSet(StringComparer.Ordinal);
        var submissions = _records.ForDriver(subjectDriverId);
        // FR-RIDE-010 / AC-RIDE-010-001: the export carries the audit-held rows for this subject, not just metadata.
        // Rows with a driver id are filtered on it; import-keyed rows are filtered on the subject's import ids.
        var datasets = new SortedDictionary<string, object>(StringComparer.Ordinal)
        {
            ["data/trips.json"] = _store.Trips.Where(row => row.DriverId == subjectDriverId).ToList(),
            ["data/scores.json"] = _store.Scores.Where(row => row.DriverId == subjectDriverId).ToList(),
            ["data/online-hours.json"] = _store.Hours.Where(row => row.DriverId == subjectDriverId).ToList(),
            ["data/locations.json"] = _store.Locations.Where(row => row.DriverId == subjectDriverId).ToList(),
            ["data/consents.json"] = _store.Consents.Where(row => row.DriverId == subjectDriverId).ToList(),
            ["data/dictionary.json"] = _store.Dictionary.Where(row => importIds.Contains(row.ImportId)).ToList(),
            ["data/unverified.json"] = _store.Unverified.Where(row => importIds.Contains(row.ImportId)).ToList(),
            ["data/access-log.json"] = SubjectAccessLog(subjectDriverId, submissions),
            // The custody journal keeps each submission's full receipt core (vehicle, session, collector,
            // key, collection time, provenance, attestation). It is held data about the subject, not
            // sealed plaintext.
            ["data/receipts.json"] = submissions.Select(row => new
            {
                row.SubmissionId,
                row.CustodyState,
                ReceiptCore = JsonDocument.Parse(Google.Protobuf.JsonFormatter.Default.Format(RideAudit.Protos.Custody.V1.ReceiptCore.Parser.ParseFrom(row.ReceiptCoreBytes))).RootElement,
            }).ToList(),
        };
        if (_accounts is not null)
        {
            foreach (var (name, rows) in _accounts.SubjectDatasets(subjectDriverId))
                datasets[name] = rows;
        }
        var manifest = new
        {
            subjectDriverId,
            license = "GPL-2.0-only",
            sealedPlaintextIncluded = false,
            imports = imports.Select(row => new { row.ImportId, row.ContentHashHex, row.Version, row.ParserVersion, row.Provenance, row.Status }),
            submissions = submissions.Select(row => new { row.SubmissionId, row.CustodyState, contentHash = Ids.Hex(RideAudit.Protos.Custody.V1.ReceiptCore.Parser.ParseFrom(row.ReceiptCoreBytes).ContentHash.ToByteArray()) }),
            datasets = datasets.Keys,
        };
        var zip = BuildZip(manifest, subjectDriverId, imports, datasets);
        var status = imports.Count == 0 && submissions.Count == 0 ? "no-personal-imports" : "exported";
        return new AccessExport(Ids.New("exp-"), status, zip);
    }

    // FR-RIDE-010: access-log entries about the subject, including the export event Authorize just wrote.
    // An entry is the subject's when the subject acted or when it targets the subject's driver id, tenant
    // or a submission. Any other principal's ids in a kept entry are replaced, so the export never
    // discloses another person's identifiers.
    private List<AccessLogEntry> SubjectAccessLog(string subjectDriverId, IReadOnlyList<SubmittedRecordView> submissions)
    {
        var own = new HashSet<string>(StringComparer.Ordinal) { subjectDriverId };
        foreach (var row in submissions)
        {
            own.Add(row.SubmissionId);
            own.Add(RideAudit.Protos.Custody.V1.ReceiptCore.Parser.ParseFrom(row.ReceiptCoreBytes).TenantId);
        }
        var tenant = _accounts?.SubjectTenant(subjectDriverId);
        if (!string.IsNullOrEmpty(tenant))
            own.Add(tenant);
        own.Remove("");
        return _access.Entries
            .Where(entry => entry.ActorId == subjectDriverId || own.Contains(entry.TenantId) || own.Contains(entry.ResourceId))
            .Select(entry => entry with
            {
                ActorId = entry.ActorId == subjectDriverId ? entry.ActorId : "other-principal",
                TenantId = own.Contains(entry.TenantId) ? entry.TenantId : "other-subject",
                ResourceId = own.Contains(entry.ResourceId) ? entry.ResourceId : "other-subject",
            })
            .ToList();
    }

    public DeletionResult Delete(string actorId, string subjectDriverId, string caseId)
    {
        _ = caseId; // retained for wire/API shape; legal-hold blocked-delete path removed
        Authorize(actorId, subjectDriverId, "dsar-delete");

        foreach (var import in _store.Imports.Where(row => row.DriverId == subjectDriverId).ToArray())
            _keys.Remove(import.ImportId);
        _store.TombstoneDriver(subjectDriverId, _clock.UtcNow.ToUnixTimeMilliseconds());
        return new DeletionResult("personal-data-deleted-custody-ciphertext-retained", true, true);
    }

    public IReadOnlyList<PresentedLocation> ViewLocations(string actorId, string driverId)
    {
        Authorize(actorId, driverId, "view-location");
        return _store.Locations.Where(row => row.DriverId == driverId).Select(row => GeoPresent.Present(row)).ToList();
    }

    private void Authorize(string actorId, string driverId, string action)
    {
        if (string.IsNullOrWhiteSpace(driverId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver id is required.");
        var allowed = string.Equals(actorId, driverId, StringComparison.Ordinal);
        _access.Append(new AccessLogEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), actorId, driverId, action, driverId, allowed));
        if (!allowed)
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Caller cannot access another driver.");
    }

    private static byte[] BuildZip(object manifest, string driverId, IReadOnlyList<ImportRecord> imports, IReadOnlyDictionary<string, object> datasets)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var json = JsonSerializer.Serialize(manifest);
            Write(zip, "manifest.json", json);
            Write(zip, "provenance.json", json);
            Write(zip, "provenance.csv", ProvenanceCsv(imports));
            foreach (var (name, rows) in datasets)
                Write(zip, name, JsonSerializer.Serialize(rows, DatasetJson));
            WriteBytes(zip, "summary.pdf", SummaryPdf("RideAudit audit summary. Sealed plaintext is not included. Subject " + driverId + "."));
            Write(zip, "NOTICE.txt", "GPL-2.0-only. This access export lists hashes and provenance and carries the audit-held rows for this subject under data/ and the subject's account records under account/. It does not contain sealed plaintext. Subject " + driverId + ".");
        }
        return stream.ToArray();
    }

    private static string ProvenanceCsv(IReadOnlyList<ImportRecord> imports)
    {
        var builder = new StringBuilder();
        builder.AppendLine("import_id,content_hash,version,parser_version,provenance,status");
        foreach (var row in imports)
            builder.Append(row.ImportId).Append(',').Append(row.ContentHashHex).Append(',').Append(row.Version).Append(',').Append(row.ParserVersion).Append(',').Append(row.Provenance).Append(',').Append(row.Status).AppendLine();
        return builder.ToString();
    }

    private static byte[] SummaryPdf(string text)
    {
        var safe = text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("(", "\\(", StringComparison.Ordinal).Replace(")", "\\)", StringComparison.Ordinal);
        var content = "BT /F1 12 Tf 72 720 Td (" + safe + ") Tj ET";
        var body = new StringBuilder();
        body.Append("%PDF-1.4\n");
        var offsets = new List<int>();
        void Obj(string value)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(body.ToString()));
            body.Append(value);
        }
        Obj("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        Obj("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        Obj("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");
        Obj("4 0 obj\n<< /Length " + content.Length + " >>\nstream\n" + content + "\nendstream\nendobj\n");
        Obj("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
        var xrefAt = Encoding.ASCII.GetByteCount(body.ToString());
        body.Append("xref\n0 6\n");
        body.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
            body.Append(offset.ToString("0000000000", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        body.Append("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n");
        body.Append(xrefAt.ToString(CultureInfo.InvariantCulture));
        body.Append("\n%%EOF\n");
        return Encoding.ASCII.GetBytes(body.ToString());
    }

    private static void Write(ZipArchive zip, string name, string text)
    {
        var entry = zip.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(text);
    }

    private static void WriteBytes(ZipArchive zip, string name, byte[] bytes)
    {
        var entry = zip.CreateEntry(name);
        using var stream = entry.Open();
        stream.Write(bytes, 0, bytes.Length);
    }
}
