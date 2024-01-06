using System.Net;

namespace DuerstCharging.Core.Charging;

public interface IChargingStation
{
    IPAddress IpAddress { get; }
    ChargingState ChargingState { get; }
    CableState CableState { get; }
    uint ErrorCode { get; }
    bool IsEnabled { get; }

    Task RetrieveInformation();
    Task SetEnabled(bool isEnabled, bool simulateOnly, CancellationToken cancellationToken);
}