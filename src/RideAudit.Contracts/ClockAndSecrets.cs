using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RideAudit.Contracts;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class FakeClock : IClock
{
    public FakeClock(DateTimeOffset utcNow) => UtcNow = utcNow;

    public DateTimeOffset UtcNow { get; set; }

    public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
}

public static class Ids
{
    public static string New(string prefix)
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return prefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string Token(string prefix)
    {
        var raw = RandomNumberGenerator.GetBytes(32);
        return prefix + Base64Url(raw);
    }

    public static string Base64Url(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static byte[] Sha256(byte[] data) => SHA256.HashData(data);

    public static byte[] Sha256Utf8(string text) => SHA256.HashData(Encoding.UTF8.GetBytes(text));

    public static string Hex(byte[] data) => Convert.ToHexString(data).ToLowerInvariant();
}

public sealed class SecretRedactor
{
    private static readonly Regex TokenPattern = new(
        @"ra1\.[A-Za-z0-9_-]+|ra-recover\.[A-Za-z0-9_-]+|fixture\.v1\.[A-Za-z0-9_-]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly List<string> _secrets = new();

    public void Track(string? secret)
    {
        if (!string.IsNullOrEmpty(secret) && secret.Length >= 8 && !_secrets.Contains(secret))
            _secrets.Add(secret);
    }

    public string Apply(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return message ?? "";

        var result = TokenPattern.Replace(message, "[redacted]");
        foreach (var secret in _secrets)
            result = result.Replace(secret, "[redacted]", StringComparison.Ordinal);
        return result;
    }
}

public sealed class InMemoryLogSink
{
    private readonly SecretRedactor _redactor;
    private readonly List<string> _lines = new();

    public InMemoryLogSink(SecretRedactor redactor) => _redactor = redactor;

    public IReadOnlyList<string> Lines
    {
        get { lock (_lines) return _lines.ToArray(); }
    }

    public void Write(string line)
    {
        var redacted = _redactor.Apply(line);
        lock (_lines) _lines.Add(redacted);
    }
}

public interface ISecretsVault
{
    string? Get(string name);
}

public sealed class EnvironmentSecretsVault : ISecretsVault
{
    public string? Get(string name) => Environment.GetEnvironmentVariable(name);
}

public sealed class MemorySecretsVault : ISecretsVault
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public void Set(string name, string value) => _values[name] = value;

    public string? Get(string name) => _values.TryGetValue(name, out var value) ? value : null;
}
