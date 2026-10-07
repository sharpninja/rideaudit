using System.Net;
using System.Net.Http;
using System.Text;
using RideAudit.Chain;
using RideAudit.Chain.EthL2;
using RideAudit.Chain.OpenTimestamps;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;
using RideAudit.TestSupport;

namespace RideAudit.Chain.Tests;

/// <summary>
/// Live calendar/RPC clients. Hermetic handlers are labeled non-live.
/// These tests never invent a Bitcoin or L2 transaction id.
/// </summary>
public class LiveChainClientTests
{
    [Fact]
    public void Public_ots_client_records_pending_proof_without_a_txid()
    {
        var digest = Ids.Sha256(Encoding.UTF8.GetBytes("ots-hermetic"));
        using var handler = new ScriptedHandler(HttpStatusCode.OK, new byte[] { 0x01, 0x02, 0x03, 0x04 });
        using var client = new PublicOtsCalendarClient(PublicOtsCalendarClient.AliceCalendar, handler);
        var result = client.Submit(digest);
        Assert.Equal(ProofSources.OpenTimestampsCalendar, client.ProofSource);
        Assert.Equal("pending", result.Status);
        Assert.Null(result.TransactionReference);
        Assert.Null(result.ChainId);
        Assert.Null(result.BlockHeight);
        Assert.Contains(Ids.Hex(digest), Encoding.UTF8.GetString(result.ProofBytes), StringComparison.Ordinal);
        Assert.Contains("Not a Bitcoin transaction", result.Disclaimer, StringComparison.Ordinal);

        var anchor = new BtcOtsAnchor(client).Anchor(new AnchorRequest(digest, digest, RideAuditPolicy.Version, ChainProfileIds.BtcOts));
        Assert.False(anchor.Confirmed);
        Assert.Equal(ErrorCodes.ChainUnconfirmed, anchor.FailureCode);
        Assert.False(anchor.Envelope.HasTransactionReference);
        Assert.False(anchor.Envelope.HasBlockHeight);
        Assert.False(anchor.Envelope.LiveBitcoinMetadata);
    }

    [Fact]
    public void Public_ots_client_fail_closes_on_http_error_without_inventing_metadata()
    {
        var digest = Ids.Sha256(Encoding.UTF8.GetBytes("ots-fail"));
        using var handler = new ScriptedHandler(HttpStatusCode.BadGateway, Array.Empty<byte>());
        using var client = new PublicOtsCalendarClient(PublicOtsCalendarClient.AliceCalendar, handler);
        var result = client.Submit(digest);
        Assert.Equal("failed", result.Status);
        Assert.Null(result.TransactionReference);
    }

    [Fact]
    [Trait("UC", "UC-RIDE-009")]
    [Trait("AC", "AC-UC-009-003")]
    public void Public_ots_submit_is_pending_only_and_does_not_confirm_until_upgrade()
    {
        var digest = Ids.Sha256(Encoding.UTF8.GetBytes("uc-009-pending-only"));
        var request = new AnchorRequest(digest, digest, RideAuditPolicy.Version, ChainProfileIds.BtcOts);
        using var handler = new ScriptedHandler(HttpStatusCode.OK, new byte[] { 0x0a, 0x0b, 0x0c });
        using var client = new PublicOtsCalendarClient(PublicOtsCalendarClient.AliceCalendar, handler);

        // A public calendar submit is pending only: no txid, chain id, block height, or write time.
        var submit = client.Submit(digest);
        Assert.Equal("pending", submit.Status);
        Assert.Null(submit.TransactionReference);
        Assert.Null(submit.ChainId);
        Assert.Null(submit.BlockHeight);
        Assert.Null(submit.WriteTimeUnixMillis);
        Assert.Contains("RIDEOTS-PENDING-1", Encoding.UTF8.GetString(submit.ProofBytes), StringComparison.Ordinal);

        // A pending proof is not the happy-path confirmation. Custody is quarantined, so admission stays open.
        var pending = new BtcOtsAnchor(client).Anchor(request);
        Assert.False(pending.Confirmed);
        Assert.Equal(RideAudit.Contracts.CustodyState.Quarantined, pending.State);
        Assert.Equal(ErrorCodes.ChainUnconfirmed, pending.FailureCode);
        Assert.Equal("pending", pending.Envelope.Status);
        Assert.False(pending.Envelope.HasTransactionReference);
        Assert.False(pending.Envelope.HasBlockHeight);
        Assert.False(pending.Envelope.LiveBitcoinMetadata);

        // Confirmation follows only an upgrade. The documented fixture stands in; it is not a live txid.
        var upgraded = new BtcOtsAnchor(new DocumentedFixtureOtsCalendar { Mode = FixtureCalendarMode.UpgradeImmediately }).Anchor(request);
        Assert.True(upgraded.Confirmed);
        Assert.Equal(RideAudit.Contracts.CustodyState.Confirmed, upgraded.State);
        Assert.StartsWith("fixture:", upgraded.Envelope.TransactionReference, StringComparison.Ordinal);
        Assert.False(upgraded.Envelope.LiveBitcoinMetadata);
    }

    [Fact]
    public void Public_l2_client_refuses_to_invent_a_transaction_id()
    {
        var digest = Ids.Sha256(Encoding.UTF8.GetBytes("l2-nosigner"));
        using var handler = new ScriptedHandler(HttpStatusCode.OK, Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":\"0x10\"}"));
        using var client = new PublicEthL2RpcClient("https://example.invalid/rpc", handler);
        var probe = client.ProbeBlockNumber();
        Assert.True(probe.Ok);
        Assert.Equal(16UL, probe.BlockNumber);

        var commit = client.Commit(ChainProfileIds.EthL2Base, digest);
        Assert.False(commit.Confirmed);
        Assert.Null(commit.TransactionReference);
        Assert.Equal(ErrorCodes.ChainFailed, commit.FailureCode);

        var attempt = new EthL2Anchor(ChainProfileIds.EthL2Base, client).Anchor(
            new AnchorRequest(digest, digest, RideAuditPolicy.Version, ChainProfileIds.EthL2Base));
        Assert.False(attempt.Confirmed);
        Assert.False(attempt.Envelope.HasTransactionReference);
    }

    [Fact]
    public void Verifier_accepts_recorded_l2_hash_and_rejects_fixture_scoped_live_source()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var digest = Ids.Sha256(ready.Package.ReceiptCoreBytes);
        var recorded = "0x" + new string('a', 64);
        var envelope = new AnchorProofEnvelope
        {
            ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(digest),
            ProfileId = ChainProfileIds.EthL2Base,
            ProofSource = ProofSources.EthL2Rpc,
            Status = "upgraded",
            ChainId = "base-sepolia-recorded",
            TransactionReference = recorded,
            BlockHeight = 12,
            WriteTimeUnixMillis = 1_759_010_000_000,
            ProofBytes = Google.Protobuf.ByteString.CopyFromUtf8("RIDEL2-RECORDED-1\n" + Ids.Hex(digest) + "\nNON-LIVE recorded RPC fixture\n"),
            Disclaimer = "NON-LIVE recorded JSON-RPC response. Not a broadcast RideAudit constructed.",
            FailureCode = "",
            LiveBitcoinMetadata = false
        };
        var good = AnchorProofVerifier.Verify(ready.Package.ReceiptCoreBytes, ready.Package.Ciphertext, envelope);
        Assert.True(good.Confirmed);

        envelope.TransactionReference = "fixture:not-an-ethereum-txid:x";
        var bad = AnchorProofVerifier.Verify(ready.Package.ReceiptCoreBytes, ready.Package.Ciphertext, envelope);
        Assert.False(bad.Confirmed);
    }

    [Fact]
    public void Production_options_refuse_fixture_but_accept_https_calendar_setting()
    {
        var fixture = new RideAudit.Server.Admission.AdmissionServerOptions
        {
            EnvironmentName = "Production",
            UseFixtureCalendar = true,
            EdgeTerminatesTls = true
        };
        Assert.Throws<InvalidOperationException>(() => fixture.Validate());

        var live = new RideAudit.Server.Admission.AdmissionServerOptions
        {
            EnvironmentName = "Production",
            UseFixtureCalendar = false,
            OtsCalendarSetting = PublicOtsCalendarClient.AliceCalendar,
            EdgeTerminatesTls = true
        };
        live.Validate();
        var calendar = RideAudit.Server.Admission.ChainEndpointFactory.CreateOts(live);
        Assert.IsType<PublicOtsCalendarClient>(calendar);
    }

    [Fact]
    public async Task Public_alice_calendar_probe_does_not_write_a_txid()
    {
        var digest = Ids.Sha256(Encoding.UTF8.GetBytes("rideaudit-legion2-ots-probe"));
        using var client = new PublicOtsCalendarClient(PublicOtsCalendarClient.AliceCalendar, timeout: TimeSpan.FromSeconds(8));
        var result = await Task.Run(() => client.Submit(digest));
        Assert.Null(result.TransactionReference);
        Assert.Null(result.BlockHeight);
        Assert.False(string.Equals(result.Status, "upgraded", StringComparison.Ordinal));
        if (result.Status == "pending")
            Assert.Contains("RIDEOTS-PENDING-1", Encoding.UTF8.GetString(result.ProofBytes), StringComparison.Ordinal);
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly byte[] _body;

        public ScriptedHandler(HttpStatusCode status, byte[] body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_status)
            {
                Content = new ByteArrayContent(_body)
            };
            return Task.FromResult(response);
        }
    }
}
