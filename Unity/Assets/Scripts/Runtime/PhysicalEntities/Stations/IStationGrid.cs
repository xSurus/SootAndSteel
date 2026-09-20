using Gamelab.Utils;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Stations
{
    // Seam for Src gameplayContext.Map.GetAdjacentStation. The map (wave B1) supplies it.
    // Position is in whatever unit the map uses; conveyors only pass their own Position back.
    public interface IStationGrid
    {
        StationRuntime GetAdjacentStation(Vector2 position, GridDirection direction);
    }
}
