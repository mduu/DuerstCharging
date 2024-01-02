namespace DuerstCharging.Core;

public enum CableState
{
    /// <summary>
    /// No cable is plugged
    /// </summary>
    NoCable = 0,

    /// <summary>
    /// Cable is connected to the charging station (not to the electric vehicle).
    /// </summary>
    CablePluggedToChargingStation = 1,

    /// <summary>
    /// Cable is connected to the charging station and locked (not to the elec- tric vehicle).
    /// </summary>
    CablePluggedToChargingStationAndLocked = 3,

    /// <summary>
    /// Cable is connected to the charging station and the electric vehicle (not locked).
    /// </summary>
    CablePluggedNotCharging = 5,

    /// <summary>
    /// Cable is connected to the charging station and the electric vehicle and locked (charging).
    /// </summary>
    CablePluggedAndCharging = 7,
}