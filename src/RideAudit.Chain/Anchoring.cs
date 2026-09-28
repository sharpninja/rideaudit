using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Chain;

public sealed record AnchorRequest(
    byte[] ReceiptCoreBytes,
    byte[] ReceiptCoreDigest,
    string PolicyVersion,
    string ConnectorId);

public sealed record AnchorAttempt(
    bool Confirmed,
    CustodyState State,
    AnchorProofEnvelope Envelope,
    string? FailureCode,
    string? OperatorMessage,
    TimeSpan ObservedLatency);

public interface IChainAnchor
{
    string ProfileId { get; }
    AnchorAttempt Anchor(AnchorRequest request);
}

public interface IElapsedTimer
{
    TimeSpan Measure(string phase, Action action);
}

public sealed class StopwatchTimer : IElapsedTimer
{
    public TimeSpan Measure(string phase, Action action)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        action();
        watch.Stop();
        return watch.Elapsed;
    }
}

public sealed class ScriptedTimer : IElapsedTimer
{
    public TimeSpan Next { get; set; } = TimeSpan.FromMilliseconds(1);

    public TimeSpan Measure(string phase, Action action)
    {
        action();
        return Next;
    }
}

public sealed record LatencySample(string Connector, string Phase, TimeSpan Elapsed, TimeSpan Budget, bool Breached);

public sealed class LatencyMonitor
{
    private readonly List<LatencySample> _samples = new();

    public IReadOnlyList<LatencySample> Samples => _samples;

    public void Observe(string connector, string phase, TimeSpan elapsed, TimeSpan budget)
    {
        _samples.Add(new LatencySample(connector, phase, elapsed, budget, elapsed > budget));
    }
}

public sealed class AnchoringPolicy
{
    private readonly LatencyMonitor _monitor;
    private readonly IElapsedTimer _timer;
    private readonly TimeSpan _chainBudget;

    public AnchoringPolicy(LatencyMonitor monitor, IElapsedTimer timer, TimeSpan chainBudget)
    {
        _monitor = monitor;
        _timer = timer;
        _chainBudget = chainBudget;
    }

    public AnchorAttempt Anchor(IChainAnchor anchor, AnchorRequest request)
    {
        AnchorAttempt? attempt = null;
        var elapsed = _timer.Measure("chain-confirmation", () => attempt = anchor.Anchor(request));
        _monitor.Observe(request.ConnectorId, "chain-confirmation", elapsed, _chainBudget);
        if (attempt is null)
            throw new RideAuditException(ErrorCodes.ChainFailed, "Chain anchor produced no result.");

        if (elapsed > _chainBudget)
        {
            return attempt with
            {
                Confirmed = false,
                State = CustodyState.Quarantined,
                FailureCode = ErrorCodes.LatencyBudgetExceeded,
                OperatorMessage = "Seal/receipt latency budget exceeded. Admission remains blocked.",
                ObservedLatency = elapsed
            };
        }

        return attempt with { ObservedLatency = elapsed };
    }
}

public sealed record AnchorVerification(
    bool Confirmed,
    bool PayloadHashMatch,
    bool DigestMatch,
    bool LiveBitcoinMetadata,
    IReadOnlyList<string> Mismatches);

/// <summary>
/// Independent check of receipt core, ciphertext hash, and anchor envelope.
/// Pending proofs and fixture values presented as live Bitcoin fail closed.
/// </summary>
public static class AnchorProofVerifier
{
    public static AnchorVerification Verify(byte[] receiptCoreBytes, byte[] ciphertext, AnchorProofEnvelope envelope)
    {
        var mismatches = new List<string>();
        var digest = Ids.Sha256(receiptCoreBytes);
        var digestMatch = digest.AsSpan().SequenceEqual(envelope.ReceiptCoreDigest.Span);
        if (!digestMatch)
            mismatches.Add("Anchor digest does not match the receipt core.");

        ReceiptCore core;
        try
        {
            core = ReceiptCore.Parser.ParseFrom(receiptCoreBytes);
        }
        catch (Google.Protobuf.InvalidProtocolBufferException)
        {
            mismatches.Add("Receipt core is not parseable.");
            return new AnchorVerification(false, false, false, false, mismatches);
        }

        var contentHash = Ids.Sha256(ciphertext);
        var payloadMatch = contentHash.AsSpan().SequenceEqual(core.ContentHash.Span);
        if (!payloadMatch)
            mismatches.Add("Recomputed sealed-payload hash does not match the receipt.");

        if (!string.Equals(envelope.Status, "upgraded", StringComparison.Ordinal))
            mismatches.Add("Anchor is not upgraded. A pending proof is not confirmation.");

        if (!envelope.HasChainId || string.IsNullOrWhiteSpace(envelope.ChainId))
            mismatches.Add("Chain id is missing.");
        if (!envelope.HasTransactionReference || string.IsNullOrWhiteSpace(envelope.TransactionReference))
            mismatches.Add("Transaction reference is missing.");
        if (!envelope.HasBlockHeight)
            mismatches.Add("Block height is missing.");
        if (!envelope.HasWriteTimeUnixMillis || envelope.WriteTimeUnixMillis <= 0)
            mismatches.Add("Write time is missing.");
        if (!envelope.HasLiveBitcoinMetadata)
            mismatches.Add("Live-Bitcoin metadata flag is absent.");

        var live = envelope.HasLiveBitcoinMetadata && envelope.LiveBitcoinMetadata;
        if (live)
            mismatches.Add("Live Bitcoin metadata was claimed without a verified OpenTimestamps upgrade.");

        if (string.Equals(envelope.ProofSource, "documented-fixture", StringComparison.Ordinal))
        {
            if (!string.Equals(envelope.ChainId, "fixture-btc-ots", StringComparison.Ordinal))
                mismatches.Add("Fixture proof chain id is not fixture-btc-ots.");
            var tx = envelope.TransactionReference ?? "";
            if (!tx.StartsWith("fixture:", StringComparison.Ordinal))
                mismatches.Add("Fixture transaction reference is not fixture-scoped.");
            if (System.Text.RegularExpressions.Regex.IsMatch(tx, "^[0-9a-fA-F]{64}$"))
                mismatches.Add("Fixture transaction reference looks like a raw Bitcoin txid.");
            var proof = envelope.ProofBytes.ToStringUtf8();
            if (!proof.Contains("RIDEOTS-FIXTURE-1", StringComparison.Ordinal)
                || !proof.Contains(Ids.Hex(digest), StringComparison.Ordinal))
                mismatches.Add("Fixture proof bytes do not commit to the receipt digest.");
            if (string.IsNullOrWhiteSpace(envelope.Disclaimer))
                mismatches.Add("Fixture disclaimer is missing.");
        }
        else if (!string.Equals(envelope.Status, "failed", StringComparison.Ordinal))
        {
            mismatches.Add("Unsupported proof source.");
        }

        var confirmed = mismatches.Count == 0 && string.Equals(envelope.Status, "upgraded", StringComparison.Ordinal);
        return new AnchorVerification(confirmed, payloadMatch, digestMatch, live, mismatches);
    }
}
