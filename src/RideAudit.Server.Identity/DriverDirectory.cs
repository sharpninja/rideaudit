using System.Security.Cryptography;
using RideAudit.Contracts;
using RideAudit.Sec;

namespace RideAudit.Server.Identity;

public sealed record DriverPrincipal(string DriverId, string TenantId, string Email);

public sealed record RegisterDriverResult(string DriverId, string TenantId, string Email, long CreatedUnixMillis, string AccessToken, string RecoveryCode);

public sealed record VehicleRecord(
    string VehicleId,
    string DriverId,
    string TenantId,
    string VinOrPlateKey,
    string Label,
    string Make,
    string Model,
    int Year,
    long RegisteredUnixMillis,
    List<VehicleChangeRecord> Changes);

public sealed record VehicleChangeRecord(long AtUnixMillis, string ActorDriverId, string Summary);

public sealed record ConfigurationProfileRecord(
    string ProfileId,
    string VehicleId,
    string TenantId,
    string Jurisdiction,
    string ChainProfile,
    bool Valid,
    string PolicyVersion,
    string Notes);

public sealed record SubjectAccount(
    string DriverId,
    string TenantId,
    string Email,
    string DisplayName,
    string Jurisdiction,
    string Purpose,
    string ConsentStatement,
    long CreatedUnixMillis);

public sealed record SubjectAccountExport(
    SubjectAccount Account,
    IReadOnlyList<VehicleRecord> Vehicles,
    IReadOnlyList<ConfigurationProfileRecord> Profiles,
    IReadOnlyList<AuditSessionRecord> Sessions);

public sealed record AuditSessionRecord(
    string SessionId,
    string DriverId,
    string TenantId,
    string VehicleId,
    string Status,
    long CreatedUnixMillis,
    bool DualPhone);

public sealed class InMemoryApplicationDatabase
{
    public List<string> AccountLines { get; } = new();
    public List<VehicleRecord> Vehicles { get; } = new();
    public List<AuditSessionRecord> Sessions { get; } = new();
    public List<byte[]> SubmissionCiphertexts { get; } = new();
    public List<byte[]> SubmissionReceipts { get; } = new();

    public byte[] DumpForAudit()
    {
        var parts = new List<byte[]>();
        foreach (var line in AccountLines)
            parts.Add(System.Text.Encoding.UTF8.GetBytes(line));
        foreach (var vehicle in Vehicles)
            parts.Add(System.Text.Encoding.UTF8.GetBytes(vehicle.VehicleId + vehicle.VinOrPlateKey + vehicle.Label + vehicle.DriverId));
        foreach (var cipher in SubmissionCiphertexts)
            parts.Add(cipher);
        foreach (var receipt in SubmissionReceipts)
            parts.Add(receipt);
        var length = parts.Sum(p => p.Length);
        var dump = new byte[length];
        var offset = 0;
        foreach (var part in parts)
        {
            part.CopyTo(dump, offset);
            offset += part.Length;
        }
        return dump;
    }
}

public sealed class DriverDirectory
{
    private readonly IClock _clock;
    private readonly AppendOnlyAccessLog _access;
    private readonly InMemoryApplicationDatabase _database;
    private readonly Dictionary<string, DriverAccount> _byId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DriverAccount> _byEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DriverAccount> _byTokenHash = new(StringComparer.Ordinal);
    private readonly Dictionary<string, VehicleRecord> _vehicles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ConfigurationProfileRecord> _profiles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AuditSessionRecord> _sessions = new(StringComparer.Ordinal);

    public DriverDirectory(IClock clock, AppendOnlyAccessLog access, InMemoryApplicationDatabase database)
    {
        _clock = clock;
        _access = access;
        _database = database;
    }

    public InMemoryApplicationDatabase Database => _database;

    public RegisterDriverResult Register(string email, string displayName, string jurisdiction, string purpose, bool consentAccepted, string consentStatement)
    {
        email = (email ?? "").Trim().ToLowerInvariant();
        if (!email.Contains('@', StringComparison.Ordinal) || string.IsNullOrWhiteSpace(displayName))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver email and display name are required.");
        if (!consentAccepted || string.IsNullOrWhiteSpace(consentStatement) || string.IsNullOrWhiteSpace(jurisdiction) || string.IsNullOrWhiteSpace(purpose))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Consent, jurisdiction, and purpose are required.");
        if (_byEmail.ContainsKey(email))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Driver email is already registered.");

        var token = Ids.Token("ra1.");
        var recovery = Ids.Token("ra-recover.");
        var account = new DriverAccount(
            Ids.New("drv-"),
            Ids.New("ten-"),
            email,
            displayName.Trim(),
            jurisdiction.Trim(),
            purpose.Trim(),
            consentStatement.Trim(),
            _clock.UtcNow.ToUnixTimeMilliseconds(),
            Sha(token),
            Sha(recovery));
        _byId[account.DriverId] = account;
        _byEmail[email] = account;
        _byTokenHash[account.TokenHash] = account;
        _database.AccountLines.Add(account.DriverId + "|" + account.TenantId + "|" + account.Email + "|" + account.TokenHash + "|" + account.RecoveryHash);
        return new RegisterDriverResult(account.DriverId, account.TenantId, account.Email, account.CreatedUnixMillis, token, recovery);
    }

    public RegisterDriverResult Recover(string email, string recoveryCode)
    {
        email = (email ?? "").Trim().ToLowerInvariant();
        if (!_byEmail.TryGetValue(email, out var account) || !FixedEquals(Sha(recoveryCode ?? ""), account.RecoveryHash))
            throw new RideAuditException(ErrorCodes.AuthRequired, "Recovery failed.");
        _byTokenHash.Remove(account.TokenHash);
        var token = Ids.Token("ra1.");
        account.TokenHash = Sha(token);
        _byTokenHash[account.TokenHash] = account;
        return new RegisterDriverResult(account.DriverId, account.TenantId, account.Email, account.CreatedUnixMillis, token, "");
    }

    public DriverPrincipal? Authenticate(string? bearerToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
            return null;
        var hash = Sha(bearerToken);
        if (!_byTokenHash.TryGetValue(hash, out var account))
            return null;
        return new DriverPrincipal(account.DriverId, account.TenantId, account.Email);
    }

    public VehicleRecord RegisterVehicle(DriverPrincipal caller, string vinOrPlate, string label, string make, string model, int year, bool consentAccepted)
    {
        if (!consentAccepted || string.IsNullOrWhiteSpace(vinOrPlate) || string.IsNullOrWhiteSpace(label))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Vehicle registration requires consent, a VIN or plate key, and a label.");
        var record = new VehicleRecord(
            Ids.New("veh-"),
            caller.DriverId,
            caller.TenantId,
            vinOrPlate.Trim(),
            label.Trim(),
            make?.Trim() ?? "",
            model?.Trim() ?? "",
            year,
            _clock.UtcNow.ToUnixTimeMilliseconds(),
            new List<VehicleChangeRecord> { new(_clock.UtcNow.ToUnixTimeMilliseconds(), caller.DriverId, "registered") });
        _vehicles[record.VehicleId] = record;
        _database.Vehicles.Add(record);
        return record;
    }

    public VehicleRecord UpdateVehicle(DriverPrincipal caller, string vehicleId, string label, string make, string model, int year, string changeReason)
    {
        var record = RequireVehicle(caller, vehicleId);
        if (string.IsNullOrWhiteSpace(changeReason))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Vehicle changes require a reason.");
        var updated = record with
        {
            Label = string.IsNullOrWhiteSpace(label) ? record.Label : label.Trim(),
            Make = make?.Trim() ?? record.Make,
            Model = model?.Trim() ?? record.Model,
            Year = year == 0 ? record.Year : year
        };
        updated.Changes.Add(new VehicleChangeRecord(_clock.UtcNow.ToUnixTimeMilliseconds(), caller.DriverId, changeReason.Trim()));
        _vehicles[vehicleId] = updated;
        return updated;
    }

    public IReadOnlyList<VehicleRecord> ListVehicles(DriverPrincipal caller)
    {
        _access.Append(new AccessLogEntry(_clock.UtcNow.ToUnixTimeMilliseconds(), caller.DriverId, caller.TenantId, "list-vehicles", caller.TenantId, true));
        return _vehicles.Values.Where(v => v.TenantId == caller.TenantId && v.DriverId == caller.DriverId).ToArray();
    }

    public ConfigurationProfileRecord PutProfile(DriverPrincipal caller, string vehicleId, string jurisdiction, string chainProfile, bool valid, string notes)
    {
        RequireVehicle(caller, vehicleId);
        if (!ChainProfileIds.Known.Contains(chainProfile))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Chain profile is not a known public profile id.");
        if (string.IsNullOrWhiteSpace(jurisdiction))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Configuration profile jurisdiction is required.");
        var profile = new ConfigurationProfileRecord(
            Ids.New("cfg-"),
            vehicleId,
            caller.TenantId,
            jurisdiction.Trim(),
            chainProfile,
            valid,
            RideAuditPolicy.Version,
            notes?.Trim() ?? "");
        _profiles[vehicleId] = profile;
        return profile;
    }

    public ConfigurationProfileRecord? GetProfile(DriverPrincipal caller, string vehicleId)
    {
        RequireVehicle(caller, vehicleId);
        return _profiles.TryGetValue(vehicleId, out var profile) && profile.TenantId == caller.TenantId ? profile : null;
    }

    public AuditSessionRecord OpenSession(DriverPrincipal caller, string vehicleId, bool dualPhone)
    {
        RequireVehicle(caller, vehicleId);
        var profile = GetProfile(caller, vehicleId);
        if (profile is null || !profile.Valid)
            throw new RideAuditException(ErrorCodes.ConfigProfileInvalid, "A valid configuration profile is required before a collection session.");
        var session = new AuditSessionRecord(
            Ids.New("ses-"),
            caller.DriverId,
            caller.TenantId,
            vehicleId,
            "open",
            _clock.UtcNow.ToUnixTimeMilliseconds(),
            dualPhone);
        _sessions[session.SessionId] = session;
        _database.Sessions.Add(session);
        return session;
    }

    public AuditSessionRecord RequireSession(DriverPrincipal caller, string sessionId, string vehicleId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new RideAuditException(ErrorCodes.SessionInvalid, "Session was not found.");
        if (session.TenantId != caller.TenantId)
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Session belongs to another tenant.");
        if (session.DriverId != caller.DriverId || session.Status != "open" || session.VehicleId != vehicleId)
            throw new RideAuditException(ErrorCodes.SessionInvalid, "Session is closed, mismatched, or not owned by the caller.");
        return session;
    }

    /// <summary>
    /// FR-RIDE-010 access export: the subject's own account, vehicles, profiles and sessions.
    /// Token and recovery hashes are credentials, not personal data about the subject, and are never exported.
    /// </summary>
    public SubjectAccountExport? ExportSubject(string driverId)
    {
        if (!_byId.TryGetValue(driverId, out var account))
            return null;
        var vehicles = _vehicles.Values.Where(row => row.DriverId == driverId).OrderBy(row => row.VehicleId, StringComparer.Ordinal).ToList();
        var vehicleIds = vehicles.Select(row => row.VehicleId).ToHashSet(StringComparer.Ordinal);
        var profiles = _profiles.Values.Where(row => vehicleIds.Contains(row.VehicleId)).OrderBy(row => row.ProfileId, StringComparer.Ordinal).ToList();
        var sessions = _sessions.Values.Where(row => row.DriverId == driverId).OrderBy(row => row.SessionId, StringComparer.Ordinal).ToList();
        return new SubjectAccountExport(
            new SubjectAccount(account.DriverId, account.TenantId, account.Email, account.DisplayName, account.Jurisdiction, account.Purpose, account.ConsentStatement, account.CreatedUnixMillis),
            vehicles,
            profiles,
            sessions);
    }

    public VehicleRecord RequireVehicle(DriverPrincipal caller, string vehicleId)
    {
        if (!_vehicles.TryGetValue(vehicleId, out var vehicle))
            throw new RideAuditException(ErrorCodes.VehicleUnregistered, "Vehicle is not registered.");
        if (vehicle.TenantId != caller.TenantId || vehicle.DriverId != caller.DriverId)
            throw new RideAuditException(ErrorCodes.VehicleUnregistered, "Vehicle is not bound to this driver.");
        return vehicle;
    }

    private static string Sha(string value) => Ids.Hex(Ids.Sha256Utf8(value));

    private static bool FixedEquals(string left, string right)
    {
        var a = System.Text.Encoding.UTF8.GetBytes(left);
        var b = System.Text.Encoding.UTF8.GetBytes(right);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    private sealed class DriverAccount
    {
        public DriverAccount(string driverId, string tenantId, string email, string displayName, string jurisdiction, string purpose, string consentStatement, long createdUnixMillis, string tokenHash, string recoveryHash)
        {
            DriverId = driverId;
            TenantId = tenantId;
            Email = email;
            DisplayName = displayName;
            Jurisdiction = jurisdiction;
            Purpose = purpose;
            ConsentStatement = consentStatement;
            CreatedUnixMillis = createdUnixMillis;
            TokenHash = tokenHash;
            RecoveryHash = recoveryHash;
        }

        public string DriverId { get; }
        public string TenantId { get; }
        public string Email { get; }
        public string DisplayName { get; }
        public string Jurisdiction { get; }
        public string Purpose { get; }
        public string ConsentStatement { get; }
        public long CreatedUnixMillis { get; }
        public string TokenHash { get; set; }
        public string RecoveryHash { get; }
    }
}
