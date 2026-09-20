using System.Collections.Generic;
using System.Numerics;
using Gamelab.Utils;

namespace Gamelab.Map.Train
{
    // Station occupancy of the train grid (Src TrainMap.stationGrid).
    public sealed class TrainGrid<TStation> where TStation : class
    {
        private readonly TrainLayout layout;
        private readonly Dictionary<TrainPoint, TStation> cells = new Dictionary<TrainPoint, TStation>();

        public TrainGrid(TrainLayout layout)
        {
            this.layout = layout;
        }

        // Like Src SnapToNearestValidCell: drops the station's previous cell, then sets the new one.
        public void Register(TrainPoint cell, TStation station)
        {
            Remove(station);
            cells[cell] = station;
        }

        public void Remove(TStation station)
        {
            foreach (KeyValuePair<TrainPoint, TStation> pair in cells)
            {
                if (pair.Value == station)
                {
                    cells.Remove(pair.Key);
                    return;
                }
            }
        }

        public bool TryGet(TrainPoint cell, out TStation station) => cells.TryGetValue(cell, out station);

        public bool IsOccupied(TrainPoint cell) => cells.ContainsKey(cell);

        // Src GetAdjacentStation: neighbour direction is truncated to a point. Null when empty.
        public TStation GetAdjacent(Vector2 posPx, GridDirection dir)
        {
            TrainPoint cur = layout.GetTileIndexFromPixels(posPx);
            Vector2 d = dir.ToVector2();
            cells.TryGetValue(new TrainPoint(cur.X + (int)d.X, cur.Y + (int)d.Y), out TStation s);
            return s;
        }
    }
}
