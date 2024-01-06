using System.Collections.Immutable;
using System.Net;
using DuerstCharging.Core.Configuration;
using Microsoft.Extensions.Options;

namespace DuerstCharging.Core.Charging;

public class ChargingNetwork(
    IOptionsMonitor<ChargingOptions> options,
    IServiceProvider serviceProvider) : IChargingNetwork
{
    public async Task<ImmutableArray<ChargingStation>> GetAllChargingStations()
    {
        if (options.CurrentValue.ChargingStationIpAddress is null)
        {
            throw new InvalidOperationException("CurrentValue.ChargingStationIpAddress is not configured properly but needed!");
        }

        // TODO Currently hardcoded list of charging-stations. Make it dynamic by broadcasting to the network.
        var chargingStation = await ChargingStation.Create(
            serviceProvider,
            IPAddress.Parse(
                options.CurrentValue.ChargingStationIpAddress));

        return new[] { chargingStation }.ToImmutableArray();
    }
}