using System;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public static class StationFactory
{
    public static AbstractStation CreateStation(string kindId, Vector2 centerPixels)
    {
        switch (kindId)
        {
            case StationIds.Cannon: return new CannonStation(centerPixels);
            case StationIds.Workbench: return new Workbench(centerPixels);
            case StationIds.Counter: return new Counter(centerPixels);
            case StationIds.SpeedLever: return new SpeedLever(centerPixels);
            case { } id when StationIds.IsComponentStationId(id):
                return new ComponentResource(centerPixels, StationIds.GetStationComponentId(id));
        }

        throw new ArgumentOutOfRangeException(nameof(kindId), kindId, $"Unknown station kind id '{kindId}'.");
    }
}