using RideAudit.Privacy;
using RideAudit.Server.Identity;

namespace RideAudit.Server.Admission;

/// <summary>FR-RIDE-010: feeds the subject's identity-side records into the access export.</summary>
public sealed class DirectorySubjectAccountSource : ISubjectAccountSource
{
    private readonly DriverDirectory _directory;

    public DirectorySubjectAccountSource(DriverDirectory directory) => _directory = directory;

    public IReadOnlyDictionary<string, object> SubjectDatasets(string driverId)
    {
        var export = _directory.ExportSubject(driverId);
        if (export is null)
            return new Dictionary<string, object>(StringComparer.Ordinal);
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["account/account.json"] = export.Account,
            ["account/vehicles.json"] = export.Vehicles,
            ["account/profiles.json"] = export.Profiles,
            ["account/sessions.json"] = export.Sessions,
        };
    }
}
