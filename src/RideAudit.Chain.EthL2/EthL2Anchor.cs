using System.Text;
using RideAudit.Chain;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Chain.EthL2;

public enum L2FixtureMode
{
    UpgradeImmediately,
    FailSubmit
}

public sealed record L2CommitResult(
    bool Confirmed,
    string Status,
    byte[] ProofBytes,
    string Disclaimer,
    string? FailureCode,
    string? ChainId,
    string? TransactionReference,
    long? BlockHeight);

public interface IEthL2Client
{
    string ProofSource { get; }

    L2CommitResult Commit(string profileId, byte[] digest);
}

/// <summary>
/// Documented Base/Polygon stand-in. It never contacts a live chain and never emits a real transaction id.
/// </summary>
public sealed class DocumentedFixtureL2Calendar : IEthL2Client
{
    public const string Disclaimer =
        "SYNTHETIC TEST FIXTURE. Not a live Base, Polygon, or Bitcoin transaction. Not broadcast. " +
        "Transaction references are fixture-scoped. Block heights are intentionally outside production height space.";

    public const long WriteTimeUnixMillis = 1_759_010_000_000;

    public string ProofSource => ProofSources.DocumentedFixtureL2;

    public L2FixtureMode Mode { get; set; } = L2FixtureMode.UpgradeImmediately;
    public int SubmitCalls { get; private set; }

    public static (string ChainId, string TransactionReference, long BlockHeight) Describe(string profileId) => profileId switch
    {
        ChainProfileIds.EthL2Base => (FixtureChainIds.EthL2Base, "fixture:not-an-ethereum-txid:rideaudit-l2-base-fixture-001", 9_000_000_001),
        ChainProfileIds.EthL2Polygon => (FixtureChainIds.EthL2Polygon, "fixture:not-an-ethereum-txid:rideaudit-l2-polygon-fixture-001", 9_000_000_002),
        _ => throw new ArgumentOutOfRangeException(nameof(profileId))
    };

    public (string Status, byte[] Proof, string ChainId, string TransactionReference, long BlockHeight) Submit(string profileId, byte[] digest)
    {
        SubmitCalls++;
        var described = Describe(profileId);
        if (Mode == L2FixtureMode.FailSubmit)
            throw new RideAuditException(ErrorCodes.ChainFailed, "Documented L2 fixture refused the digest submission.");
        return ("upgraded", BuildProof(digest, described.ChainId, described.TransactionReference), described.ChainId, described.TransactionReference, described.BlockHeight);
    }

    L2CommitResult IEthL2Client.Commit(string profileId, byte[] digest)
    {
        var (status, proof, chainId, tx, height) = Submit(profileId, digest);
        return new L2CommitResult(true, status, proof, Disclaimer, null, chainId, tx, height);
    }

    public static byte[] BuildProof(byte[] digest, string chainId, string transactionReference)
    {
        var text = "RIDEL2-FIXTURE-1\n" + chainId + "\n" + transactionReference + "\n" + Ids.Hex(digest) + "\n" + Disclaimer + "\n";
        return Encoding.UTF8.GetBytes(text);
    }
}

public static class DualFixture
{
    public const string TransactionReference = "fixture:not-a-live-txid:rideaudit-dual-btc-ots-l2-fixture-001";
    public const long BlockHeight = 9_000_000_003;

    public static byte[] BuildProof(byte[] digest, byte[] otsProof, byte[] l2Proof)
    {
        var text = "RIDEDUAL-FIXTURE-1\n" + Ids.Hex(digest) + "\n" + Encoding.UTF8.GetString(otsProof) + "\n" + Encoding.UTF8.GetString(l2Proof) + "\n" + DocumentedFixtureL2Calendar.Disclaimer + "\n";
        return Encoding.UTF8.GetBytes(text);
    }
}

/// <summary>
/// Base and Polygon profiles. Without a documented fixture calendar the anchor fails closed and writes no transaction metadata.
/// </summary>
public sealed class EthL2Anchor : IChainAnchor
{
    private readonly IEthL2Client? _client;

    public EthL2Anchor(string profileId)
        : this(profileId, (IEthL2Client?)null)
    {
    }

    public EthL2Anchor(string profileId, DocumentedFixtureL2Calendar? fixture)
        : this(profileId, (IEthL2Client?)fixture)
    {
    }

    public EthL2Anchor(string profileId, IEthL2Client? client)
    {
        if (profileId is not (ChainProfileIds.EthL2Base or ChainProfileIds.EthL2Polygon))
            throw new ArgumentOutOfRangeException(nameof(profileId));
        ProfileId = profileId;
        _client = client;
    }

    public string ProfileId { get; }

    public AnchorAttempt Anchor(AnchorRequest request)
    {
        if (_client is null)
            return FailClosed(request, ProofSources.Unconfigured, ErrorCodes.ChainProfileUnsupported, "Ethereum L2 profile is not backed by a live chain or a documented fixture. No transaction metadata was written.");

        try
        {
            var result = _client.Commit(ProfileId, request.ReceiptCoreDigest);
            if (!result.Confirmed
                || result.Status != "upgraded"
                || string.IsNullOrWhiteSpace(result.ChainId)
                || string.IsNullOrWhiteSpace(result.TransactionReference)
                || result.BlockHeight is null)
            {
                return FailClosed(
                    request,
                    _client.ProofSource,
                    result.FailureCode ?? ErrorCodes.ChainUnconfirmed,
                    result.Disclaimer);
            }

            var envelope = Upgraded(
                request,
                _client.ProofSource,
                ProfileId,
                result.ChainId,
                result.TransactionReference,
                result.BlockHeight.Value,
                result.ProofBytes,
                result.Disclaimer);
            var fixture = string.Equals(_client.ProofSource, ProofSources.DocumentedFixtureL2, StringComparison.Ordinal);
            return new AnchorAttempt(
                true,
                CustodyState.Confirmed,
                envelope,
                null,
                fixture
                    ? "Fixture L2 upgrade recorded. This is not a live chain transaction."
                    : "L2 client returned transaction metadata supplied by the RPC. This is not a fabricated hash.",
                TimeSpan.Zero);
        }
        catch (RideAuditException ex)
        {
            return FailClosed(request, _client.ProofSource, ex.Code, ex.Message);
        }
    }

    private AnchorAttempt FailClosed(AnchorRequest request, string proofSource, string code, string message)
    {
        var envelope = new AnchorProofEnvelope
        {
            ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
            ProfileId = ProfileId,
            ProofSource = proofSource,
            Status = "failed",
            Disclaimer = message,
            FailureCode = code,
            LiveBitcoinMetadata = false
        };
        return new AnchorAttempt(false, CustodyState.Quarantined, envelope, code, message, TimeSpan.Zero);
    }

    internal static AnchorProofEnvelope Upgraded(AnchorRequest request, string proofSource, string profileId, string chainId, string tx, long height, byte[] proof, string disclaimer)
    {
        return new AnchorProofEnvelope
        {
            ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
            ProfileId = profileId,
            ProofSource = proofSource,
            Status = "upgraded",
            ChainId = chainId,
            TransactionReference = tx,
            BlockHeight = height,
            WriteTimeUnixMillis = DocumentedFixtureL2Calendar.WriteTimeUnixMillis,
            ProofBytes = Google.Protobuf.ByteString.CopyFrom(proof),
            Disclaimer = disclaimer,
            FailureCode = "",
            LiveBitcoinMetadata = false
        };
    }
}

/// <summary>
/// Dual anchor admits only when both the Bitcoin OTS leg and the L2 leg confirm.
/// A partial result does not copy either transaction reference onto the receipt.
/// </summary>
public sealed class DualProfileAnchor : IChainAnchor
{
    private readonly IChainAnchor _ots;
    private readonly IChainAnchor _l2;

    public DualProfileAnchor(IChainAnchor ots, IChainAnchor l2)
    {
        _ots = ots;
        _l2 = l2;
    }

    public string ProfileId => ChainProfileIds.DualBtcOtsL2;

    public AnchorAttempt Anchor(AnchorRequest request)
    {
        var ots = _ots.Anchor(request);
        var l2 = _l2.Anchor(request);
        if (!ots.Confirmed || !l2.Confirmed)
        {
            var code = !l2.Confirmed ? l2.FailureCode ?? ErrorCodes.ChainFailed : ots.FailureCode ?? ErrorCodes.ChainFailed;
            var envelope = new AnchorProofEnvelope
            {
                ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
                ProfileId = ProfileId,
                ProofSource = ProofSources.DualIncomplete,
                Status = "failed",
                Disclaimer = "Dual anchor did not confirm both legs. No transaction metadata was copied from a partial result.",
                FailureCode = code,
                LiveBitcoinMetadata = false
            };
            return new AnchorAttempt(false, CustodyState.Quarantined, envelope, code, envelope.Disclaimer, TimeSpan.Zero);
        }

        var proof = DualFixture.BuildProof(request.ReceiptCoreDigest, ots.Envelope.ProofBytes.ToByteArray(), l2.Envelope.ProofBytes.ToByteArray());
        if (!IsDocumentedFixture(ots.Envelope.ProofSource) || !IsDocumentedFixture(l2.Envelope.ProofSource))
        {
            var envelope = new AnchorProofEnvelope
            {
                ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
                ProfileId = ProfileId,
                ProofSource = ProofSources.DualIncomplete,
                Status = "failed",
                Disclaimer = "Live dual-anchor confirmation is not implemented. No transaction metadata was copied from either leg.",
                FailureCode = ErrorCodes.ChainUnconfirmed,
                LiveBitcoinMetadata = false
            };
            return new AnchorAttempt(false, CustodyState.Quarantined, envelope, ErrorCodes.ChainUnconfirmed, envelope.Disclaimer, TimeSpan.Zero);
        }

        var upgraded = EthL2Anchor.Upgraded(
            request,
            ProofSources.DocumentedFixtureDual,
            ProfileId,
            FixtureChainIds.Dual,
            DualFixture.TransactionReference,
            DualFixture.BlockHeight,
            proof,
            DocumentedFixtureL2Calendar.Disclaimer);
        return new AnchorAttempt(true, CustodyState.Confirmed, upgraded, null, "Fixture dual anchor recorded. Neither leg is a live chain transaction.", TimeSpan.Zero);
    }

    private static bool IsDocumentedFixture(string? proofSource) =>
        string.Equals(proofSource, ProofSources.DocumentedFixture, StringComparison.Ordinal)
        || string.Equals(proofSource, ProofSources.DocumentedFixtureL2, StringComparison.Ordinal)
        || string.Equals(proofSource, ProofSources.DocumentedFixtureDual, StringComparison.Ordinal);
}
