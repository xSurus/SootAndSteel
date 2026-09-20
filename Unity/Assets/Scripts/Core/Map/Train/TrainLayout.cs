using System;
using System.Collections.Generic;
using System.Numerics;

namespace Gamelab.Map.Train
{
    public enum WallKind
    {
        ShootHole,
        Door
    }

    public readonly struct WallSpec
    {
        public WallKind Kind { get; }
        public bool IsTop { get; }
        public Vector2 CenterPx { get; }
        public Vector2 SizePx { get; }

        public WallSpec(WallKind kind, bool isTop, Vector2 centerPx, Vector2 sizePx)
        {
            Kind = kind;
            IsTop = isTop;
            CenterPx = centerPx;
            SizePx = sizePx;
        }
    }

    // Engine-free train geometry from Src TrainMap. Pixels, y down.
    public sealed class TrainLayout
    {
        // Cannon wagon interior in tile coords (Src WagonInteriorMin/Max).
        public const int WagonInteriorMinX = -3;
        public const int WagonInteriorMaxX = -2;
        public const int WagonInteriorMinY = -1;
        public const int WagonInteriorMaxY = 3;

        public int Width { get; }
        public int Height { get; }
        public int TileSize { get; }
        public Vector2 Position { get; }
        public RectPx? CannonWagonBounds { get; set; }
        public IReadOnlyList<WallSpec> WallSpecs => wallSpecs;
        public IReadOnlyList<TrainPoint> LeftWallTiles => leftWallTiles;
        public int DoorRowMin => Height / 2 - 1;
        public int DoorRowMax => Height / 2;

        private readonly List<WallSpec> wallSpecs = new List<WallSpec>();
        private readonly List<TrainPoint> leftWallTiles = new List<TrainPoint>();
        private int[] tileVariants;

        public TrainLayout(int width, int height, int tileSize, Vector2 topLeftPx, params DoorSpec[] doors)
        {
            Width = width;
            Height = height;
            TileSize = tileSize;
            Position = topLeftPx;
            doors = doors ?? new DoorSpec[0];

            float halfTile = TileSize / 2f;
            Vector2 wallSize = new Vector2(TileSize, halfTile);
            for (int x = 0; x < Width; x++)
            {
                Vector2 topPos = GetTileCenterPixels(x, 0) - new Vector2(0, halfTile + halfTile / 2f - 0.5f);
                bool topDoor = Array.Exists(doors, d => !d.OnBottom && d.Column == x);
                wallSpecs.Add(new WallSpec(topDoor ? WallKind.Door : WallKind.ShootHole, true, topPos, wallSize));

                Vector2 bottomPos = GetTileCenterPixels(x, Height - 1) + new Vector2(0, halfTile + halfTile / 2f);
                bool bottomDoor = Array.Exists(doors, d => d.OnBottom && d.Column == x);
                wallSpecs.Add(new WallSpec(bottomDoor ? WallKind.Door : WallKind.ShootHole, false, bottomPos, wallSize));
            }

            for (int y = 0; y < Height; y++)
            {
                if (y >= DoorRowMin && y <= DoorRowMax) continue;
                leftWallTiles.Add(new TrainPoint(-1, y));
            }
        }

        public static Vector2 DefaultTopLeftPx(int screenW, int screenH, int width, int height, int tileSize)
        {
            return new Vector2((screenW - width * tileSize) / 2f, (screenH - height * tileSize) / 2f);
        }

        // Src draws tile variants in the ctor with random.Next(0, 2), x outer, y inner.
        public void RollTileVariants(Random random)
        {
            tileVariants = new int[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    tileVariants[x * Height + y] = random.Next(0, 2);
                }
            }
        }

        public int GetTileVariant(int x, int y)
        {
            if (tileVariants == null) throw new InvalidOperationException("Call RollTileVariants first.");
            return tileVariants[x * Height + y];
        }

        public Vector2 GetTileTopLeftPixels(int x, int y) => Position + new Vector2(x * TileSize, y * TileSize);

        public Vector2 GetTileCenterPixels(int x, int y) => GetTileTopLeftPixels(x, y) + new Vector2(TileSize / 2f);

        public Vector2 GetTileCenterMeters(int x, int y) => WorldUnits.ToMeters(GetTileCenterPixels(x, y));

        public TrainPoint GetTileIndexFromPixels(Vector2 pixelPosition)
        {
            Vector2 local = pixelPosition - Position;
            return new TrainPoint((int)MathF.Floor(local.X / TileSize), (int)MathF.Floor(local.Y / TileSize));
        }

        public RectPx GetBounds() => new RectPx((int)Position.X, (int)Position.Y, Width * TileSize, Height * TileSize);

        public bool IsOnTrain(Vector2 position)
        {
            int px = (int)position.X, py = (int)position.Y;
            if (GetBounds().Contains(px, py)) return true;
            return CannonWagonBounds.HasValue && CannonWagonBounds.Value.Contains(px, py);
        }

        public Vector2 GetFreeSpawnTile(int playerIndex, HashSet<TrainPoint> alreadyChosen, Func<TrainPoint, bool> isOccupied)
        {
            var rows = new List<int>();
            int center = Height / 2;
            rows.Add(center);
            for (int offset = 1; offset < Height; offset++)
            {
                if (center - offset >= 0) rows.Add(center - offset);
                if (center + offset < Height) rows.Add(center + offset);
            }

            foreach (int y in rows)
            {
                for (int i = 0; i < Width; i++)
                {
                    int x = (playerIndex + i) % Width;
                    var tile = new TrainPoint(x, y);
                    if (!isOccupied(tile) && !alreadyChosen.Contains(tile))
                    {
                        alreadyChosen.Add(tile);
                        return GetTileCenterPixels(x, y);
                    }
                }
            }

            var fallback = new TrainPoint(playerIndex, center);
            alreadyChosen.Add(fallback);
            return GetTileCenterPixels(fallback.X, fallback.Y);
        }

        // Src SnapToNearestValidCell cell choice. Null when outside the main grid and the wagon.
        public TrainPoint? SnapCell(Vector2 posPx, RectPx? wagonBounds)
        {
            TrainPoint g = GetTileIndexFromPixels(posPx);
            bool inMain = g.X >= 0 && g.X < Width && g.Y >= 0 && g.Y < Height;
            if (inMain) return g;
            if (wagonBounds.HasValue && wagonBounds.Value.Contains((int)posPx.X, (int)posPx.Y))
            {
                return new TrainPoint(
                    Math.Clamp(g.X, WagonInteriorMinX, WagonInteriorMaxX),
                    Math.Clamp(g.Y, WagonInteriorMinY, WagonInteriorMaxY));
            }
            return null;
        }
    }
}
