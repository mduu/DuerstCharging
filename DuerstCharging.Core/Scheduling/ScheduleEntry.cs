namespace DuerstCharging.Core.Scheduling;

public record ScheduleEntry(
    DayOfWeek Weekday,
    TimeOnly StartTime,
    TimeOnly EndTime);