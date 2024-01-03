namespace DuerstCharging.Core.Charging;

public enum ChargingState
{
    /// <summary>
    /// Start-up of the charging station.
    /// </summary>
    Starting = 0,

    /// <summary>
    /// The charging station is not ready for charging. The charging station is not connected to an electric
    /// vehicle, it is locked by the authorization function or another mechanism.
    /// </summary>
    UnpluggedOrLocked = 1,

    /// <summary>
    /// The charging station is ready for charging and waits for a reaction from the electric vehicle.
    /// </summary>
    PluggedInAndWaitingForCar = 2,

    /// <summary>
    /// A charging process is active.
    /// </summary>
    Charging = 3,

    /// <summary>
    /// An error has occurred.
    /// </summary>
    Error = 4,

    /// <summary>
    /// The charging process is temporarily interrupted because the tempera- ture is too high or the wallbox is in
    /// suspended mode.
    /// </summary>
    Suspended = 5,
}