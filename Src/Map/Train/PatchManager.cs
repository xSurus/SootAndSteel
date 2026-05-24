using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map.Train;

/// <summary>
/// Manages snow and ice patches on train tiles.
/// Snow patches slow the player; ice patches make them slip (reduced damping + lerp authority).
/// Both types spawn proportionally as temperature drops below their respective thresholds.
/// </summary>
public class PatchManager
{
    private readonly HashSet<Point> snowTiles = new();
    private readonly HashSet<Point> iceTiles = new();
    private readonly TrainMap map;
    private readonly List<Point> allTiles;
    private readonly Random random = Random.Shared;

    private float snowSpawnTimer;
    private float snowMeltTimer;
    private float iceSpawnTimer;
    private float iceMeltTimer;

    private static readonly Color PatchTint = Color.White * 0.7f;
    private const float DrawDepth = RenderUtility.FloorLayer + 1.5f * RenderUtility.Eps;

    public PatchManager(TrainMap map)
    {
        this.map = map;
        allTiles = new List<Point>(map.Width * map.Height);
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                allTiles.Add(new Point(x, y));
            }
        }
    }

    private int TargetCount(float tempRatio, float threshold, float maxCoverage)
    {
        if (tempRatio >= threshold) return 0;
        float coldnessFraction = 1f - tempRatio / threshold;
        return (int)(allTiles.Count * maxCoverage * coldnessFraction);
    }

    public void Update(float dt, TrainState state)
    {
        var cfg = GamelabGame.Instance.GameplayConfig;
        float tempRatio = state.Temperature / cfg.TrainMaxTemperature;

        UpdatePatchType(dt, tempRatio,
            cfg.SnowStartThreshold, cfg.SnowMaxCoverage,
            cfg.SnowSpawnIntervalSeconds, cfg.SnowMeltIntervalSeconds,
            snowTiles, ref snowSpawnTimer, ref snowMeltTimer);

        UpdatePatchType(dt, tempRatio,
            cfg.IceStartThreshold, cfg.IceMaxCoverage,
            cfg.IceSpawnIntervalSeconds, cfg.IceMeltIntervalSeconds,
            iceTiles, ref iceSpawnTimer, ref iceMeltTimer);
    }

    private void UpdatePatchType(
        float dt, float tempRatio,
        float threshold, float maxCoverage,
        float spawnInterval, float meltInterval,
        HashSet<Point> tiles,
        ref float spawnTimer, ref float meltTimer)
    {
        int target = TargetCount(tempRatio, threshold, maxCoverage);

        if (tiles.Count < target)
        {
            spawnTimer += dt;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnInto(tiles);
            }
        }
        else if (tiles.Count > target)
        {
            meltTimer += dt;
            if (meltTimer >= meltInterval)
            {
                meltTimer = 0f;
                TryMeltOneFrom(tiles);
            }
        }
        else
        {
            spawnTimer = 0f;
            meltTimer = 0f;
        }
    }

    private void TrySpawnInto(HashSet<Point> targetSet)
    {
        if (targetSet.Count >= allTiles.Count) return;
        // Expected attempts ≈ 1 / (1 - combined coverage) ≤ ~3 at max coverage.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Point candidate = allTiles[random.Next(allTiles.Count)];
            if (snowTiles.Contains(candidate) || iceTiles.Contains(candidate) || IsOccupied(candidate))
                continue;

            targetSet.Add(candidate);
            return;
        }
    }

    private bool IsOccupied(Point tile)
    {
        foreach (var mapObject in map.MapObjects)
        {
            Point occupiedTile = map.GetTileIndexFromPixels(mapObject.Position);
            if (occupiedTile == tile) return true;
        }

        return false;
    }

    private void TryMeltOneFrom(HashSet<Point> tiles)
    {
        if (tiles.Count == 0) return;
        int idx = random.Next(tiles.Count);
        Point toRemove = default;
        int i = 0;
        foreach (Point tile in tiles)
        {
            if (i++ == idx)
            {
                toRemove = tile;
                break;
            }
        }

        tiles.Remove(toRemove);
    }

    public bool IsOnSnow(Vector2 pixelPos, TrainMap map) =>
        snowTiles.Contains(map.GetTileIndexFromPixels(pixelPos));

    public bool IsOnIce(Vector2 pixelPos, TrainMap map) =>
        iceTiles.Contains(map.GetTileIndexFromPixels(pixelPos));

    /// <summary>Removes whichever patch type is under the given position. Returns true if anything was removed.</summary>
    public bool TryRemovePatchAt(Vector2 pixelPos, TrainMap map)
    {
        Point tile = map.GetTileIndexFromPixels(pixelPos);
        return snowTiles.Remove(tile) || iceTiles.Remove(tile);
    }

    public void Draw(SpriteBatch spriteBatch, TrainMap map)
    {
        foreach (Point tile in snowTiles)
            DrawTile(spriteBatch, map, tile, AssetManager.SnowPatchTexture);
        foreach (Point tile in iceTiles)
            DrawTile(spriteBatch, map, tile, AssetManager.IcePatchTexture);
    }

    public void DrawLightBatch(SpriteBatch spriteBatch, TrainMap map)
    {
        foreach (Point tile in snowTiles)
            DrawTile2(spriteBatch, map, tile, AssetManager.GetDecorationTexture("IceFloor"));
        foreach (Point tile in iceTiles)
            DrawTile2(spriteBatch, map, tile, AssetManager.GetDecorationTexture("IceFloor"));
    }

    private static void DrawTile(SpriteBatch spriteBatch, TrainMap map, Point tile, Texture2D texture)
    {
        Vector2 topLeft = map.GetTileTopLeftPixels(tile.X, tile.Y);
        Vector2 scale = new Vector2(map.TileSize / (float)texture.Width, map.TileSize / (float)texture.Height);
        spriteBatch.Draw(texture, topLeft, null, PatchTint,
            0f, Vector2.Zero, scale, SpriteEffects.None, DrawDepth);
    }

    private static void DrawTile2(SpriteBatch spriteBatch, TrainMap map, Point tile, Texture2D texture)
    {
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        Vector2 tileCenter = map.GetTileTopLeftPixels(tile.X, tile.Y) + new Vector2(map.TileSize / 2f);
        float tileScale = map.TileSize / 1000f;

        spriteBatch.Draw(texture, tileCenter, null, Color.White * 0.7f,
            0f, origin, 1.5f * tileScale, SpriteEffects.None, DrawDepth);
    }
}