using System.Net;
using FluentModbus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace DuerstCharging.Core.Charging;

public class ChargingStation : IChargingStation
{
    private const int TheUnitIdentifier = 255;
    private readonly ILogger<ChargingStation> logger;
    private DateTimeOffset lastSuccessfulRetrieve = DateTimeOffset.MinValue;

    private readonly ResiliencePipeline<ModbusTcpClient> modbusConnectionPipeline;
    private readonly ResiliencePipeline<Memory<uint>> readResiliencePipeline;
    private readonly ResiliencePipeline writeResiliencePipeline;

    private ChargingStation(
        ILogger<ChargingStation> logger,
        IPAddress ipAddress)
    {
        this.logger = logger;
        IpAddress = ipAddress;

        modbusConnectionPipeline = new ResiliencePipelineBuilder<ModbusTcpClient>()
            .AddRetry(new RetryStrategyOptions<ModbusTcpClient>
            {
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 10,
                MaxDelay = TimeSpan.FromMinutes(15),
                Name = "Retry Modbus Connection",
                OnRetry = args
                    =>
                {
                    logger.LogInformation(
                        "Retry #{RetryAttemptNumber} connecting Modbus (Duration: {Duration} to station {Station}",
                        args.AttemptNumber,
                        args.Duration,
                        this);
                    return default;
                },
            })
            .Build();

        readResiliencePipeline = new ResiliencePipelineBuilder<Memory<uint>>()
            .AddRetry(new RetryStrategyOptions<Memory<uint>>
            {
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                MaxDelay = TimeSpan.FromMinutes(5),
                Name = "Retry read from modbus",
                OnRetry = args
                    =>
                {
                    logger.LogInformation(
                        "Retry #{RetryAttemptNumber} reading from modbus (Duration: {Duration} for station {Station}",
                        args.AttemptNumber,
                        args.Duration,
                        this);
                    return default;
                },
            })
            .Build();

        writeResiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                MaxDelay = TimeSpan.FromMinutes(5),
                Name = "Retry write to modbus",
                OnRetry = args
                    =>
                {
                    logger.LogInformation(
                        "Retry #{RetryAttemptNumber} writing to modbus (Duration: {Duration} for station {Station}",
                        args.AttemptNumber,
                        args.Duration,
                        this);
                    return default;
                },
            })
            .Build();
    }

    public IPAddress IpAddress { get; }
    public ChargingState ChargingState { get; private set; }
    public CableState CableState { get; private set; }
    public uint ErrorCode { get; private set; }
    public uint FailsafeCurrentSetting { get; private set; }
    public uint FailsafeTimeoutSetting { get; private set; }

    public bool IsEnabled => ChargingState != ChargingState.Suspended;

    public async Task<bool> RetrieveInformation()
    {
        if (lastSuccessfulRetrieve.AddMilliseconds(500) > DateTimeOffset.UtcNow)
        {
            // NOTE: Manual recommends to retrieve information not more then every 500ms
            logger.LogInformation(
                "Throttle information retrieval from station {Station} because last time as less then 500ms ago ({LastRetrieveTime:G}) and its recommended to not retrieve more often then every 500ms",
                this,
                lastSuccessfulRetrieve);

            return true;
        }

        try
        {
            using var client = GetConnectedClient();

            async Task<uint> GetUint32Register(ModbusTcpClient modbusClient, int startingAddress)
            {
                var registerValue = await readResiliencePipeline
                    .ExecuteAsync(async ct =>
                        await modbusClient.ReadHoldingRegistersAsync<uint>(
                            TheUnitIdentifier,
                            startingAddress,
                            1, ct));

                return registerValue.Span[0];
            }

            ChargingState = (ChargingState)await GetUint32Register(client, 1000);
            CableState = (CableState)await GetUint32Register(client, 1004);
            ErrorCode = await GetUint32Register(client, 1006);
            FailsafeCurrentSetting = await GetUint32Register(client, 1600);
            FailsafeTimeoutSetting = await GetUint32Register(client, 1602);

            lastSuccessfulRetrieve = DateTimeOffset.UtcNow;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error retrieving information from station {Station} over modbus",
                this);

            return false;
        }
    }

    public async Task<bool> SetEnabled(bool isEnabled, bool simulateOnly, CancellationToken cancellationToken)
    {
        try
        {
            using var client = GetConnectedClient();

            var value = (ushort)(isEnabled ? 1 : 0);

            if (simulateOnly)
            {
                logger.LogInformation("Simulate setting enable={Value} of charging state {IpAddress}",
                    value,
                    IpAddress);
            }
            else
            {
                await writeResiliencePipeline.ExecuteAsync(async ct =>
                {
                    await client.WriteSingleRegisterAsync(
                        TheUnitIdentifier,
                        5014,
                        value,
                        ct);
                }, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error setting enabled state of the charging-station {Station} to {EnabledState} (simulate-only={SimulationMode})",
                this,
                isEnabled,
                simulateOnly ? "true" : "false");

            return false;
        }
    }

    public static async Task<ChargingStation> Create(
        IServiceProvider serviceProvider,
        IPAddress ipAddress)
    {
        var chargingStation = new ChargingStation(
            serviceProvider.GetRequiredService<ILogger<ChargingStation>>(),
            ipAddress);

        await chargingStation.RetrieveInformation();

        return chargingStation;
    }

    public override string ToString() => $"{IpAddress}";

    private ModbusTcpClient GetConnectedClient() =>
        modbusConnectionPipeline
            .Execute(() =>
            {
                var client = new ModbusTcpClient();
                client.Connect(IpAddress, ModbusEndianness.BigEndian);

                logger.LogDebug("Connected to {Station}", IpAddress);

                return client;
            });
}