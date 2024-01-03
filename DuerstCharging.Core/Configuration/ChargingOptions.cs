using DuerstCharging.Core.Scheduling;

namespace DuerstCharging.Core.Configuration;

public class ChargingOptions
{
    public bool SimulationOnly { get; set; }
    public string? ChargingStationIpAddress { get; set; }

    public ScheduleEntry[] ChargingProhibited { get; set; } = Array.Empty<ScheduleEntry>();
}