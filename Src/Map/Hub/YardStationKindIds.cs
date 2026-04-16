namespace Gamelab.Map.Hub;

/// <summary>
/// Stable ids for prep/hub serialization (<see cref="PrepStationEntry"/>), <see cref="StationYardFactory"/>,
/// and hub shop offers. Must stay aligned with each station's <c>Type</c> string where layout capture uses
/// <c>Type</c> directly (non-resource stations).
/// </summary>
public static class YardStationKindIds
{
    public const string Cannon = "Cannon";
    public const string Workbench = "Workbench";
    public const string Counter = "Counter";
    public const string Coal = "Coal";
    public const string CoalOven = "CoalOven";
    public const string SpeedLever = "SpeedLever";
    public const string BasicProjectile = "BasicProjectile";
    public const string BasicCasing = "BasicCasing";
    public const string BasicPropellant = "BasicPropellant";
    public const string HomingCasing = "HomingCasing";
    public const string ScatterProjectile = "ScatterProjectile";
}
