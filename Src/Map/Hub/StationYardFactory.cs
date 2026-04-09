using System;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Stations.Workbenches;
using Microsoft.Xna.Framework;

namespace Gamelab.Map.Hub;

public static class StationYardFactory
{
    public static AbstractStation CreateYardStation(string kindId, Vector2 centerPixels)
    {
        return kindId switch
        {
            YardStationKindIds.Cannon => new CannonStation(centerPixels),
            YardStationKindIds.Anvil => new Anvil(centerPixels),
            YardStationKindIds.Counter => new Counter(centerPixels),
            YardStationKindIds.Coal => new CoalResource(centerPixels),
            YardStationKindIds.Gunpowder => new GunpowderResource(centerPixels),
            YardStationKindIds.CoalOven => new CoalOven(centerPixels),
            YardStationKindIds.SpeedLever => new SpeedLever(centerPixels),
            YardStationKindIds.Copper => new CopperResource(centerPixels),
            _ => throw new ArgumentOutOfRangeException(nameof(kindId), kindId, "Unknown yard station kind id.")
        };
    }
}
