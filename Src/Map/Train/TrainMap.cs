using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map.Train;

public class TrainMap
{
    public int Width => GamelabGame.Instance.GameplayConfig.TrainWidth;
    public int Height => GamelabGame.Instance.GameplayConfig.TrainHeight;
    public int TileSize => GamelabGame.Instance.GameplayConfig.TrainTileSize;
    public Vector2 Position { get; private set; }
    public World PhysicsWorld { get; private set; }
    public List<IPhysicalEntity> MapObjects { get; } = new();

    public TrainMap(GraphicsDevice graphicsDevice, World world)
    {
        PhysicsWorld = world;
    }

    public void Initialize(Vector2 position)
    {
        Position = position;
        InitializeBoundaryWalls();
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

    public void InitializeBoundaryWalls()
    {
        float wallHeightPixels = TileSize / 2f;
        Vector2 wallDimensions = new Vector2(TileSize, wallHeightPixels);

        float simWidth = wallDimensions.X.ToMeters();
        float simHeight = wallDimensions.Y.ToMeters();

        for (int x = 0; x < Width; x++)
        {
            float centerX = GetTileCenterPixels(x, 0).X;

            // --- TOP WALL ---
            var topWall = new ShootHoleWall(wallDimensions);
            Vector2 topPixelPos = new Vector2(centerX, Position.Y - wallHeightPixels / 2f);
            Body topBody =
                PhysicsWorld.CreateRectangle(simWidth, simHeight, 1f, topPixelPos.ToMeters(), 0f, BodyType.Static);
            topWall.AttachPhysics(topBody);
            MapObjects.Add(topWall);

            // --- BOTTOM WALL ---
            var bottomWall = new ShootHoleWall(wallDimensions);
            Vector2 bottomPixelPos = new Vector2(centerX, Position.Y + (Height * TileSize) + wallHeightPixels / 2f);
            Body bottomBody = PhysicsWorld.CreateRectangle(simWidth, simHeight, 1f, bottomPixelPos.ToMeters(), 0f,
                BodyType.Static);
            bottomWall.AttachPhysics(bottomBody);
            MapObjects.Add(bottomWall);
        }
    }

    public void SnapToNearestValidCell(AbstractStation station)
    {
        Point gridPos = GetTileIndexFromPixels(station.PhysicsBody.Position.ToPixels());

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
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2 drawPos = GetTileTopLeftPixels(x, y);
                spriteBatch.Draw(AssetManager.TileTexture, drawPos, Color.White);
            }
        }

        foreach (var mapObject in MapObjects)
        {
            mapObject.Draw(spriteBatch);
        }
    }

    public void Update(float dt, TrainContext context)
    {
        foreach (var mapObject in MapObjects)
        {
            if (mapObject is AbstractStation station)
            {
                station.Update(dt, context);
            }
        }
    }
}