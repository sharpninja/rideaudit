using RideAudit.Chain.EthL2;
using RideAudit.Chain.OpenTimestamps;

namespace RideAudit.Server.Admission;

public static class ChainEndpointFactory
{
    public static IOtsCalendar? CreateOts(AdmissionServerOptions options)
    {
        if (options.UseFixtureCalendar)
            return new DocumentedFixtureOtsCalendar();
        if (CalendarEndpoints.IsHttpsUrl(options.OtsCalendarSetting))
            return new PublicOtsCalendarClient(options.OtsCalendarSetting!);
        return null;
    }

    public static IEthL2Client? CreateL2(AdmissionServerOptions options)
    {
        if (options.UseFixtureL2)
            return new DocumentedFixtureL2Calendar();
        var rpc = options.L2RpcSetting ?? options.L2CalendarSetting;
        if (CalendarEndpoints.IsHttpsUrl(rpc))
            return new PublicEthL2RpcClient(rpc!);
        return null;
    }
}
