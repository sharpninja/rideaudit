// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.PlayIntegrity;
using RideAudit.Seal;

namespace RideAudit.Viewer;

public interface IBitcoinHeaderSource
{
    bool TryGet(string blockHash, out BitcoinHeader header);
}

public sealed class MemoryBitcoinHeaders : IBitcoinHeaderSource
{
    private readonly Dictionary<string, BitcoinHeader> _headers = new(StringComparer.OrdinalIgnoreCase);

    public void Add(BitcoinHeader header) => _headers[header.BlockHash] = header;

    public bool TryGet(string blockHash, out BitcoinHeader header) => _headers.TryGetValue(blockHash, out header!);
}

/// <summary>
/// Recomputes an OpenTimestamps-shaped commitment against caller-supplied Bitcoin headers.
/// Does not trust a RideAudit server boolean.
/// </summary>
public static class IndependentOtsVerifier
{
    public static string Fold(string leafHex, IReadOnlyList<string> siblings)
    {
        var current = leafHex.ToLowerInvariant();
        foreach (var sibling in siblings)
        {
            current = Hashes.Sha256Hex(current + "|" + sibling.ToLowerInvariant());
        }

        return current;
    }

    public static (bool Ok, string Detail) Verify(CustodyReceipt receipt, OtsProof proof, IBitcoinHeaderSource headers)
    {
        var committed = Hashes.Sha256Hex(receipt.CanonicalForm());
        if (!string.Equals(committed, proof.CommittedDigestHex, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "OTS committed digest does not match the custody receipt.");
        }

        if (proof.MerkleSiblingsHex.Count == 0)
        {
            return (false, "OTS proof has no merkle siblings.");
        }

        var root = Fold(proof.CommittedDigestHex, proof.MerkleSiblingsHex);
        if (!headers.TryGet(proof.BitcoinBlockHash, out var header))
        {
            return (false, "Bitcoin header for the OTS proof was not supplied.");
        }

        if (!string.Equals(header.MerkleRoot, root, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Merkle root does not match the supplied Bitcoin header.");
        }

        if (!string.Equals(receipt.ChainId, AdmissionPolicy.ChainId, StringComparison.Ordinal))
        {
            return (false, "Receipt chain id is not btc-ots.");
        }

        return (true, "OTS commitment matches the supplied Bitcoin header.");
    }
}

public static class OtsProofBuilder
{
    public static OtsProof Build(CustodyReceipt receipt, string blockHash, string siblingHex, MemoryBitcoinHeaders headers)
    {
        var digest = Hashes.Sha256Hex(receipt.CanonicalForm());
        var siblings = new[] { siblingHex };
        var root = IndependentOtsVerifier.Fold(digest, siblings);
        headers.Add(new BitcoinHeader { BlockHash = blockHash, MerkleRoot = root });
        return new OtsProof
        {
            CommittedDigestHex = digest,
            BitcoinBlockHash = blockHash,
            MerkleSiblingsHex = siblings,
        };
    }
}

public sealed class VerificationGate
{
    private readonly PackageAllowlist _allowlist;
    private readonly IBitcoinHeaderSource _headers;
    private readonly bool _allowSimulatedAttestation;

    public VerificationGate(PackageAllowlist allowlist, IBitcoinHeaderSource headers, bool allowSimulatedAttestation = false)
    {
        _allowlist = allowlist;
        _headers = headers;
        _allowSimulatedAttestation = allowSimulatedAttestation;
    }

    public IReadOnlyList<VerificationCheck> VerifyRecord(ReviewRecord record)
    {
        var checks = new List<VerificationCheck>();
        var receipt = record.Sealed.Receipt;
        var id = record.Sealed.Id;

        void Add(string name, bool pass, string detail) =>
            checks.Add(new VerificationCheck
            {
                RecordId = id,
                Name = name,
                Status = pass ? "pass" : "fail",
                Detail = detail,
            });

        Add("bundle_admitted", record.Admitted, record.Admitted ? "Submission is admitted." : "Submission is not admitted.");

        var hash = Hashes.Sha256Hex(record.Sealed.Envelope);
        Add("payload_hash", string.Equals(hash, receipt.ContentHash, StringComparison.OrdinalIgnoreCase),
            "Recomputed ciphertext hash compared with receipt.");

        var ots = IndependentOtsVerifier.Verify(receipt, record.Ots, _headers);
        Add("ots_receipt", ots.Ok, ots.Detail);

        var simulatedOk = _allowSimulatedAttestation && receipt.AttestationProvider == PlayIntegrityProviders.Stub;
        var realProvider = receipt.AttestationProvider == PlayIntegrityProviders.Real && string.IsNullOrEmpty(receipt.StubNotice);
        var providerOk = realProvider || simulatedOk;
        var allowlisted = _allowlist.IsAllowed(receipt.PackageIdentity, receipt.SigningCertDigest);
        var attestationOk = providerOk && allowlisted && !string.IsNullOrWhiteSpace(receipt.AttestationTokenHash);
        var attestationDetail = providerOk
            ? (allowlisted ? "Package and certificate match the allowlist." : "Package or certificate is not allowlisted.")
            : "Attestation provider is not court-ready Play Integrity.";
        Add("play_attestation", attestationOk, attestationDetail);

        var expectedBinding = ScopedKeyGenerator.BindingDigest(
            receipt.KeyId,
            receipt.Nonce,
            receipt.PackageIdentity,
            receipt.SigningCertDigest,
            receipt.ScopeId);
        var bindingOk = string.Equals(expectedBinding, receipt.KeyBindingDigest, StringComparison.OrdinalIgnoreCase)
            && string.Equals(receipt.KeyId, record.Sealed.WrappedKey.KeyId, StringComparison.Ordinal)
            && string.Equals(receipt.SealedRecordId, record.Sealed.Id, StringComparison.Ordinal);
        Add("nonce_key_binding", bindingOk, bindingOk ? "Nonce and key binding match." : "Nonce or key binding mismatch.");

        if (receipt.Composite)
        {
            var meta = receipt.CompositeMetadata;
            var linksOk = meta is not null && meta.SourceStreamIds.Count >= 2 && meta.SourceContentHashes.Count >= 2;
            Add("source_stream_links", linksOk, linksOk ? "Source streams are linked." : "Source stream links are missing.");
            var clockOk = meta is not null && !string.IsNullOrWhiteSpace(meta.SyncClockOffset);
            Add("clock_offsets", clockOk, clockOk ? "SyncClockOffset is present." : "SyncClockOffset is missing.");
            var overlayOk = meta is not null && !string.IsNullOrWhiteSpace(meta.OverlayManifestVersion) && !string.IsNullOrWhiteSpace(meta.TimelineManifestVersion);
            Add("overlay_timeline", overlayOk, overlayOk ? "Overlay timeline manifest is present." : "Overlay timeline is missing or inconsistent.");
        }
        else
        {
            Add("source_stream_links", true, "Not a composite record.");
            Add("clock_offsets", true, "Not a composite record.");
            Add("overlay_timeline", true, "Not a composite record.");
        }

        return checks;
    }
}
