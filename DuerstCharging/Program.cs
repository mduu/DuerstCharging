using System.Collections.Immutable;
using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Scheduling;

Console.WriteLine("Scanning charging network ...");

var schedule = new Schedule(TimeProvider.System)
{
    ChargingProhibited = new List<ScheduleEntry>
    {
        new(DayOfWeek.Monday, new TimeOnly(7, 0), new TimeOnly(20, 0)),
        new(DayOfWeek.Tuesday, new TimeOnly(7, 0), new TimeOnly(20, 0)),
        new(DayOfWeek.Wednesday, new TimeOnly(7, 0), new TimeOnly(20, 0)),
        new(DayOfWeek.Thursday, new TimeOnly(7, 0), new TimeOnly(20, 0)),
        new(DayOfWeek.Friday, new TimeOnly(7, 0), new TimeOnly(20, 0)),
        new(DayOfWeek.Saturday, new TimeOnly(7, 0), new TimeOnly(13, 0)),
    }
};

var chargingNetwork = new ChargingNetwork();

var chargingStations =
    (await chargingNetwork.GetAllChargingStations())
    .ToImmutableArray();

Console.WriteLine($"Found {chargingStations.Length} charging stations.");

foreach (var chargingStation in chargingStations)
{
    Console.WriteLine($"- Charging-Station {chargingStation}: ChargingState={chargingStation.ChargingState}, Cable-State={chargingStation.CableState}, Error-Code={chargingStation.ErrorCode}");
}

Console.WriteLine("Done.");