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
/// Append-only view of identity/admission reads (FR-RIDE-203 / TR-RIDE-SEC-003 were killed invent; this log remains for driver self-access audit, not role-based mask RBAC).
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
