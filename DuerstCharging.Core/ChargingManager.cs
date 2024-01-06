using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Configuration;
using DuerstCharging.Core.Scheduling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DuerstCharging.Core;

public class ChargingManager(
    ILogger<ChargingManager> logger,
    IOptionsMonitor<ChargingOptions> options,
    Schedule schedule,
    IChargingNetwork chargingNetwork) : IChargingManager
{
    private bool lastWasProhibited;

    public async Task ScanAndPrint()
    {
        foreach (var chargingStation in await chargingNetwork.GetAllChargingStations())
        {
            logger.LogInformation(
                "- Charging-Station {ChargingStation}: ChargingState={ChargingState}, Cable-State={CableState}, Error-Code={ErrorCode}, IsEnabled={IsEnabled}",
                chargingStation,
                chargingStation.ChargingState,
                chargingStation.CableState,
                chargingStation.ErrorCode,
                chargingStation.IsEnabled);
        }
    }

    public async Task StartUp(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting {ClassName}...", nameof(ChargingManager));
        lastWasProhibited = schedule.GetIsChargingProhibited();

        await UpdateIfNeeded(cancellationToken, true);
        logger.LogInformation("{ClassName} started", nameof(ChargingManager));
    }

    public async Task UpdateIfNeeded(CancellationToken cancellationToken, bool forceUpdate = false)
    {
        var isProhibited = schedule.GetIsChargingProhibited();
        if (forceUpdate || lastWasProhibited != isProhibited)
        {
            logger.LogInformation(
                "Charging is {EnabledDisabled}",
                isProhibited
                    ? "disabled"
                    : "enabled");

            await UpdateChargingStations(isProhibited, cancellationToken);
        }
    }

    public async Task Shutdown()
    {
        logger.LogInformation("Shutting down {ClassName}...", nameof(ChargingManager));

        foreach (var chargingStation in await chargingNetwork.GetAllChargingStations())
        {
            await chargingStation.SetEnabled(true, options.CurrentValue.SimulationOnly, CancellationToken.None);
        }

        logger.LogInformation("{ClassName} shut down", nameof(ChargingManager));
    }

    private async Task UpdateChargingStations(bool isProhibited, CancellationToken cancellationToken)
    {
        var enabledOrDisabled = isProhibited ? "disabled" : "enabled";

        logger.LogInformation(
            "Update charging stations to charging is {EnabledDisabled}",
            enabledOrDisabled);

        foreach (var chargingStation in await chargingNetwork.GetAllChargingStations())
        {
            logger.LogInformation(
                "Set enabled state for charging station {ChargingStation}",
                chargingStation);

            await chargingStation.SetEnabled(!isProhibited, options.CurrentValue.SimulationOnly, cancellationToken);
        }

        lastWasProhibited = isProhibited;

        logger.LogInformation(
            "All charging stations set to {EnabledDisabled}",
            enabledOrDisabled);
    }
}