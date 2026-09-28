using RideAudit.Contracts;

namespace RideAudit.Sec;

public sealed record AccessLogEntry(
    long AtUnixMillis,
    string ActorId,
    string TenantId,
    string Action,
    string ResourceId,
    bool Allowed);

/// <summary>
/// FR-RIDE-203 / TR-RIDE-SEC-003. Append-only view of sensitive identity and admission reads.
/// </summary>
public sealed class AppendOnlyAccessLog
{
    private readonly List<AccessLogEntry> _entries = new();

    public void Append(AccessLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_entries) _entries.Add(entry);
    }

    public IReadOnlyList<AccessLogEntry> Entries
    {
        get { lock (_entries) return _entries.ToArray(); }
    }
}

public static class Roles
{
    public const string Subject = "subject";
    public const string Auditor = "auditor";
    public const string Admin = "admin";
}

/// <summary>
/// S7 scaffold for precise-location least privilege. RideAudit public admission does not
/// return coordinates. The checker fails closed for the subject role.
/// </summary>
public static class LocationAccessPolicy
{
    public static bool MayViewPreciseLocation(string role) =>
        string.Equals(role, Roles.Auditor, StringComparison.Ordinal)
        || string.Equals(role, Roles.Admin, StringComparison.Ordinal);
}
