using System.Net;
using FluentModbus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuerstCharging.Core.Charging;

public class ChargingStation : IChargingStation
{
    private const int TheUnitIdentifier = 255;
    private readonly ILogger<ChargingStation> logger;
    private DateTimeOffset lastRetrieve = DateTimeOffset.MinValue;

    private ChargingStation(
        ILogger<ChargingStation> logger,
        IPAddress ipAddress)
    {
        this.logger = logger;
        IpAddress = ipAddress;
    }

    public IPAddress IpAddress { get; }
    public ChargingState ChargingState { get; private set; }
    public CableState CableState { get; private set; }
    public uint ErrorCode { get; private set; }
    public uint FailsafeCurrentSetting { get; private set; }
    public uint FailsafeTimeoutSetting { get; private set; }
    
    public bool IsEnabled => ChargingState != ChargingState.Suspended;

    public async Task RetrieveInformation()
    {
        if (lastRetrieve.AddMilliseconds(500) > DateTimeOffset.UtcNow)
        {
            // NOTE: Manual recommends to retrieve information not more then every 500ms
            return;
        }

        using var client = GetConnectedClient();

        if (client is null)
        {
            throw new Exception($"Error connecting to the charging station on IP {IpAddress}");
        }

        async Task<uint> GetUint32Register(ModbusTcpClient modbusClient, int startingAddress) =>
            (await modbusClient.ReadHoldingRegistersAsync<uint>(
                TheUnitIdentifier,
                startingAddress,
                1)).Span[0];

        ChargingState = (ChargingState)await GetUint32Register(client, 1000);
        CableState = (CableState)await GetUint32Register(client, 1004);
        ErrorCode = await GetUint32Register(client, 1006);
        FailsafeCurrentSetting = await GetUint32Register(client, 1600);
        FailsafeTimeoutSetting = await GetUint32Register(client, 1602);

        lastRetrieve = DateTimeOffset.UtcNow;
    }

    public async Task SetEnabled(bool isEnabled, bool simulateOnly, CancellationToken cancellationToken)
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
            await client.WriteSingleRegisterAsync(
                TheUnitIdentifier,
                5014,
                value,
                cancellationToken);
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

    private ModbusTcpClient GetConnectedClient()
    {
        var client = new ModbusTcpClient();
        client.Connect(IpAddress, ModbusEndianness.BigEndian);

        return client;
    }
}