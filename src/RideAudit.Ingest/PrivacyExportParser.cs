using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace RideAudit.Ingest;

public sealed record ParsedExport(
    List<DictionarySignal> Dictionary,
    List<TripRow> Trips,
    List<ScoreRow> Scores,
    List<OnlineHoursRow> Hours,
    List<UnverifiedFile> Unverified,
    List<string> RejectedFalseLabels,
    bool DictionaryPresent);

public static class PrivacyExportParser
{
    public const string ParserVersion = "privacy-export-1";

    public static ParsedExport Parse(byte[] zipBytes, string importId, string driverId, string jurisdiction)
    {
        var dictionary = new List<DictionarySignal>();
        var trips = new List<TripRow>();
        var scores = new List<ScoreRow>();
        var hours = new List<OnlineHoursRow>();
        var unverified = new List<UnverifiedFile>();
        var rejected = new List<string>();
        var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var stream = new MemoryStream(zipBytes, writable: false);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name) || entry.FullName.Contains("..", StringComparison.Ordinal))
                {
                    unverified.Add(new UnverifiedFile(importId, entry.FullName, "rejected-path"));
                    continue;
                }
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                files[entry.FullName.Replace('\\', '/')] = reader.ReadToEnd();
            }
        }
        catch (InvalidDataException ex)
        {
            throw new Contracts.RideAuditException(Contracts.ErrorCodes.ImportRejected, "Privacy export is not a readable ZIP. " + ex.Message);
        }

        var dictionaryPresent = files.Keys.Any(name => name.EndsWith("DataDictionary.csv", StringComparison.OrdinalIgnoreCase));
        foreach (var (name, body) in files)
        {
            if (name.EndsWith("DataDictionary.csv", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var row in ReadCsv(body).Skip(1))
                {
                    if (row.Count < 3)
                        continue;
                    dictionary.Add(new DictionarySignal(importId, row[0].Trim(), row[1].Trim(), row[2].Trim().ToLowerInvariant()));
                }
                continue;
            }
            if (name.EndsWith("trips.csv", StringComparison.OrdinalIgnoreCase))
            {
                trips.AddRange(ReadTrips(body, importId, driverId, jurisdiction));
                continue;
            }
            if (name.EndsWith("scores.csv", StringComparison.OrdinalIgnoreCase))
            {
                scores.AddRange(ReadScores(body, importId, driverId, rejected));
                continue;
            }
            if (name.EndsWith("online_hours.csv", StringComparison.OrdinalIgnoreCase))
            {
                hours.AddRange(ReadHours(body, importId, driverId, jurisdiction));
                continue;
            }
            unverified.Add(new UnverifiedFile(importId, name, "unverified-file-type"));
        }

        return new ParsedExport(dictionary, trips, scores, hours, unverified, rejected, dictionaryPresent);
    }

    private static IEnumerable<TripRow> ReadTrips(string body, string importId, string driverId, string jurisdiction)
    {
        foreach (var row in DataRows(body))
        {
            if (row.Count < 4 || !long.TryParse(row[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
                || !long.TryParse(row[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var end))
                continue;
            yield return new TripRow(importId, driverId, jurisdiction, row[0].Trim(), start, end, row[3].Trim(), ProvenanceTags.PrivacyExport);
        }
    }

    private static IEnumerable<ScoreRow> ReadScores(string body, string importId, string driverId, List<string> rejected)
    {
        foreach (var row in DataRows(body))
        {
            if (row.Count < 7 || !long.TryParse(row[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var when))
                continue;
            var source = row[6].Trim();
            if (source is not (ProvenanceTags.PrivacyExport or ProvenanceTags.Manual))
            {
                rejected.Add(source);
                continue;
            }
            yield return new ScoreRow(
                importId,
                driverId,
                when,
                Int(row[1]),
                Int(row[2]),
                Int(row[3]),
                Int(row[4]),
                Int(row[5]),
                source);
        }
    }

    private static IEnumerable<OnlineHoursRow> ReadHours(string body, string importId, string driverId, string jurisdiction)
    {
        foreach (var row in DataRows(body))
        {
            if (row.Count < 2 || !long.TryParse(row[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
                || !long.TryParse(row[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var end))
                continue;
            yield return new OnlineHoursRow(importId, driverId, jurisdiction, start, end);
        }
    }

    private static IEnumerable<List<string>> DataRows(string body) => ReadCsv(body).Skip(1);

    private static int Int(string text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

    public static List<List<string>> ReadCsv(string body)
    {
        var rows = new List<List<string>>();
        foreach (var line in body.Split('\n'))
        {
            var trimmed = line.Trim('\r', ' ');
            if (trimmed.Length == 0)
                continue;
            rows.Add(trimmed.Split(',').Select(cell => cell.Trim()).ToList());
        }
        return rows;
    }
}
