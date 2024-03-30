using DuerstCharging.Core;
using DuerstCharging.Core.Charging;
using DuerstCharging.Core.Logging;
using DuerstCharging.Core.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace DuerstCharging;

public static class ServiceConfiguration
{
    public static IServiceCollection AddChargingServices(this IServiceCollection services)
    {
        services.AddTransient<TimeProvider>(_ => TimeProvider.System);
        services.AddSingleton<IChargingNetwork, ChargingNetwork>();
        services.AddSingleton<ISchedule, Schedule>();
        services.AddSingleton<IChargingManager, ChargingManager>();

        services.AddTransient<ILogReader, LogReader>();

        services.AddHostedService<Worker>();

        return services;
    }
}