using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Stations.Workbenches;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map.Train;

public class TrainMap
{
    public int Width => GamelabGame.Instance.GameplayConfig.TrainWidth;
    public int Height => GamelabGame.Instance.GameplayConfig.TrainHeight;
    public int TileSize => GamelabGame.Instance.GameplayConfig.TrainTileSize;
    public Vector2 Position { get; private set; }
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    public List<IPhysicalEntity> MapObjects { get; } = new();

    public TrainMap() : this(ComputeDefaultTopLeftPixels(), spawnBreakableBottomEdge: true, -1, -1)
    {
    }

    /// <param name="spawnBreakableBottomEdge">When false, skips all per-tile bottom <see cref="ShootHoleWall"/>s.</param>
    /// <param name="bottomWallOmitStartTileX">If &gt;= 0 and &lt; <paramref name="bottomWallOmitEndTileXExclusive"/>, those tile columns have no bottom wall (hub hatch gap).</param>
    public TrainMap(
        Vector2 topLeftPixels,
        bool spawnBreakableBottomEdge = true,
        int bottomWallOmitStartTileX = -1,
        int bottomWallOmitEndTileXExclusive = -1)
    {
        Position = topLeftPixels;
        InitializeBoundaryWalls(spawnBreakableBottomEdge, bottomWallOmitStartTileX, bottomWallOmitEndTileXExclusive);
    }

    private static Vector2 ComputeDefaultTopLeftPixels()
    {
        GameplayContext ctx = GamelabGame.Instance.Services.GetService<GameplayContext>()
            ?? throw new InvalidOperationException("GameplayContext must exist before TrainMap is created.");
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
        return new Point((int)(localPos.X / TileSize), (int)(localPos.Y / TileSize));
    }
    // TODO temporary change to enable gap in wall for players to enter/exit
    public void InitializeBoundaryWalls(
        bool spawnBreakableBottomEdge = true,
        int bottomWallOmitStartTileX = -1,
        int bottomWallOmitEndTileXExclusive = -1)
    {
        float halfTile = TileSize / 2f;
        Vector2 horizontalWallSize = new Vector2(TileSize, halfTile);

        bool omitBottomBand = bottomWallOmitStartTileX >= 0
                              && bottomWallOmitEndTileXExclusive > bottomWallOmitStartTileX;

        for (int x = 0; x < Width; x++)
        {
            // top walls
            Vector2 topPos = GetTileCenterPixels(x, 0) - new Vector2(0, halfTile + halfTile / 2f);
            MapObjects.Add(new ShootHoleWall(horizontalWallSize, topPos));

            // bottom walls
            Vector2 bottomPos = GetTileCenterPixels(x, Height - 1) + new Vector2(0, halfTile + halfTile / 2f);
            MapObjects.Add(new ShootHoleWall(horizontalWallSize, bottomPos));
        }

        // blockers above and below the bridge
        Point[] bridgeBlocker = [new Point(-1, 0), new Point(-1, 1), new Point(-1, 3), new Point(-1, 4)];
        float tileSimSize = TileSize.ToMeters();

        foreach (var tile in bridgeBlocker)
        {
            Vector2 wallCenterMeters = GetTileCenterMeters(tile.X, tile.Y);
            gameplayContext.PhysicsWorld.CreateRectangle(tileSimSize, tileSimSize, 1f, wallCenterMeters);
        }
    }

    public void AddDefaultStationLoadout()
    {
        MapObjects.Add(new SpeedLever(GetTileCenterPixels(7, 3)));
        MapObjects.Add(new CannonStation(GetTileCenterPixels(5, 2)));
        MapObjects.Add(new CopperResource(GetTileCenterPixels(2, 0)));
        MapObjects.Add(new GunpowderResource(GetTileCenterPixels(2, 4)));
        MapObjects.Add(new Anvil(GetTileCenterPixels(1, 0)));
        MapObjects.Add(new Anvil(GetTileCenterPixels(1, 4)));
        MapObjects.Add(new Counter(GetTileCenterPixels(0, 0)));
        MapObjects.Add(new Counter(GetTileCenterPixels(3, 0)));
        MapObjects.Add(new Counter(GetTileCenterPixels(0, 4)));
        MapObjects.Add(new Counter(GetTileCenterPixels(3, 4)));
    }

    public void SnapToNearestValidCell(AbstractStation station)
    {
        Point gridPos = GetTileIndexFromPixels(station.Position);

        if (gridPos.X >= 0 && gridPos.X < Width && gridPos.Y >= 0 && gridPos.Y < Height)
        {
            Vector2 targetCenterMeters = GetTileCenterMeters(gridPos.X, gridPos.Y);
            station.PhysicsBody.Position = targetCenterMeters;

            if (station is CannonStation cannon)
            {
                cannon.AimingBar.PhysicsBody.Position = targetCenterMeters;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawTrainTiles(spriteBatch);
        foreach (var mapObject in MapObjects)
        {
            mapObject.Draw(spriteBatch);
        }
    }

    private void DrawTrainTiles(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2 drawPos = GetTileTopLeftPixels(x, y);
                spriteBatch.Draw(AssetManager.TileTexture, drawPos, Color.White);
            }
        }

        Vector2 bridgePos = GetTileTopLeftPixels(-1, 2);
        spriteBatch.Draw(AssetManager.TileTexture, bridgePos, Color.White);
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

    public void Dispose()
    {
    }
}