// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Chain.OpenTimestamps;

public sealed record OtsSubmitResult(
    string Status,
    byte[] ProofBytes,
    string Disclaimer,
    string? ChainId = null,
    string? TransactionReference = null,
    long? BlockHeight = null,
    long? WriteTimeUnixMillis = null);

public interface IOtsCalendar
{
    string ProofSource { get; }

    OtsSubmitResult Submit(byte[] receiptCoreDigest);
}

public static class CalendarEndpoints
{
    public const string DocumentedFixture = "documented-fixture";

    public static bool IsHttpsUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    public static bool IsDocumentedFixture(string? value) =>
        string.Equals(value, DocumentedFixture, StringComparison.Ordinal);
}
