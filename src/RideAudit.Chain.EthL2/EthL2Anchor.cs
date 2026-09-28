using RideAudit.Chain;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Chain.EthL2;

/// <summary>
/// S8 scaffold. Selecting Base, Polygon, or dual-anchor fails closed.
/// No transaction hash is invented to satisfy FR-RIDE-018.
/// </summary>
public sealed class EthL2Anchor : IChainAnchor
{
    public EthL2Anchor(string profileId)
    {
        if (profileId is not (ChainProfileIds.EthL2Base or ChainProfileIds.EthL2Polygon or ChainProfileIds.DualBtcOtsL2))
            throw new ArgumentOutOfRangeException(nameof(profileId));
        ProfileId = profileId;
    }

    public string ProfileId { get; }

    public AnchorAttempt Anchor(AnchorRequest request)
    {
        var envelope = new AnchorProofEnvelope
        {
            ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
            ProfileId = ProfileId,
            ProofSource = "eth-l2-unsupported",
            Status = "failed",
            Disclaimer = "Ethereum L2 and dual-anchor profiles are not implemented. No transaction metadata was fabricated.",
            FailureCode = ErrorCodes.ChainProfileUnsupported,
            LiveBitcoinMetadata = false
        };
        return new AnchorAttempt(
            false,
            CustodyState.Quarantined,
            envelope,
            ErrorCodes.ChainProfileUnsupported,
            "Chain profile " + ProfileId + " is configured but not implemented. Record is not admitted.",
            TimeSpan.Zero);
    }
}
