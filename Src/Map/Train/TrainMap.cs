using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Conveyors;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Serialization;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map.Train;

public readonly record struct DoorSpec(bool OnBottom, int Column);

public class TrainMap : IDisposable
{
    public int Width => GamelabGame.Instance.GameplayConfig.TrainWidth;
    public int Height => GamelabGame.Instance.GameplayConfig.TrainHeight;
    public int TileSize => GamelabGame.Instance.GameplayConfig.TrainTileSize;
    public Vector2 Position { get; private set; }
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly Random random = Random.Shared;

    public List<IPhysicalEntity> MapObjects { get; } = new();
    private readonly Dictionary<Point, AbstractStation> stationGrid = new();
    private CannonWagon cannonWagon;

    public float originalSize => AssetManager.TileTexture[0].Width;
    public float scale => TileSize / originalSize;

    private int[] tileTypes;

    public TrainMap() : this(ComputeDefaultTopLeftPixels())
    {
    }

    public TrainMap(Vector2 topLeftPixels, params DoorSpec[] doors)
    {
        Position = topLeftPixels;
        InitializeBoundaryWalls(doors);

        tileTypes = new int[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tileTypes[x * Height + y] = random.Next(0, 2);
            }
        }
    }

    private static Vector2 ComputeDefaultTopLeftPixels()
    {
        GameplayContext ctx = GamelabGame.Instance.Services.GetService<GameplayContext>()
                              ?? throw new InvalidOperationException(
                                  "GameplayContext must exist before TrainMap is created.");
        GameplayConfig cfg = GamelabGame.Instance.GameplayConfig;
        int w = ctx.ScreenWidth;
        int h = ctx.ScreenHeight;
        return new Vector2(
            (w - cfg.TrainWidth * cfg.TrainTileSize) / 2f,
            (h - cfg.TrainHeight * cfg.TrainTileSize) / 2f);
    }

    public Vector2 GetTileTopLeftPixels(int x, int y)
    {
        return Position + new Vector2(x * TileSize, y * TileSize);
    }

    public Vector2 GetTileCenterPixels(int x, int y)
    {
        return GetTileTopLeftPixels(x, y) + new Vector2(TileSize / 2f);
    }

    public Vector2 GetTileCenterMeters(int x, int y)
    {
        return GetTileCenterPixels(x, y).ToMeters();
    }

    public Point GetTileIndexFromPixels(Vector2 pixelPosition)
    {
        Vector2 localPos = pixelPosition - Position;
        return new Point((int)MathF.Floor(localPos.X / TileSize), (int)MathF.Floor(localPos.Y / TileSize));
    }

    public AbstractStation GetAdjacentStation(Vector2 currentWorldPos, GridDirection direction)
    {
        Point currentGrid = GetTileIndexFromPixels(currentWorldPos);
        Point neighborGrid = currentGrid + direction.ToVector2().ToPoint();
        stationGrid.TryGetValue(neighborGrid, out AbstractStation adjacentStation);
        return adjacentStation;
    }

    public List<StationSaveData> CaptureLayout()
    {
        List<StationSaveData> list = new();
        foreach (IPhysicalEntity entity in MapObjects)
        {
            if (entity is not AbstractStation station)
            {
                continue;
            }

            // Structural racks are spawned by AddDefaultStructures and should not be serialized
            // into the train layout, otherwise they duplicate after load.
            if (station.StationId == StationIds.BulletRack)
            {
                continue;
            }

            // Only persist stations that ended up on the train (incl. cannon wagon). Anything
            // a player dragged outside is considered "left behind".
            if (!IsOnTrain(station.Position))
            {
                continue;
            }

            string kindId = station.StationId;
            Point t = GetTileIndexFromPixels(station.Position);
            GridDirection direction = GridDirection.Right;
            if (station is Conveyor conveyor)
            {
                direction = conveyor.FacingDirection;
            }

            list.Add(new StationSaveData(kindId, t.X, t.Y, direction));
        }

        return list;
    }

    public void LoadLayout(List<StationSaveData> layout)
    {
        if (layout == null || layout.Count == 0)
        {
            AddDefaultStationLoadout();
            AddDefaultStructures();
            return;
        }

        foreach (StationSaveData e in layout)
        {
            // Structural racks are reconstructed by AddDefaultStructures.
            if (e.KindId == StationIds.BulletRack)
            {
                continue;
            }

            Vector2 center = GetTileCenterPixels(e.TileX, e.TileY);
            AbstractStation station = StationFactory.CreateStation(e.KindId, center, e.FacingDirection);
            MapObjects.Add(station);
            SnapToNearestValidCell(station);
        }

        AddDefaultStructures();
    }

    private void InitializeBoundaryWalls(DoorSpec[] doors)
    {
        float halfTile = TileSize / 2f;
        Vector2 wallSize = new Vector2(TileSize, halfTile);

        for (int x = 0; x < Width; x++)
        {
            // top walls
            Vector2 topPos = GetTileCenterPixels(x, 0) - new Vector2(0, halfTile + halfTile / 2f);
            if (Array.Exists(doors, d => !d.OnBottom && d.Column == x))
                MapObjects.Add(new DoorWall(wallSize, topPos));
            else
                MapObjects.Add(new ShootHoleWall(wallSize, topPos, true));

            // bottom walls
            Vector2 bottomPos = GetTileCenterPixels(x, Height - 1) + new Vector2(0, halfTile + halfTile / 2f);
            if (Array.Exists(doors, d => d.OnBottom && d.Column == x))
                MapObjects.Add(new DoorWall(wallSize, bottomPos));
            else
                MapObjects.Add(new ShootHoleWall(wallSize, bottomPos, false));
        }

        // left side wall with passage gap matching DrawSideWalls door rows
        int doorYMin = Height / 2 - 1;
        int doorYMax = Height / 2;
        float tileSimSize = TileSize.ToMeters();
        for (int y = 0; y < Height; y++)
        {
            if (y >= doorYMin && y <= doorYMax) continue;
            Vector2 wallCenterMeters = GetTileCenterMeters(-1, y);
            gameplayContext.PhysicsWorld.CreateRectangle(tileSimSize, tileSimSize, 1f, wallCenterMeters);
        }
    }

    public void AddDefaultStructures()
    {
        float cannonWagonWidth = CannonWagon.WidthTiles * TileSize;

        Vector2 cannonWagonCenter = new Vector2(
            Position.X - cannonWagonWidth / 2f,
            Position.Y + Height * TileSize - CannonWagon.HeightTiles * TileSize / 2f + TileSize / 2f);
        cannonWagon = new CannonWagon(cannonWagonCenter);
        MapObjects.Add(cannonWagon);
        MapObjects.Add(cannonWagon.TopSlot);
        MapObjects.Add(cannonWagon.BottomSlot);
        MapObjects.Add(cannonWagon.TopRack);
        MapObjects.Add(cannonWagon.BottomRack);
        Vector2 coalWagonPos = new Vector2(
            Position.X - cannonWagonWidth - 1.5f * TileSize,
            Position.Y + (Height * TileSize) / 2f);
        MapObjects.Add(new CoalWagon(coalWagonPos));
        MapObjects.Add(new TrainNose(GetTileCenterPixels(8, 2)));
    }

    public void AddDefaultStationLoadout()
    {
        AddStationAndSnap(new SpeedLever(GetTileCenterPixels(7, 3)));
        AddStationAndSnap(new ComponentResourceStation(GetTileCenterPixels(2, 0), ComponentIds.BasicCasing));
        AddStationAndSnap(new ComponentResourceStation(GetTileCenterPixels(2, 4), ComponentIds.BasicProjectile));
        AddStationAndSnap(new ComponentResourceStation(GetTileCenterPixels(3, 0), ComponentIds.BasicPropellant));
        AddStationAndSnap(new CoalResourceStation(GetTileCenterPixels(7, 0)));
        AddStationAndSnap(new Workbench(GetTileCenterPixels(1, 0)));
        AddStationAndSnap(new Workbench(GetTileCenterPixels(1, 4)));
        AddStationAndSnap(new Counter(GetTileCenterPixels(0, 0)));
        AddStationAndSnap(new Counter(GetTileCenterPixels(0, 4)));
        AddStationAndSnap(new Counter(GetTileCenterPixels(3, 4)));
    }

    private void AddStationAndSnap(AbstractStation station)
    {
        MapObjects.Add(station);
        SnapToNearestValidCell(station);
    }

    // Cannon wagon interior in tile coords relative to the train origin. These cells avoid the
    // wagon's left/right/top/bottom walls (which would otherwise push physics on snap/load).
    private const int WagonInteriorMinX = -3;
    private const int WagonInteriorMaxX = -2;
    private const int WagonInteriorMinY = -1;
    private const int WagonInteriorMaxY = 3;

    public void SnapToNearestValidCell(AbstractStation station)
    {
        Point gridPos = GetTileIndexFromPixels(station.Position);
        bool insideMainGrid = gridPos.X >= 0 && gridPos.X < Width
                                             && gridPos.Y >= 0 && gridPos.Y < Height;

        // If the station was dropped on the cannon wagon area, snap to the nearest *safe* wagon
        // interior cell so it doesn't end up overlapping a wagon wall (which would visibly shift
        // it once the next level loads and the walls are re-created).
        bool insideWagon = !insideMainGrid
                           && cannonWagon != null
                           && cannonWagon.GetBounds().Contains((int)station.Position.X, (int)station.Position.Y);
        if (insideWagon)
        {
            gridPos.X = Math.Clamp(gridPos.X, WagonInteriorMinX, WagonInteriorMaxX);
            gridPos.Y = Math.Clamp(gridPos.Y, WagonInteriorMinY, WagonInteriorMaxY);
        }
        else if (!insideMainGrid)
        {
            return;
        }

        Vector2 targetCenterMeters = GetTileCenterMeters(gridPos.X, gridPos.Y);
        station.PhysicsBody.Position = targetCenterMeters;
        var existingPair = stationGrid.FirstOrDefault(x => x.Value == station);
        if (existingPair.Value != null)
        {
            stationGrid.Remove(existingPair.Key);
        }

        stationGrid[gridPos] = station;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawTrainTiles(spriteBatch);
        DrawSideWalls(spriteBatch);
        foreach (var mapObject in MapObjects)
        {
            mapObject.Draw(spriteBatch);
        }
    }

    public void DrawSideWalls(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 offset = new Vector2(-scale * 0.9f * AssetManager.GetWallTexture("WallTileUpperSide").Width, 0);

        Texture2D upperLeftCornerTex = AssetManager.GetWallTexture("WallTileUpperLeftCorner");
        Vector2 upperLeftCornerOrigin = new Vector2(upperLeftCornerTex.Width / 2f, upperLeftCornerTex.Height);

        Vector2 ulTL = GetTileTopLeftPixels(0, 0) + offset + new Vector2(0, -tileSize * 2f);
        Vector2 ulFeet = ulTL + new Vector2(upperLeftCornerTex.Width / 2f * scale - 4f, upperLeftCornerTex.Height * scale);
        float ulDepth = RenderUtility.CalculateDepth(ulFeet.Y);

        spriteBatch.Draw(upperLeftCornerTex, ulFeet, null, Color.White,
            0f, upperLeftCornerOrigin, scale, SpriteEffects.None, ulDepth);

        int doorYMin = Height / 2 - 1;
        int doorYMax = Height / 2;

        for (int y = 0; y < Height; y++)
        {
            if (y >= doorYMin && y <= doorYMax) continue;

            Texture2D upperSideTex = AssetManager.GetWallTexture("WallTileUpperSide");
            Vector2 upperSideOrigin = new Vector2(upperSideTex.Width / 2f, upperSideTex.Height);

            Vector2 sideTL = GetTileTopLeftPixels(0, y) + new Vector2(0, -tileSize) + offset;
            Vector2 sideFeet = sideTL + new Vector2(upperSideTex.Width / 2f * scale, upperSideTex.Height * scale);
            float sideDepth = RenderUtility.CalculateDepth(sideFeet.Y) + RenderUtility.Eps;

            spriteBatch.Draw(upperSideTex, sideFeet, null, Color.White,
                0f, upperSideOrigin, scale, SpriteEffects.None, sideDepth);
        }
    }

    private void DrawTrainTiles(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Texture2D tileTex = AssetManager.TileTexture[tileTypes[x * Height + y]];
                Vector2 origin = new Vector2(tileTex.Width / 2f, tileTex.Height);
                Vector2 feetPosition = GetTileCenterPixels(x, y) + new Vector2(0, TileSize / 2f);

                spriteBatch.Draw(tileTex, feetPosition, null, Color.White,
                    0f, origin, scale, SpriteEffects.None, RenderUtility.FloorLayer);
            }
        }
    }

    public void Update(float dt)
    {
        foreach (var mapObject in MapObjects)
        {
            if (mapObject is IUpdatable station)
            {
                station.Update(dt);
            }
        }
    }

    public Rectangle GetBounds()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, Width * TileSize, Height * TileSize);
    }

    public bool IsOnTrain(Vector2 position)
    {
        Point p = new((int)position.X, (int)position.Y);
        if (GetBounds().Contains(p)) return true;
        return cannonWagon != null && cannonWagon.GetBounds().Contains(p);
    }

    public void Dispose()
    {
        foreach (IDisposable disposable in MapObjects.OfType<IDisposable>())
        {
            disposable.Dispose();
        }
    }
}