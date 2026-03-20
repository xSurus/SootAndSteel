using System.Collections.Generic;
using Gamelab.Interactable.Stations;
using Gamelab.Interactable.Structures;
using Gamelab.Map.Train.State;
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
    private TileCell[,] grid;
    private Vector2 trainPosition;
    private Texture2D tileTexture;
    private World physicsWorld;
    private readonly List<ShootHoleWall> boundaryWalls = new();

    public TrainMap(GraphicsDevice graphicsDevice, World world)
    {
        physicsWorld = world;

        tileTexture = new Texture2D(graphicsDevice, TileSize, TileSize);
        Color[] data = new Color[TileSize * TileSize];
        for (int i = 0; i < data.Length; i++)
        {
            int x = i % TileSize;
            int y = i / TileSize;
            bool isBorder = x == 0 || y == 0 || x == TileSize - 1 || y == TileSize - 1;
            data[i] = isBorder ? Color.DarkGray : Color.Gray;
        }

        tileTexture.SetData(data);

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new TileCell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2 worldPos = new Vector2(x * TileSize, y * TileSize);
                grid[x, y] = new TileCell(x, y, worldPos, physicsWorld, TileSize);
            }
        }
    }

    public void SetPosition(Vector2 position)
    {
        trainPosition = position;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2 worldPos = trainPosition + new Vector2(x * TileSize, y * TileSize);
                grid[x, y].UpdateWorldPosition(worldPos);
            }
        }

        InitializeBoundaryWalls();
    }

    public void InitializeBoundaryWalls()
    {
        float wallHeightPixels = TileSize / 2f;
        Vector2 wallDimensions = new Vector2(TileSize, wallHeightPixels);

        float simWidth = wallDimensions.X.ToMeters();
        float simHeight = wallDimensions.Y.ToMeters();

        for (int x = 0; x < Width; x++)
        {
            // --- TOP WALL ---
            var topWall = new ShootHoleWall(wallDimensions);
            Vector2 topPixelPos = trainPosition + new Vector2(x * TileSize + TileSize / 2f, -wallHeightPixels / 2f);

            Body topBody =
                physicsWorld.CreateRectangle(simWidth, simHeight, 1f, topPixelPos.ToMeters(), 0f, BodyType.Static);
            topWall.AttachPhysics(topBody);
            boundaryWalls.Add(topWall);

            // --- BOTTOM WALL ---
            var bottomWall = new ShootHoleWall(wallDimensions);
            Vector2 bottomPixelPos = trainPosition +
                                     new Vector2(x * TileSize + TileSize / 2f,
                                         (Height * TileSize) + wallHeightPixels / 2f);

            Body bottomBody = physicsWorld.CreateRectangle(simWidth, simHeight, 1f, bottomPixelPos.ToMeters(), 0f,
                BodyType.Static);
            bottomWall.AttachPhysics(bottomBody);
            boundaryWalls.Add(bottomWall);
        }
    }

    public TileCell GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return grid[x, y];
    }

    public void PlaceObject(int x, int y, AbstractStation abstractStation)
    {
        var tile = GetTile(x, y);
        tile?.SetObject(abstractStation);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].Draw(spriteBatch, tileTexture);
            }
        }

        foreach (var wall in boundaryWalls)
        {
            wall.Draw(spriteBatch);
        }
    }

    public void Update(float dt, TrainContext context)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].AbstractStation?.Update(dt, context);
            }
        }
    }

    public void Dispose()
    {
        tileTexture?.Dispose();
    }
}