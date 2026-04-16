using System;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public static class StationFactory
{
    public static AbstractStation CreateStation(string kindId, Vector2 centerPixels)
    {
        return kindId switch
        {
            StationIds.Cannon => new CannonStation(centerPixels),
            StationIds.Workbench => new Workbench(centerPixels),
            StationIds.Counter => new Counter(centerPixels),
            StationIds.SpeedLever => new SpeedLever(centerPixels),
            StationIds.BasicPropellant => new ComponentResource(centerPixels, StationIds.BasicPropellant),
            StationIds.BasicCasing => new ComponentResource(centerPixels, StationIds.BasicCasing),
            StationIds.BasicProjectile => new ComponentResource(centerPixels, StationIds.BasicProjectile),
            StationIds.HomingCasing => new ComponentResource(centerPixels, StationIds.HomingCasing),
            StationIds.ScatterProjectile => new ComponentResource(centerPixels, StationIds.ScatterProjectile),

            _ => throw new ArgumentOutOfRangeException(nameof(kindId), kindId, "Unknown entity kind id.")
        };
    }
}