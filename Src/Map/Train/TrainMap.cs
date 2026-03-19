using System.Collections.Generic;
using Gamelab.Interactable.Stations;
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
    private List<Body> boundaryWalls = [];

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

        BuildTrainBoundaries();
    }

    private void BuildTrainBoundaries()
    {
        foreach (Body wall in boundaryWalls)
        {
            physicsWorld.Remove(wall);
        }

        boundaryWalls.Clear();

        float simLeft = trainPosition.X.ToMeters();
        float simTop = trainPosition.Y.ToMeters();
        float simRight = (trainPosition.X + (Width * TileSize)).ToMeters();
        float simBottom = (trainPosition.Y + (Height * TileSize)).ToMeters();

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

    public void PlaceObject(int x, int y, AbstractStation abstractStation)
    {
        var tile = GetTile(x, y);
        tile?.SetObject(abstractStation);
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

    public void Update(float deltaTime, TrainContext context)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].AbstractStation?.Update(deltaTime, context);
            }
        }
    }

    public void Dispose()
    {
        tileTexture?.Dispose();
    }
}