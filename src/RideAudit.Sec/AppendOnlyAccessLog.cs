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
    public const string Counsel = "counsel";
}

/// <summary>
/// FR-RIDE-202. Precise geolocation is limited to auditor, admin, and counsel.
/// The subject role receives a masked coordinate.
/// </summary>
public static class LocationAccessPolicy
{
    public static bool MayViewPreciseLocation(string role) =>
        role is Roles.Auditor or Roles.Admin or Roles.Counsel;
}

public sealed class RoleDirectory
{
    private readonly Dictionary<string, HashSet<string>> _granted = new(StringComparer.Ordinal);

    public void GrantBootstrap(string driverId, string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driverId);
        EnsureKnown(role);
        if (!_granted.TryGetValue(driverId, out var set))
        {
            set = new HashSet<string>(StringComparer.Ordinal);
            _granted[driverId] = set;
        }
        set.Add(role);
    }

    public void Grant(string actorRole, string driverId, string role)
    {
        if (!string.Equals(actorRole, Roles.Admin, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.AuthForbidden, "Only an admin can grant roles.");
        GrantBootstrap(driverId, role);
    }

    public bool Is(string driverId, string role)
    {
        if (string.Equals(role, Roles.Subject, StringComparison.Ordinal))
            return true;
        return _granted.TryGetValue(driverId, out var set) && set.Contains(role);
    }

    public static bool IsElevated(string role) =>
        role is Roles.Admin or Roles.Auditor or Roles.Counsel;

    private static void EnsureKnown(string role)
    {
        if (role is not (Roles.Subject or Roles.Auditor or Roles.Admin or Roles.Counsel))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Role is not recognized.");
    }
}
