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
    private readonly List<Point> allTiles;
    private readonly Random random = Random.Shared;

    private float snowSpawnTimer;
    private float snowMeltTimer;
    private float iceSpawnTimer;
    private float iceMeltTimer;

    private static readonly Color SnowColor = new Color(178, 198, 218, 180);
    private static readonly Color IceColor = new Color(100, 180, 255, 180);
    private const float DrawDepth = RenderUtility.FloorLayer + 2 * RenderUtility.Eps;

    public IReadOnlySet<Point> SnowTiles => snowTiles;
    public IReadOnlySet<Point> IceTiles => iceTiles;

    public PatchManager(TrainMap map)
    {
        allTiles = new List<Point>(map.Width * map.Height);
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                allTiles.Add(new Point(x, y));
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
            if (!snowTiles.Contains(candidate) && !iceTiles.Contains(candidate))
            {
                targetSet.Add(candidate);
                return;
            }
        }
    }

    private void TryMeltOneFrom(HashSet<Point> tiles)
    {
        if (tiles.Count == 0) return;
        int idx = random.Next(tiles.Count);
        Point toRemove = default;
        int i = 0;
        foreach (Point tile in tiles) { if (i++ == idx) { toRemove = tile; break; } }
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
            DrawTile(spriteBatch, map, tile, SnowColor);
        foreach (Point tile in iceTiles)
            DrawTile(spriteBatch, map, tile, IceColor);
    }

    private static void DrawTile(SpriteBatch spriteBatch, TrainMap map, Point tile, Color color)
    {
        Vector2 topLeft = map.GetTileTopLeftPixels(tile.X, tile.Y);
        Rectangle rect = new((int)topLeft.X, (int)topLeft.Y, map.TileSize, map.TileSize);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, null, color,
            0f, Vector2.Zero, SpriteEffects.None, DrawDepth);
    }

    public void FillIce()
    {
        float maxCoverage = GamelabGame.Instance.GameplayConfig.IceMaxCoverage;
        int target = (int)(allTiles.Count * maxCoverage);
        while (iceTiles.Count < target)
            TrySpawnInto(iceTiles);
    }

    public void Clear()
    {
        snowTiles.Clear();
        iceTiles.Clear();
    }
}
