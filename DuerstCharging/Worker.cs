using DuerstCharging.Core;

namespace DuerstCharging;

public class Worker(
    ILogger<Worker> logger,
    IChargingManager chargingManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await chargingManager.ScanAndPrint();

            var cancellationToken = new CancellationToken(false);
            await chargingManager.StartUp(cancellationToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await chargingManager.UpdateIfNeeded(cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error while executing");
        }
        finally
        {
            logger.LogInformation("Shutting down charging-manager ...");
            await chargingManager.Shutdown();
            logger.LogInformation("Worker is shut down");
        }
    }
}