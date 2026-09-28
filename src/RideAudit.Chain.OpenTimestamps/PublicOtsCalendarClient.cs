// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Net.Http;
using RideAudit.Contracts;

namespace RideAudit.Chain.OpenTimestamps;

/// <summary>
/// HTTP client for a public OpenTimestamps calendar. A successful POST stores the
/// calendar body as pending proof bytes. This type never invents a Bitcoin txid,
/// block height, or upgraded attestation. live_bitcoin_metadata stays false.
/// </summary>
public sealed class PublicOtsCalendarClient : IOtsCalendar, IDisposable
{
    public const string AliceCalendar = "https://alice.btc.calendar.opentimestamps.org";
    public const string Disclaimer =
        "OpenTimestamps calendar response. Pending proof only. Not a Bitcoin transaction. " +
        "No transaction_reference or block_height was written. live_bitcoin_metadata is false.";

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly Uri _digestUri;

    public PublicOtsCalendarClient(string calendarUrl, HttpMessageHandler? handler = null, TimeSpan? timeout = null)
    {
        if (!CalendarEndpoints.IsHttpsUrl(calendarUrl))
            throw new RideAuditException(ErrorCodes.ChainFailed, "OpenTimestamps calendar URL must be https.");

        var root = calendarUrl.TrimEnd('/');
        _digestUri = new Uri(root + "/digest");
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

    public string ProofSource => ProofSources.OpenTimestampsCalendar;

    public string CalendarUrl => _digestUri.GetLeftPart(UriPartial.Authority);

    public OtsSubmitResult Submit(byte[] receiptCoreDigest)
    {
        if (receiptCoreDigest.Length != 32)
            throw new RideAuditException(ErrorCodes.ChainFailed, "OpenTimestamps digest must be 32 bytes.");

        try
        {
            using var content = new ByteArrayContent(receiptCoreDigest);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            using var response = _http.PostAsync(_digestUri, content).GetAwaiter().GetResult();
            var body = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return new OtsSubmitResult(
                    "failed",
                    body.Length == 0 ? Array.Empty<byte>() : body,
                    "Calendar returned HTTP " + (int)response.StatusCode + ". No transaction metadata was written.");
            }

            if (body.Length == 0)
            {
                return new OtsSubmitResult(
                    "failed",
                    Array.Empty<byte>(),
                    "Calendar returned an empty body. No transaction metadata was written.");
            }

            var labeled = LabelPending(receiptCoreDigest, body);
            return new OtsSubmitResult("pending", labeled, Disclaimer);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            return new OtsSubmitResult(
                "failed",
                Array.Empty<byte>(),
                "Calendar request failed: " + ex.GetType().Name + ". No transaction metadata was written.");
        }
    }

    internal static byte[] LabelPending(byte[] digest, byte[] calendarBody)
    {
        var header = System.Text.Encoding.UTF8.GetBytes(
            "RIDEOTS-PENDING-1\n" + Ids.Hex(digest) + "\n" + Disclaimer + "\n");
        var labeled = new byte[header.Length + calendarBody.Length];
        header.CopyTo(labeled, 0);
        calendarBody.CopyTo(labeled, header.Length);
        return labeled;
    }

    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }
}
