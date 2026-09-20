using System;
using System.Collections.Generic;
using System.Numerics;
using Gamelab.Config;

namespace Gamelab.Map.Train
{
    // Port of the logic of Src/Map/Train/PatchManager.cs without drawing (the view reads
    // SnowTiles / IceTiles and redraws when Version changes). Each patch type keeps a List
    // (insertion order, used for the melt pick) plus a HashSet (lookup). Src enumerates a
    // HashSet, whose order is unspecified; insertion order is the deterministic equivalent.
    public sealed class PatchField
    {
        private sealed class Patches
        {
            public readonly List<TrainPoint> Order = new List<TrainPoint>();
            public readonly HashSet<TrainPoint> Set = new HashSet<TrainPoint>();
            public float SpawnTimer;
            public float MeltTimer;
            public bool Add(TrainPoint p)
            {
                if (!Set.Add(p)) return false;
                Order.Add(p);
                return true;
            }
            public bool Remove(TrainPoint p)
            {
                if (!Set.Remove(p)) return false;
                Order.Remove(p);
                return true;
            }
        }

        private readonly TrainLayout layout;
        private readonly TrainStateTuning tuning;
        private readonly Random random;
        private readonly Func<TrainPoint, bool> isOccupied;
        private readonly List<TrainPoint> allTiles;
        private readonly Patches snow = new Patches();
        private readonly Patches ice = new Patches();

        // Incremented on every add or remove so the view can redraw only on change.
        public int Version { get; private set; }
        public IReadOnlyList<TrainPoint> SnowTiles => snow.Order;
        public IReadOnlyList<TrainPoint> IceTiles => ice.Order;

        public PatchField(TrainLayout layout, TrainStateTuning tuning, Random random,
            Func<TrainPoint, bool> isOccupied)
        {
            this.layout = layout;
            this.tuning = tuning;
            this.random = random;
            this.isOccupied = isOccupied;
            allTiles = new List<TrainPoint>(layout.Width * layout.Height);
            for (int x = 0; x < layout.Width; x++)
                for (int y = 0; y < layout.Height; y++)
                    allTiles.Add(new TrainPoint(x, y));
        }

        public int TileCount => allTiles.Count;

        public int TargetCount(float tempRatio, float threshold, float maxCoverage)
        {
            if (tempRatio >= threshold) return 0;
            float coldnessFraction = 1f - tempRatio / threshold;
            return (int)(allTiles.Count * maxCoverage * coldnessFraction);
        }

        public void Update(float dt, float temperature)
        {
            float tempRatio = temperature / tuning.MaxTemperature;
            UpdatePatchType(dt, tempRatio, tuning.SnowStartThreshold, tuning.SnowMaxCoverage,
                tuning.SnowSpawnIntervalSeconds, tuning.SnowMeltIntervalSeconds, snow);
            UpdatePatchType(dt, tempRatio, tuning.IceStartThreshold, tuning.IceMaxCoverage,
                tuning.IceSpawnIntervalSeconds, tuning.IceMeltIntervalSeconds, ice);
        }

        private void UpdatePatchType(float dt, float tempRatio, float threshold, float maxCoverage,
            float spawnInterval, float meltInterval, Patches p)
        {
            int target = TargetCount(tempRatio, threshold, maxCoverage);
            if (p.Order.Count < target)
            {
                p.SpawnTimer += dt;
                if (p.SpawnTimer >= spawnInterval)
                {
                    p.SpawnTimer = 0f;
                    TrySpawnInto(p);
                }
            }
            else if (p.Order.Count > target)
            {
                p.MeltTimer += dt;
                if (p.MeltTimer >= meltInterval)
                {
                    p.MeltTimer = 0f;
                    TryMeltOneFrom(p);
                }
            }
            else
            {
                p.SpawnTimer = 0f;
                p.MeltTimer = 0f;
            }
        }

        private void TrySpawnInto(Patches target)
        {
            if (target.Order.Count >= allTiles.Count) return;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                TrainPoint candidate = allTiles[random.Next(allTiles.Count)];
                if (snow.Set.Contains(candidate) || ice.Set.Contains(candidate) || isOccupied(candidate))
                    continue;
                target.Add(candidate);
                Version++;
                return;
            }
        }

        private void TryMeltOneFrom(Patches p)
        {
            if (p.Order.Count == 0) return;
            int idx = random.Next(p.Order.Count);
            p.Remove(p.Order[idx]);
            Version++;
        }

        public bool IsOnSnow(Vector2 px) => snow.Set.Contains(layout.GetTileIndexFromPixels(px));

        public bool IsOnIce(Vector2 px) => ice.Set.Contains(layout.GetTileIndexFromPixels(px));

        // Removes whichever patch type is under the position. Returns true if anything was removed.
        public bool TryRemovePatchAt(Vector2 px)
        {
            TrainPoint tile = layout.GetTileIndexFromPixels(px);
            bool removed = snow.Remove(tile) || ice.Remove(tile);
            if (removed) Version++;
            return removed;
        }
    }
}
