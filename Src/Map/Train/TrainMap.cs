using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Events;
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
    private GameEvents GameEvents { get; }

    public TrainMap(GraphicsDevice graphicsDevice, World world, GameEvents gameEvents, Point virtualScreenSize)
    {
        PhysicsWorld = world;
        this.GameEvents = gameEvents;
        Position = new Vector2(
            (virtualScreenSize.X - Width * TileSize) / 2f,
            (virtualScreenSize.Y - Height * TileSize) / 2f
        );
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

        float trainHeightPixels = Height * TileSize;
        float endWallWidthPixels = wallHeightPixels;
        float endWallSimWidth = endWallWidthPixels.ToMeters();
        float endWallSimHeight = trainHeightPixels.ToMeters();

        for (int x = 0; x < Width; x++)
        {
            float centerX = GetTileCenterPixels(x, 0).X;

            // --- TOP WALL ---
            Vector2 topPixelPos = new Vector2(centerX, Position.Y - wallHeightPixels / 2f);
            var topWall = new ShootHoleWall(PhysicsWorld, GameEvents, wallDimensions, topPixelPos);
            MapObjects.Add(topWall);

            // --- BOTTOM WALL ---
            Vector2 bottomPixelPos = new Vector2(centerX, Position.Y + (Height * TileSize) + wallHeightPixels / 2f);
            var bottomWall = new ShootHoleWall(PhysicsWorld, GameEvents, wallDimensions, bottomPixelPos);
            MapObjects.Add(bottomWall);
        }

        Vector2 backWallPosition = new Vector2(
            Position.X - endWallWidthPixels / 2f,
            Position.Y + trainHeightPixels / 2f
        );
        Body backWallBody = PhysicsWorld.CreateRectangle(
            endWallSimWidth,
            endWallSimHeight,
            1f,
            backWallPosition.ToMeters(),
            0f,
            BodyType.Static
        );

        Vector2 frontWallPosition = new Vector2(
            Position.X + Width * TileSize + endWallWidthPixels / 2f,
            Position.Y + trainHeightPixels / 2f
        );
        Body frontWallBody = PhysicsWorld.CreateRectangle(
            endWallSimWidth,
            endWallSimHeight,
            1f,
            frontWallPosition.ToMeters(),
            0f,
            BodyType.Static
        );
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
        DrawTrainNose(spriteBatch);
        DrawTrainTiles(spriteBatch);
        foreach (var mapObject in MapObjects)
        {
            mapObject.Draw(spriteBatch);
        }
    }

    private void DrawTrainNose(SpriteBatch spriteBatch)
    {
        int trainHeightPixels = Height * TileSize;
        float noseLength = TileSize * 1.5f;
        int sliceHeight = 2;
        int numSlices = trainHeightPixels / sliceHeight;
        float trainRightEdge = Position.X + Width * TileSize;

        for (int i = 0; i < numSlices; i++)
        {
            float t = MathHelper.Distance(i, numSlices / 2f) / (numSlices / 2f);
            float sliceWidth = noseLength * (1f - t);

            if (sliceWidth < 1f)
            {
                continue;
            }

            Rectangle slice = new Rectangle(
                (int)trainRightEdge,
                (int)(Position.Y + i * sliceHeight),
                (int)sliceWidth,
                sliceHeight
            );
            spriteBatch.Draw(AssetManager.BlankTexture, slice, Color.Gray);
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
    }

    public void Update(float dt, GameplayContext context)
    {
        foreach (var mapObject in MapObjects)
        {
            if (mapObject is AbstractStation station)
            {
                station.Update(dt, context);
            }
        }
    }

    public Rectangle GetBounds()
    {
        return new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            Width * TileSize,
            Height * TileSize
        );
    }

    public void Dispose()
    {
    }
}