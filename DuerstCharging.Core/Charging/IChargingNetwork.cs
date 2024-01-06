using System.Collections.Immutable;

namespace DuerstCharging.Core.Charging;

public interface IChargingNetwork
{
    Task<ImmutableArray<ChargingStation>> GetAllChargingStations();
}