using System.Net;
using FluentModbus;

namespace DuerstCharging.Core.Charging;

public class ChargingStation
{
    private const int TheUnitIdentifier = 255;
    private DateTimeOffset lastRetrieve = DateTimeOffset.MinValue;

    private ChargingStation(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
    }

    public IPAddress IpAddress { get; }
    public ChargingState ChargingState { get; private set; }
    public CableState CableState { get; private set; }
    public uint ErrorCode { get; private set; }
    public bool IsEnabled => ChargingState != ChargingState.Suspended;

    public static async Task<ChargingStation> Create(IPAddress ipAddress)
    {
        var chargingStation = new ChargingStation(ipAddress);
        await chargingStation.RetrieveInformation();
        return chargingStation;
    }

    public override string ToString() => $"{IpAddress}";

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

        lastRetrieve = DateTimeOffset.UtcNow;
    }

    public async Task SetEnabled(bool isEnabled, bool simulateOnly, CancellationToken cancellationToken)
    {
        using var client = GetConnectedClient();

        var value = (ushort)(isEnabled ? 1 : 0);

        if (simulateOnly)
        {
            Console.WriteLine($"Simulate setting enable={value} of charging state {IpAddress}");
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

    private ModbusTcpClient GetConnectedClient()
    {
        var client = new ModbusTcpClient();
        client.Connect(IpAddress, ModbusEndianness.BigEndian);

        return client;
    }
}