namespace DuerstCharging.Core.Scheduling;

public class Schedule(TimeProvider timeProvider)
{
    public IEnumerable<ScheduleEntry> ChargingProhibited { get; set; }

    public bool GetIsChargingProhibited()
    {
        var currentTime = timeProvider.GetUtcNow();
        var currentDayOfWeek = currentTime.DayOfWeek;
        var currentTimeOnly = TimeOnly.FromTimeSpan(currentTime.TimeOfDay);

        return ChargingProhibited
            .Any(entry =>
                currentDayOfWeek == entry.Weekday &&
                currentTimeOnly >= entry.StartTime &&
                currentTimeOnly <= entry.EndTime);
    }
}