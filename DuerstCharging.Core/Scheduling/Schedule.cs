using DuerstCharging.Core.Configuration;
using Microsoft.Extensions.Options;

namespace DuerstCharging.Core.Scheduling;

public class Schedule(
    TimeProvider timeProvider,
    IOptionsMonitor<ChargingOptions> options)
{
    public bool GetIsChargingProhibited()
    {
        var currentTime = timeProvider.GetLocalNow();
        var currentDayOfWeek = currentTime.DayOfWeek;
        var currentTimeOnly = TimeOnly.FromTimeSpan(currentTime.TimeOfDay);

        return options.CurrentValue.ChargingProhibited
            .Any(entry =>
                currentDayOfWeek == entry.Weekday &&
                currentTimeOnly >= entry.StartTime &&
                currentTimeOnly <= entry.EndTime);
    }
}