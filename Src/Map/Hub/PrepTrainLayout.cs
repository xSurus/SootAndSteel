using System.Collections.Generic;
using Gamelab.Map.Train;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Resources;
using Microsoft.Xna.Framework;

namespace Gamelab.Map.Hub;

public readonly record struct PrepStationEntry(string KindId, int TileX, int TileY);

public static class PrepTrainLayout
{
    public static List<PrepStationEntry> Capture(TrainMap map)
    {
        List<PrepStationEntry> list = [];
        foreach (IPhysicalEntity entity in map.MapObjects)
        {
            if (entity is not AbstractStation station)
            {
                continue;
            }

            string kindId = ToKindId(station);
            Point t = map.GetTileIndexFromPixels(station.Position);
            if (t.X < 0 || t.X >= map.Width || t.Y < 0 || t.Y >= map.Height)
            {
                continue;
            }

            list.Add(new PrepStationEntry(kindId, t.X, t.Y));
        }

        return list;
    }

    /// <summary>Maps runtime station to <see cref="YardStationKindIds"/> / <see cref="StationYardFactory"/>.</summary>
    public static string ToKindId(AbstractStation station) =>
        station switch
        {
            CoalResource => YardStationKindIds.Coal,
            GunpowderResource => YardStationKindIds.Gunpowder,
            CopperResource => YardStationKindIds.Copper,
            _ => station.Type
        };

    public static void ApplyLayout(TrainMap trainMap, List<PrepStationEntry> layout)
    {
        foreach (PrepStationEntry e in layout)
        {
            Vector2 center = trainMap.GetTileCenterPixels(e.TileX, e.TileY);
            AbstractStation station = StationYardFactory.CreateYardStation(e.KindId, center);
            trainMap.MapObjects.Add(station);
            trainMap.SnapToNearestValidCell(station);
        }
    }

    public static void ApplyFromPendingOrDefault(GamelabGame game, TrainMap trainMap)
    {
        List<PrepStationEntry>? layout = game.PendingPrepTrainLayout;
        game.PendingPrepTrainLayout = null;

        if (layout == null || layout.Count == 0)
        {
            trainMap.AddDefaultStationLoadout();
            return;
        }

        ApplyLayout(trainMap, layout);
    }
}
