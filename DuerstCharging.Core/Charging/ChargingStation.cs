using System.Net;
using FluentModbus;

namespace DuerstCharging.Core.Charging;

public class ChargingStation
{
    private DateTimeOffset lastUpdate = DateTimeOffset.MinValue;

    private ChargingStation(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
    }

    public static async Task<ChargingStation> Create(IPAddress ipAddress)
    {
        var chargingStation = new ChargingStation(ipAddress);
        await chargingStation.RetrieveInformation();
        return chargingStation;
    }

    public IPAddress IpAddress { get; init; }
    public ChargingState ChargingState { get; private set; }
    public CableState CableState { get; private set; }
    public uint ErrorCode { get; private set; }

    public override string ToString() => $"{IpAddress}";

    public async Task RetrieveInformation()
    {
        if (lastUpdate.AddMilliseconds(500) > DateTimeOffset.UtcNow)
        {
            // NOTE: Manual recommends to retrieve information not more then every 500ms
            return;
        }
        
        using var client = GetConnectedClient();

        if (client is null)
        {
            throw new Exception($"Error connecting to the charging station on IP {IpAddress}");
        }

        async Task<uint> GetUintRegister(ModbusTcpClient modbusClient, int startingAddress) =>
            (await modbusClient.ReadHoldingRegistersAsync<uint>(
                255,
                startingAddress,
                1)).Span[0];

        ChargingState = (ChargingState)await GetUintRegister(client, 1000);
        CableState = (CableState)await GetUintRegister(client, 1004);
        ErrorCode = await GetUintRegister(client, 1006);

        lastUpdate = DateTimeOffset.UtcNow;
    }

    private ModbusTcpClient GetConnectedClient()
    {
        var client = new ModbusTcpClient();
        client.Connect(IpAddress, ModbusEndianness.BigEndian);

        return client;
    }
}