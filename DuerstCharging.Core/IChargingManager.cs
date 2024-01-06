namespace DuerstCharging.Core;

public interface IChargingManager
{
    Task ScanAndPrint();
    Task StartUp(CancellationToken cancellationToken);
    Task UpdateIfNeeded(CancellationToken cancellationToken, bool forceUpdate = false);
    Task Shutdown();
}