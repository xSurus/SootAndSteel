namespace Gamelab.PhysicalEntities.Stations;

public static class StationIds
{
    public const string Cannon = "Cannon";
    public const string Workbench = "Workbench";
    public const string Counter = "Counter";
    public const string SpeedLever = "SpeedLever";
    public const string ComponentResource = "ComponentResource";

    public static string GetComponentStationId(string componentId)
    {
        return ComponentResource + "." + componentId;
    }

    public static string GetStationComponentId(string stationId)
    {
        return stationId.Split('.')[1];
    }

    public static bool IsComponentStationId(string stationId)
    {
        return stationId.StartsWith(ComponentResource + ".");
    }
}