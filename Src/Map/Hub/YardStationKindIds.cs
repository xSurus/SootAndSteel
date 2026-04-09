namespace Gamelab.Map.Hub;

/// <summary>
/// Stable ids for prep/hub serialization (<see cref="PrepStationEntry"/>), <see cref="StationYardFactory"/>,
/// and hub shop offers. Must stay aligned with each station's <c>Type</c> string where layout capture uses
/// <c>Type</c> directly (non-resource stations).
/// </summary>
public static class YardStationKindIds
{
    public const string Cannon = "Cannon";
    public const string Anvil = "Anvil";
    public const string Counter = "Counter";
    public const string Coal = "Coal";
    public const string Gunpowder = "Gunpowder";
    public const string CoalOven = "CoalOven";
    public const string SpeedLever = "SpeedLever";
    public const string Copper = "Copper";
}
