using System;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Microsoft.Xna.Framework;

namespace Gamelab.Map.Hub;

public static class StationYardFactory
{
    public static AbstractStation CreateYardStation(string kindId, Vector2 centerPixels)
    {
        return kindId switch
        {
            YardStationKindIds.Cannon => new CannonStation(centerPixels),
            YardStationKindIds.Workbench => new Workbench(centerPixels),
            YardStationKindIds.Counter => new Counter(centerPixels),
            YardStationKindIds.SpeedLever => new SpeedLever(centerPixels),
            YardStationKindIds.BasicPropellant => new ComponentResource(centerPixels, YardStationKindIds.BasicPropellant),
            YardStationKindIds.BasicCasing => new ComponentResource(centerPixels, YardStationKindIds.BasicCasing),
            YardStationKindIds.BasicProjectile => new ComponentResource(centerPixels, YardStationKindIds.BasicProjectile),
            YardStationKindIds.HomingCasing => new ComponentResource(centerPixels, YardStationKindIds.HomingCasing),
            YardStationKindIds.ScatterProjectile => new ComponentResource(centerPixels, YardStationKindIds.ScatterProjectile),
            _ => throw new ArgumentOutOfRangeException(nameof(kindId), kindId, "Unknown yard station kind id.")
        };
    }
}
