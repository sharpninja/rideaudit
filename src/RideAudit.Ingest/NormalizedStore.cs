using System.Globalization;
using System.Text;

namespace RideAudit.Ingest;

public sealed record TripRow(
    string ImportId,
    string DriverId,
    string Jurisdiction,
    string TripId,
    long StartedUnixMillis,
    long EndedUnixMillis,
    string Status,
    string Provenance);

public sealed record ScoreRow(
    string ImportId,
    string DriverId,
    long ObservedUnixMillis,
    int Overall,
    int GentleBraking,
    int SmoothSteering,
    int PhoneMount,
    int SpeedVsArea,
    string Provenance);

public sealed record OnlineHoursRow(
    string ImportId,
    string DriverId,
    string Jurisdiction,
    long StartedUnixMillis,
    long EndedUnixMillis);

public sealed record LocationRow(
    string SampleId,
    string ImportId,
    string DriverId,
    string Jurisdiction,
    long ObservedUnixMillis,
    double Latitude,
    double Longitude,
    string Provenance,
    string MetricKind);

public sealed record UnverifiedFile(string ImportId, string Name, string Reason);

public sealed record DictionarySignal(string ImportId, string FileName, string Signal, string Availability);

public sealed record ConsentLedgerEntry(
    string ImportId,
    string DriverId,
    long AtUnixMillis,
    string Jurisdiction,
    string Purpose,
    string Statement,
    bool Granted,
    string Provenance);

public sealed record ImportRecord(
    string ImportId,
    string DriverId,
    string ContentHashHex,
    int Version,
    string ParserVersion,
    string Provenance,
    string Status,
    byte[] Ciphertext,
    long AtUnixMillis);

public sealed class ImportKeyRing
{
    private readonly Dictionary<string, byte[]> _keys = new(StringComparer.Ordinal);

    public void Put(string importId, byte[] key) => _keys[importId] = key.ToArray();

    public bool Contains(string importId) => _keys.ContainsKey(importId);

    public void Remove(string importId) => _keys.Remove(importId);

    public string AuditHex() => string.Join(",", _keys.Values.Select(Convert.ToHexString));
}

public sealed class TimeSeriesIndex<T>
{
    private readonly List<Entry> _entries = new();

    public int Count => _entries.Count;
    public int LastComparisons { get; private set; }

    public void Add(long time, T item)
    {
        var entry = new Entry(time, item);
        var index = _entries.BinarySearch(entry, EntryTime.Instance);
        if (index < 0)
            index = ~index;
        _entries.Insert(index, entry);
    }

    public List<T> Window(long startInclusive, long endExclusive)
    {
        LastComparisons = 0;
        var start = LowerBound(startInclusive);
        var end = LowerBound(endExclusive);
        var rows = new List<T>(Math.Max(0, end - start));
        for (var i = start; i < end; i++)
            rows.Add(_entries[i].Item);
        return rows;
    }

    public void RemoveAll(Predicate<T> match) => _entries.RemoveAll(entry => match(entry.Item));

    private int LowerBound(long time)
    {
        var lo = 0;
        var hi = _entries.Count;
        while (lo < hi)
        {
            LastComparisons++;
            var mid = lo + ((hi - lo) / 2);
            if (_entries[mid].Time < time)
                lo = mid + 1;
            else
                hi = mid;
        }
        return lo;
    }

    private readonly record struct Entry(long Time, T Item);

    private sealed class EntryTime : IComparer<Entry>
    {
        public static readonly EntryTime Instance = new();
        public int Compare(Entry x, Entry y) => x.Time.CompareTo(y.Time);
    }
}

public sealed class NormalizedStore
{
    private readonly Dictionary<string, ImportRecord> _byHash = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _versions = new(StringComparer.Ordinal);

    public List<ImportRecord> Imports { get; } = new();
    public List<TripRow> Trips { get; } = new();
    public List<ScoreRow> Scores { get; } = new();
    public List<OnlineHoursRow> Hours { get; } = new();
    public List<LocationRow> Locations { get; } = new();
    public List<UnverifiedFile> Unverified { get; } = new();
    public List<DictionarySignal> Dictionary { get; } = new();
    public List<ConsentLedgerEntry> Consents { get; } = new();
    public TimeSeriesIndex<TripRow> TripIndex { get; } = new();

    public ImportRecord? FindHash(string driverId, string hash) =>
        _byHash.TryGetValue(driverId + "|" + hash, out var row) ? row : null;

    public int NextVersion(string driverId)
    {
        _versions.TryGetValue(driverId, out var version);
        version++;
        _versions[driverId] = version;
        return version;
    }

    public void Remember(ImportRecord record)
    {
        Imports.Add(record);
        _byHash[record.DriverId + "|" + record.ContentHashHex] = record;
    }

    public void AddTrip(TripRow row)
    {
        Trips.Add(row);
        TripIndex.Add(row.StartedUnixMillis, row);
    }

    public string AuditText()
    {
        var builder = new StringBuilder();
        foreach (var row in Imports)
            builder.Append(row.ImportId).Append(row.ContentHashHex).Append(row.ParserVersion).Append(row.Provenance);
        foreach (var row in Trips)
            builder.Append(row.TripId).Append(row.Provenance);
        foreach (var row in Scores)
            builder.Append(row.Overall.ToString(CultureInfo.InvariantCulture)).Append(row.Provenance);
        foreach (var row in Locations)
            builder.Append(row.SampleId).Append(row.MetricKind).Append(row.Provenance);
        foreach (var row in Unverified)
            builder.Append(row.Name).Append(row.Reason);
        foreach (var row in Consents)
            builder.Append(row.Statement).Append(row.Purpose);
        foreach (var row in Hours)
            builder.Append(row.StartedUnixMillis.ToString(CultureInfo.InvariantCulture));
        return builder.ToString();
    }

    public void TombstoneDriver(string driverId, long atUnixMillis)
    {
        var importIds = Imports.Where(row => row.DriverId == driverId).Select(row => row.ImportId).ToHashSet(StringComparer.Ordinal);
        var kept = Consents.Where(row => row.DriverId == driverId).Select(row => row with
        {
            Statement = "",
            Purpose = "deletion-tombstone",
            AtUnixMillis = atUnixMillis
        }).ToList();
        Consents.RemoveAll(row => row.DriverId == driverId);
        Consents.AddRange(kept);
        foreach (var import in Imports.Where(row => row.DriverId == driverId).ToArray())
            _byHash.Remove(driverId + "|" + import.ContentHashHex);
        Imports.RemoveAll(row => row.DriverId == driverId);
        Trips.RemoveAll(row => row.DriverId == driverId);
        TripIndex.RemoveAll(row => row.DriverId == driverId);
        Scores.RemoveAll(row => row.DriverId == driverId);
        Hours.RemoveAll(row => row.DriverId == driverId);
        Locations.RemoveAll(row => row.DriverId == driverId);
        Unverified.RemoveAll(row => importIds.Contains(row.ImportId));
        Dictionary.RemoveAll(row => importIds.Contains(row.ImportId));
    }
}
