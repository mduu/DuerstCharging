using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Scheduling;

namespace DuerstCharging.Core;

public class ChargingManager(
    Schedule schedule,
    ChargingNetwork chargingNetwork,
    bool simulateOnly)
{
    private bool lastWasProhibited;

    public async Task ScanAndPrint()
    {
        foreach (var chargingStation in await chargingNetwork.GetAllChargingStations())
        {
            Console.WriteLine($"- Charging-Station {chargingStation}: ChargingState={chargingStation.ChargingState}, Cable-State={chargingStation.CableState}, Error-Code={chargingStation.ErrorCode}, IsEnabled={chargingStation.IsEnabled}");
        }
    }

    public async Task StartUp(CancellationToken cancellationToken)
    {
        lastWasProhibited = schedule.GetIsChargingProhibited();

        await UpdateIfNeeded(cancellationToken, true);
    }

    public async Task UpdateIfNeeded(CancellationToken cancellationToken, bool forceUpdate = false)
    {
        var isProhibited = schedule.GetIsChargingProhibited();
        if (forceUpdate || lastWasProhibited != isProhibited)
        {
            Console.WriteLine($"{DateTimeOffset.Now:G}: Prohibited change to {isProhibited}");
            await UpdateChargingStations(isProhibited, cancellationToken);
        }
    }

    public async Task Shutdown()
    {
        foreach (var chargingStation in await chargingNetwork.GetAllChargingStations())
        {
            await chargingStation.SetEnabled(true, simulateOnly, CancellationToken.None);
        }
    }

    private async Task UpdateChargingStations(bool isProhibited, CancellationToken cancellationToken)
    {
        foreach (var chargingStation in await chargingNetwork.GetAllChargingStations())
        {
            await chargingStation.SetEnabled(!isProhibited, simulateOnly, cancellationToken);
        }

        lastWasProhibited = isProhibited;
    }
}