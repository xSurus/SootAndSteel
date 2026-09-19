// Unity/Assets/Scripts/Core/PhysicalEntities/Stations/StationDefinition.cs
namespace Gamelab.PhysicalEntities.Stations
{
    // Catalog-relevant subset of the original Gamelab.Config.StationConfig (JSON-loaded,
    // not ported — Scope decisions #5). Shop pricing/icon fields aren't included since
    // Src/Items/ (inventory) and Src/Services/Shop/ aren't ported either.
    public sealed class StationDefinition
    {
        public string StationId { get; }
        public string Name { get; }
        public string Description { get; }

        public StationDefinition(string stationId, string name, string description)
        {
            StationId = stationId;
            Name = name;
            Description = description;
        }
    }
}
