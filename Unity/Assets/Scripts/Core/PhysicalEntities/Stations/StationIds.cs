// Unity/Assets/Scripts/Core/PhysicalEntities/Stations/StationIds.cs
namespace Gamelab.PhysicalEntities.Stations
{
    public static class StationIds
    {
        public const string Cannon = "Cannon";
        public const string BulletRack = "BulletRack";
        public const string Conveyor = "Conveyor";
        public const string UpgradedComponentConveyor = "UpgradedComponentConveyor";
        public const string BulletConveyor = "BulletConveyor";
        public const string Workbench = "Workbench";
        public const string AutoWorkbench = "AutoWorkbench";
        public const string Counter = "Counter";
        public const string SpeedLever = "SpeedLever";
        public const string Resource = "Resource";
        public const string Component = "Component";

        public static string GetComponentResourceId(string componentId)
        {
            return Component + componentId;
        }

        public static string GetResourceStationId(string resourceId)
        {
            return Resource + resourceId;
        }

        public static string GetStationResourceId(string stationId)
        {
            return stationId.Split(Resource)[1];
        }

        public static string GetStationComponentId(string stationId)
        {
            return stationId.Split(Component)[1];
        }

        public static bool IsResourceStationId(string stationId)
        {
            return stationId.StartsWith(Resource);
        }

        public static bool IsComponentStationId(string stationId)
        {
            return stationId.StartsWith(Resource + Component);
        }
    }
}
