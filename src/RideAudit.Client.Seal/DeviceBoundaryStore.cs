// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;

namespace RideAudit.Client.Seal;

/// <summary>
/// Device-boundary store. Accepts sealed envelopes only and never rewrites them.
/// </summary>
public sealed class DeviceBoundaryStore
{
    private readonly Dictionary<string, SealedRecord> _records = new(StringComparer.Ordinal);

    public IReadOnlyCollection<SealedRecord> Records => _records.Values;

    public void Commit(SealedRecord record)
    {
        if (record.SealedBefore != HandoffStage.DurableStore)
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_INVALID",
                "FR-RIDE-015",
                "Sealing must complete before durable store.");
        }

        if (!CollectionSealer.LooksSealed(record.Envelope))
        {
            throw new RideAuditFailClosedException(
                "PLAINTEXT_REJECTED",
                "FR-RIDE-015",
                "Durable store rejected an unsealed payload.");
        }

        if (record.PlaintextRetained)
        {
            throw new RideAuditFailClosedException(
                "PLAINTEXT_REJECTED",
                "FR-RIDE-015",
                "Plaintext must not be retained.");
        }

        if (string.IsNullOrWhiteSpace(record.Receipt.ContentHash) ||
            string.IsNullOrWhiteSpace(record.Receipt.Algorithm) ||
            string.IsNullOrWhiteSpace(record.Receipt.KeyId))
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_MISSING",
                "FR-RIDE-015",
                "Sealed record is missing algorithm or content hash.");
        }

        if (_records.ContainsKey(record.Id))
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_INVALID",
                "TR-RIDE-STORE-002",
                "Sealed blobs are immutable. Corrections must be a new version.");
        }

        _records.Add(record.Id, record);
    }

    public SealedRecord Correct(string originalId, SealedRecord replacement)
    {
        if (!_records.ContainsKey(originalId))
        {
            throw new RideAuditFailClosedException(
                "SUBMISSION_NOT_FOUND",
                "TR-RIDE-STORE-002",
                "Cannot correct a record that was never stored.");
        }

        if (string.Equals(replacement.Id, originalId, StringComparison.Ordinal))
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_INVALID",
                "TR-RIDE-STORE-002",
                "Correction must use a new record id.");
        }

        var stored = new SealedRecord
        {
            Id = replacement.Id,
            Envelope = replacement.Envelope,
            Receipt = replacement.Receipt,
            WrappedKey = replacement.WrappedKey,
            Kind = replacement.Kind,
            SealedBefore = replacement.SealedBefore,
            Version = _records[originalId].Version + 1,
            SupersedesId = originalId,
        };
        Commit(stored);
        return stored;
    }

    public bool TryGet(string id, out SealedRecord record) => _records.TryGetValue(id, out record!);
}
