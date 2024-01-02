using System.Collections.Immutable;
using System.Net;

namespace DuerstCharging.Core;

public class ChargingNetwork
{
    public async Task<ImmutableArray<ChargingStation>> GetAllChargingStations()
    {
        // TODO Currently hardcoded list of charging-stations. Make it dynamic by broadcasting to the network.
        var result = new[]
        {
            await ChargingStation.Create(IPAddress.Parse("192.168.1.12"))
        }.ToImmutableArray();
        
        return result;
    }
}