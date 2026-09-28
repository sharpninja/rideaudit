// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using RideAudit.Contracts;

namespace RideAudit.Chain.EthL2;

/// <summary>
/// Ethereum JSON-RPC client. Probe can read eth_blockNumber when an https RPC is configured.
/// Commit refuses unless a recorded handler supplies a transaction hash. This type never
/// invents a hash, never signs a transaction, and never claims a live broadcast.
/// </summary>
public sealed class PublicEthL2RpcClient : IEthL2Client, IDisposable
{
    public const string Disclaimer =
        "No L2 signer is configured. Refusing to invent a transaction id. " +
        "eth_blockNumber probes do not admit a receipt.";

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly Uri _rpc;

    public PublicEthL2RpcClient(string rpcUrl, HttpMessageHandler? handler = null, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(rpcUrl)
            || !(rpcUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                 || rpcUrl.StartsWith("http://127.0.0.1", StringComparison.Ordinal)))
        {
            throw new RideAuditException(ErrorCodes.ChainFailed, "L2 RPC URL must be https or loopback http.");
        }

        _rpc = new Uri(rpcUrl);
        if (handler is null)
        {
            _http = new HttpClient { Timeout = timeout ?? TimeSpan.FromSeconds(10) };
            _ownsHttp = true;
        }
        else
        {
            _http = new HttpClient(handler, disposeHandler: false) { Timeout = timeout ?? TimeSpan.FromSeconds(10) };
            _ownsHttp = false;
        }
    }

    public string ProofSource => ProofSources.EthL2Rpc;

    public L2CommitResult Commit(string profileId, byte[] digest)
    {
        return new L2CommitResult(
            false,
            "failed",
            Encoding.UTF8.GetBytes("RIDEL2-NO-SIGNER-1\n" + Ids.Hex(digest) + "\n" + Disclaimer + "\n"),
            Disclaimer,
            ErrorCodes.ChainFailed,
            null,
            null,
            null);
    }

    public L2RpcProbe ProbeBlockNumber()
    {
        try
        {
            var payload = "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"eth_blockNumber\",\"params\":[]}";
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = _http.PostAsync(_rpc, content).GetAwaiter().GetResult();
            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
                return L2RpcProbe.Fail("RPC HTTP " + (int)response.StatusCode);

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.String)
                return L2RpcProbe.Fail("RPC result is missing.");

            var hex = result.GetString() ?? "";
            if (!hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return L2RpcProbe.Fail("RPC result is not a hex block number.");

            if (!ulong.TryParse(hex.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var height))
                return L2RpcProbe.Fail("RPC block number is not parseable.");

            return new L2RpcProbe(true, height, hex, "eth_blockNumber from configured RPC. Not a transaction receipt.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or OperationCanceledException)
        {
            return L2RpcProbe.Fail(ex.GetType().Name);
        }
    }

    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }
}

public sealed record L2RpcProbe(bool Ok, ulong? BlockNumber, string? Hex, string Detail)
{
    public static L2RpcProbe Fail(string detail) => new(false, null, null, detail);
}

/// <summary>
/// Hermetic double. Copies a transaction hash from a recorded JSON response. Labeled non-live.
/// </summary>
public sealed class RecordedEthL2CommitClient : IEthL2Client
{
    private readonly L2CommitResult _result;

    public RecordedEthL2CommitClient(L2CommitResult result) => _result = result;

    public string ProofSource => ProofSources.EthL2Rpc;

    public L2CommitResult Commit(string profileId, byte[] digest) => _result;
}
