using DuerstCharging.Core;
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

var chargingManager = new ChargingManager(schedule,
    new ChargingNetwork(),
    true);

try
{
    await chargingManager.ScanAndPrint();

    var cancellationToken = new CancellationToken(false);
    await chargingManager.StartUp(cancellationToken);

    while (true)
    {
        await chargingManager.UpdateIfNeeded(cancellationToken);
        await Task.Delay(1000, cancellationToken);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error! Exception: {ex.Message}");
}
finally
{
    Console.WriteLine("Shutting down ...");
    await chargingManager.Shutdown();
}

Console.WriteLine("Done.");