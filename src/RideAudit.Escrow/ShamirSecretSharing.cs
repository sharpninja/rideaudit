using System.Security.Cryptography;

namespace RideAudit.Escrow;

public sealed record ShamirShare(byte X, byte[] Data);

/// <summary>
/// Byte-wise Shamir secret sharing over GF(2^8) with the AES polynomial 0x11b.
/// </summary>
public sealed class ShamirSecretSharing
{
    public IReadOnlyList<ShamirShare> Split(byte[] secret, int threshold, int shares)
    {
        if (secret.Length == 0)
            throw new ArgumentException("Secret is empty.", nameof(secret));
        if (threshold < 2 || shares < threshold || shares > 255)
            throw new ArgumentOutOfRangeException(nameof(threshold), "M-of-N requires 2 <= M <= N <= 255.");

        var result = new ShamirShare[shares];
        for (var i = 0; i < shares; i++)
            result[i] = new ShamirShare((byte)(i + 1), new byte[secret.Length]);

        var coefficients = new byte[threshold];
        for (var b = 0; b < secret.Length; b++)
        {
            coefficients[0] = secret[b];
            RandomNumberGenerator.Fill(coefficients.AsSpan(1));
            while (coefficients[threshold - 1] == 0)
                coefficients[threshold - 1] = (byte)RandomNumberGenerator.GetInt32(1, 256);
            for (var s = 0; s < shares; s++)
                result[s].Data[b] = Evaluate(coefficients, result[s].X);
        }
        return result;
    }

    public byte[] Combine(IReadOnlyList<ShamirShare> shares, int threshold)
    {
        if (shares.Count < threshold)
            throw new InvalidOperationException("Quorum not met.");
        var distinct = shares.Select(s => s.X).Distinct().ToArray();
        if (distinct.Length < threshold)
            throw new InvalidOperationException("Duplicate shares do not satisfy quorum.");
        var length = shares[0].Data.Length;
        if (shares.Any(s => s.Data.Length != length))
            throw new InvalidOperationException("Share lengths differ.");

        var selected = shares.Take(threshold).ToArray();
        var secret = new byte[length];
        for (var b = 0; b < length; b++)
        {
            byte value = 0;
            for (var i = 0; i < selected.Length; i++)
            {
                byte basis = 1;
                for (var j = 0; j < selected.Length; j++)
                {
                    if (i == j)
                        continue;
                    basis = Gf256.Mul(basis, Gf256.Div(selected[j].X, (byte)(selected[i].X ^ selected[j].X)));
                }
                value ^= Gf256.Mul(selected[i].Data[b], basis);
            }
            secret[b] = value;
        }
        return secret;
    }

    private static byte Evaluate(byte[] coefficients, byte x)
    {
        byte y = 0;
        byte xPow = 1;
        foreach (var coefficient in coefficients)
        {
            y ^= Gf256.Mul(coefficient, xPow);
            xPow = Gf256.Mul(xPow, x);
        }
        return y;
    }
}

internal static class Gf256
{
    private static readonly byte[] Exp = new byte[512];
    private static readonly byte[] Log = new byte[256];

    static Gf256()
    {
        // Generator 0x03. Multiplying by 0x02 is not primitive for polynomial 0x11b.
        var x = 1;
        for (var i = 0; i < 255; i++)
        {
            Exp[i] = (byte)x;
            Log[x] = (byte)i;
            var doubled = x << 1;
            if ((doubled & 0x100) != 0)
                doubled ^= 0x11b;
            x = doubled ^ x;
        }
        for (var i = 255; i < 512; i++)
            Exp[i] = Exp[i - 255];
    }

    public static byte Mul(byte a, byte b)
    {
        if (a == 0 || b == 0)
            return 0;
        return Exp[Log[a] + Log[b]];
    }

    public static byte Div(byte a, byte b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        if (a == 0)
            return 0;
        return Exp[Log[a] + 255 - Log[b]];
    }
}
