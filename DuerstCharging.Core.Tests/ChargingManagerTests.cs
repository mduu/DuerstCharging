using System.Collections.Immutable;
using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Configuration;
using DuerstCharging.Core.Scheduling;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace DuerstCharging.Core.Tests;

public class ChargingManagerTests
{
    private readonly IChargingNetwork chargingNetwork = A.Fake<IChargingNetwork>();
    private readonly ILogger<ChargingManager> logger = A.Fake<ILogger<ChargingManager>>();
    private readonly IOptionsMonitor<ChargingOptions> options;
    private readonly ISchedule schedule = A.Fake<ISchedule>();
    private readonly ChargingManager sut;
    private readonly IChargingStation theChargingStation = A.Fake<IChargingStation>();

    private ChargingOptions chargingOptions = new()
    {
        SimulationOnly = false,
        ChargingProhibited = Array.Empty<ScheduleEntry>(),
        ChargingStationIpAddress = "1.1.1.1"
    };

    public ChargingManagerTests()
    {
        A.CallTo(() => schedule.GetIsChargingProhibited()).Returns(false);

        options = A.Fake<IOptionsMonitor<ChargingOptions>>();
        A.CallTo(() => options.CurrentValue).Returns(chargingOptions);

        A.CallTo(() => chargingNetwork.GetAllChargingStations())
            .ReturnsLazily(() => Task.FromResult(
                new[] { theChargingStation }.ToImmutableArray()));

        sut = new ChargingManager(logger, options, schedule, chargingNetwork);
        chargingOptions = new ChargingOptions
        {
            SimulationOnly = false,
            ChargingProhibited = Array.Empty<ScheduleEntry>(),
            ChargingStationIpAddress = "1.1.1.1"
        };
    }

    [Fact]
    public async Task StartUp_Always_MustUpdateChargingStationState()
    {
        // Arrange

        // Act
        await sut.StartUp(CancellationToken.None);

        // Assert
        A.CallTo(
                () => theChargingStation.SetEnabled(A<bool>._, A<bool>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task UpdateIfNeeded_IfProhibitedDidNotChanged_MustNotUpdateChargingStationState()
    {
        // Arrange

        // Act
        await sut.UpdateIfNeeded(CancellationToken.None);

        // Assert
        A.CallTo(
                () => theChargingStation.SetEnabled(A<bool>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateIfNeeded_IfProhibitedChanged_MustUpdateChargingStationState()
    {
        // Arrange
        A.CallTo(() => schedule.GetIsChargingProhibited()).Returns(true);

        // Act
        await sut.UpdateIfNeeded(CancellationToken.None);

        // Assert
        A.CallTo(
                () => theChargingStation.SetEnabled(A<bool>._, A<bool>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}