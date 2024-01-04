using DuerstCharging;
using DuerstCharging.Core;
using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Configuration;
using DuerstCharging.Core.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);
builder.Environment.ApplicationName = "Duerst Charging";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/chargingman.log", rollingInterval: RollingInterval.Month)
    .WriteTo.Console(LogEventLevel.Information)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(dispose: true));

builder.Services.Configure<ChargingOptions>(
    builder.Configuration.GetSection(nameof(ChargingOptions)));

builder.Services.AddSingleton<ChargingNetwork>();
builder.Services.AddTransient<TimeProvider>(_ => TimeProvider.System);
builder.Services.AddSingleton<Schedule>();
builder.Services.AddSingleton<ChargingManager>();
builder.Services.AddHostedService<Worker>();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("{AppName} is running", builder.Environment.ApplicationName);
logger.LogInformation("EnvironmentName={EnvironmentName}", builder.Environment.EnvironmentName);
var options = host.Services.GetRequiredService<IOptions<ChargingOptions>>();
logger.LogInformation(
    "Starting configuration: Simulation-only={SimulationOnly}, ChargingStationIpAddress={ChargingStationIpAddress}, # schedule entries={NumberOfScheduleEntries}",
    options.Value.SimulationOnly,
    options.Value.ChargingStationIpAddress,
    options.Value.ChargingProhibited.Length);

await host.RunAsync();