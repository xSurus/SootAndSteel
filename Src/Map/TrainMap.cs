using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map;

public class TrainMap
{
    private int Width { get; }
    private int Height { get; }
    public int TileSize { get; }

    private TileCell[,] grid;
    private Vector2 trainPosition;
    private Texture2D tileTexture;

    // Physics dependencies
    private readonly World physicsWorld;
    private float pixelsPerMeter;
    private List<Body> boundaryWalls = [];

    public TrainMap(int width, int height, int tileSize, GraphicsDevice graphicsDevice, World world,
        float pixelsPerMeter)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        physicsWorld = world;
        this.pixelsPerMeter = pixelsPerMeter;

        // Create a simple tile texture with a border effect
        tileTexture = new Texture2D(graphicsDevice, tileSize, tileSize);
        Color[] data = new Color[tileSize * tileSize];
        for (int i = 0; i < data.Length; i++)
        {
            int x = i % tileSize;
            int y = i / tileSize;
            bool isBorder = x == 0 || y == 0 || x == tileSize - 1 || y == tileSize - 1;
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
                grid[x, y] = new TileCell(x, y, worldPos, physicsWorld, pixelsPerMeter, TileSize);
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

        BuildTrainBoundaries();
    }

    private void BuildTrainBoundaries()
    {
        foreach (Body wall in boundaryWalls)
        {
            physicsWorld.Remove(wall);
        }

        boundaryWalls.Clear();

        float simLeft = trainPosition.X / pixelsPerMeter;
        float simTop = trainPosition.Y / pixelsPerMeter;
        float simRight = (trainPosition.X + (Width * TileSize)) / pixelsPerMeter;
        float simBottom = (trainPosition.Y + (Height * TileSize)) / pixelsPerMeter;

        var topWall = physicsWorld.CreateEdge(new Vector2(simLeft, simTop), new Vector2(simRight, simTop));
        var bottomWall = physicsWorld.CreateEdge(new Vector2(simLeft, simBottom), new Vector2(simRight, simBottom));
        var leftWall = physicsWorld.CreateEdge(new Vector2(simLeft, simTop), new Vector2(simLeft, simBottom));
        var rightWall = physicsWorld.CreateEdge(new Vector2(simRight, simTop), new Vector2(simRight, simBottom));

        boundaryWalls.AddRange([topWall, bottomWall, leftWall, rightWall]);
    }

    public TileCell GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return grid[x, y];
    }

    public void PlaceObject(int x, int y, TileObject tileObject)
    {
        var tile = GetTile(x, y);
        tile?.SetObject(tileObject);
    }

    public Rectangle GetBounds()
    {
        return new Rectangle(
            (int)trainPosition.X,
            (int)trainPosition.Y,
            Width * TileSize,
            Height * TileSize
        );
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
    }

    public void Update(float deltaTime)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].TileObject?.Update(deltaTime);
            }
        }
    }

    public void Dispose()
    {
        tileTexture?.Dispose();
    }
}