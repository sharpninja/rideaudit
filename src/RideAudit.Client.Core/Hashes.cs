// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Security.Cryptography;
using System.Text;

namespace RideAudit.Client.Core;

public static class Hashes
{
    public static string Sha256Hex(ReadOnlySpan<byte> data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string Sha256Hex(string text) =>
        Sha256Hex(Encoding.UTF8.GetBytes(text));

    public static void Zero(byte[]? buffer)
    {
        if (buffer is { Length: > 0 })
        {
            CryptographicOperations.ZeroMemory(buffer);
        }
    }
}
