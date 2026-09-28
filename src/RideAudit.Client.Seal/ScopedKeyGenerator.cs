// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Security.Cryptography;
using RideAudit.Client.Core;
using RideAudit.PlayIntegrity;

namespace RideAudit.Client.Seal;

public sealed class ScopedKeyGenerator
{
    public const string ForbiddenScopeId = "all-records";
    public const string ForbiddenKeyId = "LONG_LIVED_SHARED";

    private readonly Dictionary<string, (string KeyId, byte[] Dek, string Binding)> _sessionKeys = new(StringComparer.Ordinal);

    public static string BindingDigest(
        string keyId,
        string nonce,
        string packageIdentity,
        string certDigest,
        string scopeId) =>
        Hashes.Sha256Hex(keyId + "|" + nonce + "|" + packageIdentity + "|" + certDigest + "|" + scopeId);

    public (string KeyId, byte[] Dek, string Binding) Create(
        KeyScope scope,
        string scopeId,
        PlayAuthorization authorization)
    {
        if (!authorization.Accepted || authorization.Evidence is null)
        {
            throw new RideAuditFailClosedException(
                "ATTESTATION_FAILED",
                "FR-RIDE-026",
                "Refusing key generation without an accepted Play Integrity authorization.");
        }

        if (string.IsNullOrWhiteSpace(scopeId) ||
            string.Equals(scopeId, ForbiddenScopeId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scopeId, ForbiddenKeyId, StringComparison.Ordinal))
        {
            throw new RideAuditFailClosedException(
                "KEY_SCOPE_REJECTED",
                "FR-RIDE-016",
                "Long-lived shared keys for all records are rejected.");
        }

        var evidence = authorization.Evidence;
        if (scope == KeyScope.Session)
        {
            var cacheKey = evidence.Nonce + "|" + scopeId;
            if (_sessionKeys.TryGetValue(cacheKey, out var cached))
            {
                return (cached.KeyId, cached.Dek.ToArray(), cached.Binding);
            }
        }

        var material = scope == KeyScope.Session
            ? evidence.Nonce + "|" + scopeId
            : evidence.Nonce + "|" + scopeId + "|" + Guid.NewGuid().ToString("N");
        var keyId = "k-" + Hashes.Sha256Hex(scope.ToString() + "|" + material)[..32];
        var dek = RandomNumberGenerator.GetBytes(32);
        var binding = BindingDigest(keyId, evidence.Nonce, evidence.PackageIdentity, evidence.SigningCertDigest, scopeId);
        if (scope == KeyScope.Session)
        {
            _sessionKeys[evidence.Nonce + "|" + scopeId] = (keyId, dek.ToArray(), binding);
        }

        return (keyId, dek, binding);
    }

    public void DiscardSessionKeys()
    {
        foreach (var entry in _sessionKeys.Values)
        {
            Hashes.Zero(entry.Dek);
        }

        _sessionKeys.Clear();
    }
}
