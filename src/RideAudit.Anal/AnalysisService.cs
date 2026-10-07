using RideAudit.Contracts;
using RideAudit.Ingest;

namespace RideAudit.Anal;

public sealed record CoverageCell(string Signal, bool CollectedBySource, bool AvailableToAuditor, bool Missing, string GapNotice);

public sealed record CoverageMatrix(string DriverId, IReadOnlyList<CoverageCell> Cells);

public sealed record OnlineHoursAudit(bool HoursDataPresent, IReadOnlyList<string> Violations, string Policy);

public sealed record IncidentReport(int TripCount, int ScoreCount, string GapNotice, IReadOnlyList<string> Lines);

public sealed class AnalysisService
{
    private static readonly string[] Signals = ["trip_records", "smooth_cruiser", "online_hours", "precise_gps", "imu", "concierge_location"];
    private readonly NormalizedStore _store;

    public AnalysisService(NormalizedStore store) => _store = store;

    public CoverageMatrix Coverage(string actorId, string driverId, string? importId)
    {
        Authorize(actorId, driverId);
        var dictionary = _store.Dictionary.Where(row => row.ImportId.Length > 0 && (string.IsNullOrEmpty(importId) || row.ImportId == importId) && Owns(driverId, row.ImportId)).ToList();
        var cells = new List<CoverageCell>();
        foreach (var signal in Signals)
        {
            var rows = dictionary.Where(row => string.Equals(row.Signal, signal, StringComparison.Ordinal)).ToList();
            var collected = rows.Any(row => row.Availability == "collected");
            var available = Available(driverId, importId, signal);
            var explicitlyMissing = rows.Any(row => row.Availability is "not_collected" or "unknown");
            var missing = !available && (collected || explicitlyMissing || (rows.Count == 0 && signal is "smooth_cruiser" or "imu" or "precise_gps"));
            var notice = missing && signal is "smooth_cruiser" or "imu" or "precise_gps" or "concierge_location" ? ApiGapNotice.Text : "";
            cells.Add(new CoverageCell(signal, collected, available, missing, notice));
        }
        return new CoverageMatrix(driverId, cells);
    }

    public OnlineHoursAudit OnlineHours(string actorId, string driverId, string jurisdiction, int maxHours, int breakHours)
    {
        Authorize(actorId, driverId);
        var max = maxHours <= 0 ? 12 : maxHours;
        var rest = breakHours <= 0 ? 6 : breakHours;
        var policy = "max " + max + "h then " + rest + "h break" + (maxHours > 0 || breakHours > 0 ? " (regional override)" : " (default 12h / 6h)");
        var rows = _store.Hours.Where(row => row.DriverId == driverId && (jurisdiction.Length == 0 || row.Jurisdiction == jurisdiction)).OrderBy(row => row.StartedUnixMillis).ToList();
        if (rows.Count == 0)
            return new OnlineHoursAudit(false, Array.Empty<string>(), policy + "; hours data absent, no violation inferred");

        var violations = new List<string>();
        for (var i = 0; i < rows.Count; i++)
        {
            var hours = (rows[i].EndedUnixMillis - rows[i].StartedUnixMillis) / 3_600_000d;
            if (hours > max)
                violations.Add("shift " + rows[i].StartedUnixMillis + "-" + rows[i].EndedUnixMillis + " exceeds " + max + "h");
            if (i + 1 < rows.Count && hours >= max)
            {
                var gap = (rows[i + 1].StartedUnixMillis - rows[i].EndedUnixMillis) / 3_600_000d;
                if (gap < rest)
                    violations.Add("break after shift ending " + rows[i].EndedUnixMillis + " is under " + rest + "h");
            }
        }
        return new OnlineHoursAudit(true, violations, policy);
    }

    public IncidentReport Incident(string actorId, string driverId, long startUnixMillis, long endUnixMillis)
    {
        Authorize(actorId, driverId);
        if (endUnixMillis <= startUnixMillis)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Incident window end must be after the start.");
        var trips = _store.TripIndex.Window(startUnixMillis, endUnixMillis).Where(row => row.DriverId == driverId).ToList();
        var scores = _store.Scores.Where(row => row.DriverId == driverId && row.ObservedUnixMillis >= startUnixMillis && row.ObservedUnixMillis < endUnixMillis).ToList();
        var lines = trips.Select(row => "trip " + row.TripId + " " + row.StartedUnixMillis + " provenance " + row.Provenance)
            .Concat(scores.Select(row => "score " + row.ObservedUnixMillis + " overall " + row.Overall + " provenance " + row.Provenance))
            .ToList();
        var notice = lines.Count == 0 ? ApiGapNotice.Text : "";
        return new IncidentReport(trips.Count, scores.Count, notice, lines);
    }

    private bool Available(string driverId, string? importId, string signal) => signal switch
    {
        "trip_records" => _store.Trips.Any(row => Match(row.DriverId, row.ImportId, driverId, importId)),
        "smooth_cruiser" => _store.Scores.Any(row => Match(row.DriverId, row.ImportId, driverId, importId) && row.Provenance is ProvenanceTags.PrivacyExport or ProvenanceTags.Manual),
        "online_hours" => _store.Hours.Any(row => Match(row.DriverId, row.ImportId, driverId, importId)),
        "precise_gps" => _store.Locations.Any(row => Match(row.DriverId, row.ImportId, driverId, importId) && row.Provenance == ProvenanceTags.ThirdParty),
        "concierge_location" => _store.Locations.Any(row => Match(row.DriverId, row.ImportId, driverId, importId) && row.Provenance == ProvenanceTags.Concierge),
        _ => false
    };

    private bool Owns(string driverId, string importId) =>
        _store.Imports.Any(row => row.ImportId == importId && row.DriverId == driverId);

    private static bool Match(string rowDriver, string rowImport, string driverId, string? importId) =>
        rowDriver == driverId && (string.IsNullOrEmpty(importId) || rowImport == importId);

    private static void Authorize(string actorId, string driverId)
    {
        if (string.IsNullOrWhiteSpace(driverId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver id is required.");
        if (!string.Equals(actorId, driverId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Caller cannot read another driver.");
    }
}
