using System.Text;
using RideAudit.Chain;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Chain.OpenTimestamps;

public enum FixtureCalendarMode
{
    UpgradeImmediately,
    PendingForever,
    FailSubmit,
    FailUpgrade
}

/// <summary>
/// Documented OpenTimestamps stand-in. It never contacts Bitcoin and never emits a live txid.
/// See Fixtures/btc-ots-fixture.json.
/// </summary>
public sealed class DocumentedFixtureOtsCalendar
{
    public const string FixtureId = "rideaudit-btc-ots-fixture-001";
    public const string ChainId = "fixture-btc-ots";
    public const string TransactionReference = "fixture:not-a-bitcoin-txid:rideaudit-btc-ots-fixture-001";
    public const long BlockHeight = 9_000_000_001;
    public const long WriteTimeUnixMillis = 1_759_010_000_000;
    public const string Disclaimer =
        "SYNTHETIC TEST FIXTURE. Not a live Bitcoin transaction. Not broadcast. Not a real txid. " +
        "block_height 9000000001 is intentionally outside Bitcoin height space.";

    public FixtureCalendarMode Mode { get; set; } = FixtureCalendarMode.UpgradeImmediately;
    public int SubmitCalls { get; private set; }

    public (string Status, byte[] Proof) Submit(byte[] receiptCoreDigest)
    {
        SubmitCalls++;
        if (Mode == FixtureCalendarMode.FailSubmit)
            throw new RideAuditException(ErrorCodes.ChainFailed, "Documented fixture calendar refused the digest submission.");

        var status = Mode == FixtureCalendarMode.UpgradeImmediately ? "upgraded" : "pending";
        if (Mode == FixtureCalendarMode.FailUpgrade)
            status = "failed";
        return (status, BuildProof(receiptCoreDigest, status));
    }

    public static byte[] BuildProof(byte[] digest, string status)
    {
        var text = "RIDEOTS-FIXTURE-1\n" + FixtureId + "\n" + status + "\n" + Ids.Hex(digest) + "\n" + Disclaimer + "\n";
        return Encoding.UTF8.GetBytes(text);
    }
}

public sealed class UnconfiguredOtsCalendar
{
    public AnchorAttempt Fail(AnchorRequest request)
    {
        var envelope = new AnchorProofEnvelope
        {
            ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
            ProfileId = ChainProfileIds.BtcOts,
            ProofSource = "unconfigured",
            Status = "failed",
            Disclaimer = "No OpenTimestamps calendar is configured. Confirmation metadata is intentionally absent.",
            FailureCode = ErrorCodes.ChainFailed,
            LiveBitcoinMetadata = false
        };
        return new AnchorAttempt(false, CustodyState.Quarantined, envelope, ErrorCodes.ChainFailed,
            "Bitcoin OpenTimestamps calendar is not configured. Record is not admitted.", TimeSpan.Zero);
    }
}

public sealed class BtcOtsAnchor : IChainAnchor
{
    private readonly DocumentedFixtureOtsCalendar? _fixture;
    private readonly UnconfiguredOtsCalendar _unconfigured = new();

    public BtcOtsAnchor(DocumentedFixtureOtsCalendar? fixture) => _fixture = fixture;

    public string ProfileId => ChainProfileIds.BtcOts;

    public AnchorAttempt Anchor(AnchorRequest request)
    {
        if (_fixture is null)
            return _unconfigured.Fail(request);

        try
        {
            var (status, proof) = _fixture.Submit(request.ReceiptCoreDigest);
            var envelope = new AnchorProofEnvelope
            {
                ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
                ProfileId = ChainProfileIds.BtcOts,
                ProofSource = "documented-fixture",
                Status = status,
                ProofBytes = Google.Protobuf.ByteString.CopyFrom(proof),
                Disclaimer = DocumentedFixtureOtsCalendar.Disclaimer,
                FailureCode = status == "upgraded" ? "" : ErrorCodes.ChainUnconfirmed,
                LiveBitcoinMetadata = false
            };

            if (status == "upgraded")
            {
                envelope.ChainId = DocumentedFixtureOtsCalendar.ChainId;
                envelope.TransactionReference = DocumentedFixtureOtsCalendar.TransactionReference;
                envelope.BlockHeight = DocumentedFixtureOtsCalendar.BlockHeight;
                envelope.WriteTimeUnixMillis = DocumentedFixtureOtsCalendar.WriteTimeUnixMillis;
                return new AnchorAttempt(true, CustodyState.Confirmed, envelope, null,
                    "Fixture OTS upgrade recorded. This is not a live Bitcoin transaction.", TimeSpan.Zero);
            }

            var code = status == "failed" ? ErrorCodes.ChainFailed : ErrorCodes.ChainUnconfirmed;
            return new AnchorAttempt(false, CustodyState.Quarantined, envelope, code,
                "OTS proof is not upgraded. Pending or failed proofs do not admit the record.", TimeSpan.Zero);
        }
        catch (RideAuditException ex)
        {
            var envelope = new AnchorProofEnvelope
            {
                ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(request.ReceiptCoreDigest),
                ProfileId = ChainProfileIds.BtcOts,
                ProofSource = "documented-fixture",
                Status = "failed",
                Disclaimer = DocumentedFixtureOtsCalendar.Disclaimer,
                FailureCode = ex.Code,
                LiveBitcoinMetadata = false
            };
            return new AnchorAttempt(false, CustodyState.Quarantined, envelope, ex.Code, ex.Message, TimeSpan.Zero);
        }
    }
}
