using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Serialization;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map.Train;

public readonly record struct DoorSpec(bool OnBottom, int Column);

public class TrainMap
{
    public int Width => GamelabGame.Instance.GameplayConfig.TrainWidth;
    public int Height => GamelabGame.Instance.GameplayConfig.TrainHeight;
    public int TileSize => GamelabGame.Instance.GameplayConfig.TrainTileSize;
    public Vector2 Position { get; private set; }
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly Random random = Random.Shared;

    public List<IPhysicalEntity> MapObjects { get; } = new();

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
        return new Point((int)(localPos.X / TileSize), (int)(localPos.Y / TileSize));
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

            string kindId = station.StationId;
            Point t = GetTileIndexFromPixels(station.Position);
            if (t.X >= 0 && t.X < Width && t.Y >= 0 && t.Y < Height)
            {
                list.Add(new StationSaveData(kindId, t.X, t.Y));
            }
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
            Vector2 center = GetTileCenterPixels(e.TileX, e.TileY);
            AbstractStation station = StationFactory.CreateStation(e.KindId, center);
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
            MapObjects.Add(new ShootHoleWall(wallSize, bottomPos, false));
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

    public void AddDefaultStructures()
    {
        Vector2 coalWagonPos = new Vector2(
            Position.X - 4 * TileSize,
            Position.Y + (Height * TileSize) / 2f);
        MapObjects.Add(new CoalWagon(coalWagonPos));
        MapObjects.Add(new TrainNose(GetTileCenterPixels(8, 2)));
    }

    public void AddDefaultStationLoadout()
    {
        MapObjects.Add(new SpeedLever(GetTileCenterPixels(7, 3)));
        MapObjects.Add(new CannonStation(GetTileCenterPixels(5, 2)));
        MapObjects.Add(new ComponentResource(GetTileCenterPixels(2, 0), "BasicCasing"));
        MapObjects.Add(new ComponentResource(GetTileCenterPixels(2, 4), "BasicProjectile"));
        MapObjects.Add(new ComponentResource(GetTileCenterPixels(3, 0), "BasicPropellant"));
        MapObjects.Add(new Workbench(GetTileCenterPixels(1, 0)));
        MapObjects.Add(new Workbench(GetTileCenterPixels(1, 4)));
        MapObjects.Add(new Counter(GetTileCenterPixels(0, 0)));
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
        Vector2 ulFeet = ulTL + new Vector2(upperLeftCornerTex.Width / 2f * scale, upperLeftCornerTex.Height * scale);
        float ulDepth = RenderUtility.CalculateDepth(ulFeet.Y);

        spriteBatch.Draw(upperLeftCornerTex, ulFeet, null, Color.White,
            0f, upperLeftCornerOrigin, scale, SpriteEffects.None, ulDepth);

        for (int y = 0; y < Height; y++)
        {
            if (y == 2) continue;

            Texture2D upperSideTex = AssetManager.GetWallTexture("WallTileUpperSide");
            Vector2 upperSideOrigin = new Vector2(upperSideTex.Width / 2f, upperSideTex.Height);

            Vector2 sideTL = GetTileTopLeftPixels(0, y) + new Vector2(0, -tileSize) + offset;
            Vector2 sideFeet = sideTL + new Vector2(upperSideTex.Width / 2f * scale, upperSideTex.Height * scale);
            float sideDepth = RenderUtility.CalculateDepth(sideFeet.Y) + RenderUtility.Eps;

            spriteBatch.Draw(upperSideTex, sideFeet, null, Color.White,
                0f, upperSideOrigin, scale, SpriteEffects.None, sideDepth);
        }

        Texture2D wallTileTopTex = AssetManager.GetWallTexture("WallTileTop");
        Vector2 wallTileTopOrigin = new Vector2(wallTileTopTex.Width / 2f, wallTileTopTex.Height);

        for (int bridgeX = -1; bridgeX >= -2; bridgeX--)
        {
            Vector2 topBridgeTL = GetTileTopLeftPixels(bridgeX, 2) + new Vector2(0, -tileSize * 2f);
            Vector2 topBridgeFeet =
                topBridgeTL + new Vector2(wallTileTopTex.Width / 2f * scale, wallTileTopTex.Height * scale);
            float topBridgeDepth = RenderUtility.CalculateDepth(topBridgeFeet.Y);

            spriteBatch.Draw(wallTileTopTex, topBridgeFeet, null, Color.White,
                0f, wallTileTopOrigin, scale, SpriteEffects.None, topBridgeDepth);
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

        Texture2D bridgeTex = AssetManager.TileTexture[0];
        Vector2 bridgeOrigin = new Vector2(bridgeTex.Width / 2f, bridgeTex.Height);

        for (int bridgeX = -1; bridgeX >= -2; bridgeX--)
        {
            Vector2 bottomCenter = GetTileCenterPixels(bridgeX, 2) + new Vector2(0, TileSize / 2f);

            spriteBatch.Draw(bridgeTex, bottomCenter, null, Color.White,
                0f, bridgeOrigin, scale, SpriteEffects.None, RenderUtility.FloorLayer);
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

    public void Dispose()
    {
    }
}