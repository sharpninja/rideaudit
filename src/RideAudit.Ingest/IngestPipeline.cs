using System.Security.Cryptography;
using System.Text;
using RideAudit.Contracts;

namespace RideAudit.Ingest;

public sealed record IngestCommand(
    string DriverId,
    string Jurisdiction,
    string Purpose,
    string ConsentStatement,
    bool ConsentGranted,
    string Provenance);

public sealed record ImportResult(
    string ImportId,
    string Status,
    string ContentHashHex,
    int Version,
    string GapNotice,
    string Provenance);

public sealed class IngestPipeline
{
    /// <summary>NaN, infinities and out-of-range values are not coordinates; such rows are skipped like other malformed rows.</summary>
    private static bool IsCoordinate(double value, double limit) => double.IsFinite(value) && Math.Abs(value) <= limit;

    private readonly NormalizedStore _store;
    private readonly ImportKeyRing _keys;
    private readonly IClock _clock;

    public IngestPipeline(NormalizedStore store, ImportKeyRing keys, IClock clock)
    {
        _store = store;
        _keys = keys;
        _clock = clock;
    }

    public NormalizedStore Store => _store;
    public ImportKeyRing Keys => _keys;

    public ImportResult IngestPrivacyExport(IngestCommand command, byte[] zipBytes, string sourceLabel)
    {
        RequireConsent(command);
        var hash = Ids.Hex(Ids.Sha256(zipBytes));
        var existing = _store.FindHash(command.DriverId, hash);
        if (existing is not null)
            return new ImportResult(existing.ImportId, "duplicate-hash", hash, existing.Version, "", existing.Provenance);

        var importId = Ids.New("imp-");
        var parsed = PrivacyExportParser.Parse(zipBytes, importId, command.DriverId, command.Jurisdiction);
        var gap = Gap(parsed);
        var status = !parsed.DictionaryPresent ? "unverified-package" : gap.Length > 0 || parsed.RejectedFalseLabels.Count > 0 ? "imported-with-gap" : "imported";
        var record = Seal(command, importId, hash, PrivacyExportParser.ParserVersion, command.Provenance.Length == 0 ? ProvenanceTags.PrivacyExport : command.Provenance, status, zipBytes);
        _store.Dictionary.AddRange(parsed.Dictionary);
        foreach (var trip in parsed.Trips)
            _store.AddTrip(trip);
        _store.Scores.AddRange(parsed.Scores);
        _store.Hours.AddRange(parsed.Hours);
        _store.Unverified.AddRange(parsed.Unverified);
        _ = sourceLabel;
        return new ImportResult(record.ImportId, status, hash, record.Version, gap, record.Provenance);
    }

    public ImportResult IngestThirdParty(IngestCommand command, byte[] csvBytes, string sourceName)
    {
        RequireConsent(command);
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Third-party source name is required.");
        var hash = Ids.Hex(Ids.Sha256(csvBytes));
        var existing = _store.FindHash(command.DriverId, hash);
        if (existing is not null)
            return new ImportResult(existing.ImportId, "duplicate-hash", hash, existing.Version, "", existing.Provenance);

        var importId = Ids.New("imp-");
        var text = Encoding.UTF8.GetString(csvBytes);
        foreach (var row in PrivacyExportParser.ReadCsv(text).Skip(1))
        {
            if (row.Count < 4 || !long.TryParse(row[0], out var when) || !double.TryParse(row[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                || !double.TryParse(row[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng)
                || !IsCoordinate(lat, 90) || !IsCoordinate(lng, 180))
                continue;
            _store.Locations.Add(new LocationRow(Ids.New("loc-"), importId, command.DriverId, command.Jurisdiction, when, lat, lng, ProvenanceTags.ThirdParty, "third-party-sample"));
        }
        var record = Seal(command, importId, hash, "third-party-csv-1", ProvenanceTags.ThirdParty, "imported", csvBytes);
        return new ImportResult(record.ImportId, "imported", hash, record.Version, "", ProvenanceTags.ThirdParty);
    }

    public ImportResult RecordManualScore(IngestCommand command, long observedUnixMillis, int overall, int gentle, int steering, int mount, int speed)
    {
        RequireConsent(command);
        var canonical = string.Join('|', command.DriverId, observedUnixMillis, overall, gentle, steering, mount, speed, ProvenanceTags.Manual);
        var hash = Ids.Hex(Ids.Sha256Utf8(canonical));
        var existing = _store.FindHash(command.DriverId, hash);
        if (existing is not null)
            return new ImportResult(existing.ImportId, "duplicate-hash", hash, existing.Version, "", existing.Provenance);
        var importId = Ids.New("imp-");
        _store.Scores.Add(new ScoreRow(importId, command.DriverId, observedUnixMillis, overall, gentle, steering, mount, speed, ProvenanceTags.Manual));
        var record = Seal(command, importId, hash, "manual-score-1", ProvenanceTags.Manual, "imported", Encoding.UTF8.GetBytes(canonical));
        return new ImportResult(record.ImportId, "imported", hash, record.Version, "", ProvenanceTags.Manual);
    }

    private ImportRecord Seal(IngestCommand command, string importId, string hash, string parserVersion, string provenance, string status, byte[] plaintext)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(key, 16))
            aes.Encrypt(nonce, plaintext, cipher, tag);
        var blob = new byte[nonce.Length + tag.Length + cipher.Length];
        nonce.CopyTo(blob, 0);
        tag.CopyTo(blob, nonce.Length);
        cipher.CopyTo(blob, nonce.Length + tag.Length);
        var version = _store.NextVersion(command.DriverId);
        var record = new ImportRecord(importId, command.DriverId, hash, version, parserVersion, provenance, status, blob, _clock.UtcNow.ToUnixTimeMilliseconds());
        _keys.Put(importId, key);
        _store.Remember(record);
        _store.Consents.Add(new ConsentLedgerEntry(importId, command.DriverId, record.AtUnixMillis, command.Jurisdiction, command.Purpose, command.ConsentStatement, true, provenance));
        return record;
    }

    private static void RequireConsent(IngestCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.DriverId) || string.IsNullOrWhiteSpace(command.Jurisdiction) || string.IsNullOrWhiteSpace(command.Purpose))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver, jurisdiction, and purpose are required.");
        if (!command.ConsentGranted || string.IsNullOrWhiteSpace(command.ConsentStatement))
            throw new RideAuditException(ErrorCodes.ConsentRequired, "Provenance consent is required before an import is stored.");
    }

    private static string Gap(ParsedExport parsed)
    {
        var fieldNotice = parsed.FieldGaps.Count == 0
            ? ""
            : string.Join(" ", parsed.FieldGaps.Select(gap => gap.Detail));
        var available = new HashSet<string>(StringComparer.Ordinal);
        if (parsed.Trips.Count > 0)
            available.Add("trip_records");
        if (parsed.Scores.Count > 0)
            available.Add("smooth_cruiser");
        if (parsed.Hours.Count > 0)
            available.Add("online_hours");
        var missing = parsed.Dictionary.Any(row => (row.Signal is "smooth_cruiser" or "imu" or "precise_gps") && !available.Contains(row.Signal))
            || !parsed.DictionaryPresent
            || parsed.RejectedFalseLabels.Count > 0
            || parsed.Unverified.Count > 0;
        if (!missing && parsed.Scores.Count > 0 && parsed.Unverified.Count == 0 && fieldNotice.Length == 0)
            return "";
        if (parsed.Scores.Count == 0 || parsed.RejectedFalseLabels.Count > 0 || parsed.Unverified.Count > 0 || !parsed.DictionaryPresent || fieldNotice.Length > 0)
            return JoinGap(fieldNotice, ApiGapNotice.Text);
        return JoinGap(fieldNotice, "");
    }

    private static string JoinGap(string fieldNotice, string rest)
    {
        if (fieldNotice.Length == 0)
            return rest;
        if (rest.Length == 0)
            return fieldNotice;
        return fieldNotice + " " + rest;
    }
}
