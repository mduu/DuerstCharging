using DuerstCharging;
using DuerstCharging.Core;
using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Configuration;
using DuerstCharging.Core.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Environment.ApplicationName = "Duerst Charging";

builder.Services.Configure<ChargingOptions>(
    builder.Configuration.GetSection(nameof(ChargingOptions)));

builder.Services.AddSingleton<ChargingNetwork>();

builder.Services.AddSingleton(
    new Schedule(TimeProvider.System)
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
    });

builder.Services.AddSingleton<ChargingManager>();

builder.Services.AddHostedService<Worker>();

using IHost host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("{AppName} is running", builder.Environment.ApplicationName);
logger.LogInformation("EnvironmentName={EnvironmentName}", builder.Environment.EnvironmentName);
var options = host.Services.GetRequiredService<IOptions<ChargingOptions>>();
logger.LogInformation(
    "Starting configuration: Simulation-only={SimulationOnly}, ChargingStationIpAddress={ChargingStationIpAddress}",
    options.Value.SimulationOnly,
    options.Value.ChargingStationIpAddress);

await host.RunAsync();