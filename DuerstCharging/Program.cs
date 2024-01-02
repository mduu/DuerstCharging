using System.Collections.Immutable;
using DuerstCharging.Core;

Console.WriteLine("Scanning charging network ...");

var chargingNetwork = new ChargingNetwork();
var chargingStations = 
    (await chargingNetwork.GetAllChargingStations())
    .ToImmutableArray();

Console.WriteLine($"Found {chargingStations.Length} charging stations.");

foreach (var chargingStation in chargingStations)
{
    Console.WriteLine($"Charging-Station {chargingStation}: ChargingState={chargingStation.ChargingState}, Cable-State={chargingStation.CableState}, Error-Code={chargingStation.ErrorCode}");
}

Console.WriteLine("Done.");
