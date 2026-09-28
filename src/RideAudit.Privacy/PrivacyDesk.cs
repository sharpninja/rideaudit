using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using RideAudit.Contracts;
using RideAudit.Ingest;
using RideAudit.Sec;
using RideAudit.Server.Counsel;

namespace RideAudit.Privacy;

public static class RetentionPolicy
{
    public static TimeSpan For(string jurisdiction, string dataClass)
    {
        var california = string.Equals(jurisdiction, "US-CA", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(dataClass, "precise-geo", StringComparison.Ordinal))
            return california ? TimeSpan.FromDays(30) : TimeSpan.FromDays(180);
        return california ? TimeSpan.FromDays(365) : TimeSpan.FromDays(730);
    }
}

public sealed class LegalHoldRegistry
{
    private readonly Dictionary<string, string> _holds = new(StringComparer.Ordinal);

    public bool IsHeld(string driverId) => _holds.ContainsKey(driverId);

    public void Place(string actorRole, string driverId, string caseId)
    {
        if (actorRole is not (Roles.Admin or Roles.Counsel))
            throw new RideAuditException(ErrorCodes.AuthForbidden, "Only admin or counsel can place a legal hold.");
        if (string.IsNullOrWhiteSpace(driverId) || string.IsNullOrWhiteSpace(caseId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver id and case id are required.");
        _holds[driverId] = caseId;
    }

    public string? CaseId(string driverId) => _holds.TryGetValue(driverId, out var caseId) ? caseId : null;
}

public sealed record MaskedLocation(string SampleId, string Latitude, string Longitude, bool Precise);

public sealed record AccessExport(string ExportId, string Status, byte[] ZipBytes);

public sealed record DeletionResult(string Status, bool Deleted, bool CustodyCiphertextRetained);

public static class GeoMask
{
    public static MaskedLocation Present(LocationRow row, string role)
    {
        if (LocationAccessPolicy.MayViewPreciseLocation(role))
        {
            return new MaskedLocation(
                row.SampleId,
                row.Latitude.ToString("G17", CultureInfo.InvariantCulture),
                row.Longitude.ToString("G17", CultureInfo.InvariantCulture),
                true);
        }
        return new MaskedLocation(
            row.SampleId,
            Math.Round(row.Latitude, 1).ToString("0.0", CultureInfo.InvariantCulture),
            Math.Round(row.Longitude, 1).ToString("0.0", CultureInfo.InvariantCulture),
            false);
    }
}

public sealed class PrivacyDesk
{
    private readonly NormalizedStore _store;
    private readonly ImportKeyRing _keys;
    private readonly LegalHoldRegistry _holds;
    private readonly AppendOnlyAccessLog _access;
    private readonly IClock _clock;
    private readonly ISubmittedRecordSource _records;

    public PrivacyDesk(NormalizedStore store, ImportKeyRing keys, LegalHoldRegistry holds, AppendOnlyAccessLog access, IClock clock, ISubmittedRecordSource records)
    {
        _store = store;
        _keys = keys;
        _holds = holds;
        _access = access;
        _clock = clock;
        _records = records;
    }

    public LegalHoldRegistry Holds => _holds;

    public AccessExport Export(string actorId, string actorRole, string subjectDriverId)
    {
        Authorize(actorId, actorRole, subjectDriverId, "dsar-export");
        var imports = _store.Imports.Where(row => row.DriverId == subjectDriverId).ToList();
        var submissions = _records.ForDriver(subjectDriverId);
        var manifest = new
        {
            subjectDriverId,
            license = "GPL-2.0-only",
            sealedPlaintextIncluded = false,
            imports = imports.Select(row => new { row.ImportId, row.ContentHashHex, row.Version, row.ParserVersion, row.Provenance, row.Status }),
            submissions = submissions.Select(row => new { row.SubmissionId, row.CustodyState, contentHash = Ids.Hex(RideAudit.Protos.Custody.V1.ReceiptCore.Parser.ParseFrom(row.ReceiptCoreBytes).ContentHash.ToByteArray()) })
        };
        var zip = BuildZip(manifest, subjectDriverId);
        var status = imports.Count == 0 && submissions.Count == 0 ? "no-personal-imports" : "exported";
        return new AccessExport(Ids.New("exp-"), status, zip);
    }

    public DeletionResult Delete(string actorId, string actorRole, string subjectDriverId, string caseId)
    {
        Authorize(actorId, actorRole, subjectDriverId, "dsar-delete");
        if (_holds.IsHeld(subjectDriverId))
        {
            _access.Append(new AccessLogEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), actorId, subjectDriverId, "dsar-delete-blocked", caseId, false));
            return new DeletionResult("blocked-legal-hold", false, true);
        }
        foreach (var import in _store.Imports.Where(row => row.DriverId == subjectDriverId).ToArray())
            _keys.Remove(import.ImportId);
        _store.TombstoneDriver(subjectDriverId, _clock.UtcNow.ToUnixTimeMilliseconds());
        return new DeletionResult("personal-data-deleted-custody-ciphertext-retained", true, true);
    }

    public IReadOnlyList<MaskedLocation> ViewLocations(string actorId, string actorRole, string driverId)
    {
        Authorize(actorId, actorRole, driverId, "view-location");
        return _store.Locations.Where(row => row.DriverId == driverId).Select(row => GeoMask.Present(row, actorRole)).ToList();
    }

    public int SweepRetention()
    {
        var now = _clock.UtcNow;
        var removed = 0;
        removed += _store.Locations.RemoveAll(row => Expired(row.DriverId, row.Jurisdiction, "precise-geo", row.ObservedUnixMillis, now));
        removed += _store.Trips.RemoveAll(row => Expired(row.DriverId, row.Jurisdiction, "trips", row.StartedUnixMillis, now));
        _store.TripIndex.RemoveAll(row => !_store.Trips.Contains(row));
        return removed;
    }

    private bool Expired(string driverId, string jurisdiction, string dataClass, long observedUnixMillis, DateTimeOffset now)
    {
        if (_holds.IsHeld(driverId))
            return false;
        var age = now - DateTimeOffset.FromUnixTimeMilliseconds(observedUnixMillis);
        return age > RetentionPolicy.For(jurisdiction, dataClass);
    }

    private void Authorize(string actorId, string actorRole, string driverId, string action)
    {
        if (string.IsNullOrWhiteSpace(driverId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver id is required.");
        var allowed = RoleDirectory.IsElevated(actorRole) || string.Equals(actorId, driverId, StringComparison.Ordinal);
        _access.Append(new AccessLogEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), actorId, driverId, action, driverId, allowed));
        if (!allowed)
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Subject role cannot access another driver.");
    }

    private static byte[] BuildZip(object manifest, string driverId)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            Write(zip, "manifest.json", JsonSerializer.Serialize(manifest));
            Write(zip, "NOTICE.txt", "GPL-2.0-only. This portable audit ZIP lists hashes and provenance. It does not contain sealed plaintext. Subject " + driverId + ".");
        }
        return stream.ToArray();
    }

    private static void Write(ZipArchive zip, string name, string text)
    {
        var entry = zip.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(text);
    }
}
