using DuerstCharging;
using DuerstCharging.Components;
using DuerstCharging.Core.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Environment.ApplicationName = "Duerst Charging";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/chargingman.log", rollingInterval: RollingInterval.Month)
    .WriteTo.Console(LogEventLevel.Information)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(dispose: true));

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

builder.Services.Configure<ChargingOptions>(
    builder.Configuration.GetSection(nameof(ChargingOptions)));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddChargingServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Starting log output
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("{AppName} is running", builder.Environment.ApplicationName);
logger.LogInformation("EnvironmentName={EnvironmentName}", builder.Environment.EnvironmentName);
var options = app.Services.GetRequiredService<IOptions<ChargingOptions>>();
logger.LogInformation(
    "Starting configuration: Simulation-only={SimulationOnly}, ChargingStationIpAddress={ChargingStationIpAddress}, # schedule entries={NumberOfScheduleEntries}",
    options.Value.SimulationOnly,
    options.Value.ChargingStationIpAddress,
    options.Value.ChargingProhibited.Length);

await app.RunAsync();