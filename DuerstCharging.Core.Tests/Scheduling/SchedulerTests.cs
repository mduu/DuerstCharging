using DuerstCharging.Core.Scheduling;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace DuerstCharging.Core.Tests.Scheduling;

public class SchedulerTests
{
    private readonly Schedule sut;
    private readonly FakeTimeProvider timeProviderFake = new();

    public SchedulerTests()
    {
        sut = new Schedule(timeProviderFake)
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
    }

    [Fact]
    public void GetIsChargingProhibited_OutsideOfProhibited_MustReturnFalse()
    {
        timeProviderFake.SetUtcNow(
            new DateTimeOffset(2024, 1, 8, 6, 59, 0, TimeSpan.Zero));

        var result = sut.GetIsChargingProhibited();

        result.Should().BeFalse();
    }

    [Fact]
    public void GetIsChargingProhibited_CloseInsightStartingOfProhibited_MustReturnTrue()
    {
        timeProviderFake.SetUtcNow(
            new DateTimeOffset(2024, 1, 8, 7, 0, 0, TimeSpan.Zero));

        var result = sut.GetIsChargingProhibited();

        result.Should().BeTrue();
    }

    [Fact]
    public void GetIsChargingProhibited_CloseInsideEndingOfProhibited_MustReturnTrue()
    {
        timeProviderFake.SetUtcNow(
            new DateTimeOffset(2024, 1, 9, 20, 0, 0, TimeSpan.Zero));

        var result = sut.GetIsChargingProhibited();

        result.Should().BeTrue();
    }

    [Fact]
    public void GetIsChargingProhibited_CloseOutsideOfProhibited_MustReturnFalse()
    {
        timeProviderFake.SetUtcNow(
            new DateTimeOffset(2024, 1, 9, 20, 0, 1, TimeSpan.Zero));

        var result = sut.GetIsChargingProhibited();

        result.Should().BeFalse();
    }

    [Fact]
    public void GetIsChargingProhibited_WithoutProhibitedAtThatDay_MustReturnFalse()
    {
        timeProviderFake.SetUtcNow(
            new DateTimeOffset(2024, 1, 14, 9, 0, 0, TimeSpan.Zero));

        var result = sut.GetIsChargingProhibited();

        result.Should().BeFalse();
    }
}