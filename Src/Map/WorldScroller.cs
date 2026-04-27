using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public class WorldScroller
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    private readonly List<Vector2> tilePositions = new();
    private readonly List<int> tileTypes = new();
    private readonly Random random = Random.Shared;

    private int ScreenWidth => gameplayContext.ScreenWidth;
    private int ScreenHeight => gameplayContext.ScreenHeight;

    private int TileWidth => AssetManager.TrainTrackTexture[0].Width;
    private float centerY;

    private readonly record struct ScrollerTree(Vector2 TrunkBase, float Scale);

    private const string PineDecorationKey = "Snow_Covered_Pine";
    private readonly List<ScrollerTree> trees = new();
    private const float TreeBaseScale = 0.22f;
    private const float TreeScaleVariance = 0.08f;
    private const float TreeHorizontalSpacingBase = 360f;
    private const float TreeHorizontalSpacingVariance = 180f;
    private const float MinTrunkBaseClearanceAboveTracks = 60f;
    private const float TrunkBaseOffsetBelowRailStrip = 300f;
    private const float UpperTrunkBasePlacementBandHeight = 140f;
    private const float LowerTrunkBasePlacementBandHeight = 140f;

    public WorldScroller()
    {
        centerY = (gameplayContext.ScreenHeight / 2.0f) - (AssetManager.TrainTrackTexture[0].Height / 2.0f);
        int numTilesNeeded = (gameplayContext.ScreenWidth / TileWidth) + 3;
        for (int i = 0; i < numTilesNeeded; i++)
        {
            SpawnTile(i * TileWidth);
        }

        SpawnInitialTrees();
    }

    private void SpawnTile(float xOffset)
    {
        tilePositions.Add(new Vector2(xOffset, centerY));
        tileTypes.Add(random.Next(0, 2));
    }

    private float MaxVisibleWidth => gameplayContext.ScreenWidth / GamelabGame.Instance.GameplayConfig.CameraMinZoom;

    private void SpawnInitialTrees()
    {
        float xCursorTop = -TreeHorizontalSpacingBase;
        float xCursorBottom = -TreeHorizontalSpacingBase * 0.5f;
        float endX = MaxVisibleWidth + TreeHorizontalSpacingBase;

        while (xCursorTop < endX)
        {
            SpawnTree(xCursorTop, placeTrunkBaseAboveTracks: true);
            xCursorTop += NextTreeSpacing();
        }

        while (xCursorBottom < endX)
        {
            SpawnTree(xCursorBottom, placeTrunkBaseAboveTracks: false);
            xCursorBottom += NextTreeSpacing();
        }
    }

    private float NextTreeSpacing()
    {
        return TreeHorizontalSpacingBase + (float)random.NextDouble() * TreeHorizontalSpacingVariance;
    }

    private void SpawnTree(float x, bool placeTrunkBaseAboveTracks)
    {
        float railTopY = centerY;
        float railBottomY = centerY + AssetManager.TrainTrackTexture[0].Height;
        float scale = TreeBaseScale + ((float)random.NextDouble() - 0.5f) * TreeScaleVariance;
        float scaledTreeHeight = AssetManager.GetDecorationTexture(PineDecorationKey).Height * scale;

        float trunkBaseY = placeTrunkBaseAboveTracks
            ? RandomTrunkBaseYAboveTheRailStrip(railTopY, scaledTreeHeight)
            : RandomTrunkBaseYBelowTheRailStrip(railBottomY);

        trees.Add(new ScrollerTree(new Vector2(x, trunkBaseY), scale));
    }

    private float RandomTrunkBaseYAboveTheRailStrip(float railTopY, float scaledTreeHeight)
    {
        float topOfTreeY = MathHelper.Lerp(0f, UpperTrunkBasePlacementBandHeight, (float)random.NextDouble());
        float trunkBaseY = topOfTreeY + scaledTreeHeight;
        float maxTrunkBaseYBeforeTrackZone = railTopY - MinTrunkBaseClearanceAboveTracks;
        return Math.Min(trunkBaseY, maxTrunkBaseYBeforeTrackZone);
    }

    private float RandomTrunkBaseYBelowTheRailStrip(float railBottomY)
    {
        float bandTopY = railBottomY + TrunkBaseOffsetBelowRailStrip;
        float bandBottomY = MathF.Min(ScreenHeight, bandTopY + LowerTrunkBasePlacementBandHeight);
        return MathHelper.Lerp(bandTopY, bandBottomY, (float)random.NextDouble());
    }

    public void Update(float deltaTime)
    {
        float speed = gameplayContext.State.actualSpeed;

        for (int i = 0; i < tilePositions.Count; i++)
        {
            tilePositions[i] = new Vector2(tilePositions[i].X - (speed * deltaTime), centerY + 20);
        }

        if (tilePositions.Count > 0 && tilePositions[0].X < -TileWidth)
        {
            float lastX = tilePositions[tilePositions.Count - 1].X;
            tilePositions.RemoveAt(0);
            tileTypes.RemoveAt(0);
            SpawnTile(lastX + TileWidth);
        }

        UpdateTrees(deltaTime, speed);
    }

    private void UpdateTrees(float deltaTime, float speed)
    {
        Texture2D treeTex = AssetManager.GetDecorationTexture(PineDecorationKey);
        float despawnWhenPastThisLeftEdge = -treeTex.Width * 1.5f;

        for (int i = 0; i < trees.Count; i++)
        {
            ScrollerTree t = trees[i];
            trees[i] = t with { TrunkBase = new Vector2(t.TrunkBase.X - speed * deltaTime, t.TrunkBase.Y) };
        }

        for (int i = trees.Count - 1; i >= 0; i--)
        {
            if (trees[i].TrunkBase.X >= despawnWhenPastThisLeftEdge) continue;

            bool placeInUpperBand = trees[i].TrunkBase.Y < centerY;
            float rightmostXInSameBand = MaxVisibleWidth;
            for (int j = 0; j < trees.Count; j++)
            {
                bool jIsUpperBand = trees[j].TrunkBase.Y < centerY;
                if (jIsUpperBand == placeInUpperBand && trees[j].TrunkBase.X > rightmostXInSameBand)
                    rightmostXInSameBand = trees[j].TrunkBase.X;
            }

            float newX = rightmostXInSameBand + NextTreeSpacing();
            trees.RemoveAt(i);
            SpawnTree(newX, placeTrunkBaseAboveTracks: placeInUpperBand);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawSnowBackdrop(spriteBatch);

        for (int i = 0; i < tilePositions.Count; i++)
        {
            spriteBatch.Draw(AssetManager.TrainTrackTexture[tileTypes[i]], tilePositions[i], null, Color.White, 0f,
                Vector2.Zero, 1f, SpriteEffects.None, RenderUtility.BackgroundLayer);
        }

        DrawTrees(spriteBatch);
    }

    private void DrawSnowBackdrop(SpriteBatch spriteBatch)
    {
        float minZoom = GamelabGame.Instance.GameplayConfig.CameraMinZoom;
        int padX = (int)MathF.Ceiling(ScreenWidth / minZoom);
        int padY = (int)MathF.Ceiling(ScreenHeight / minZoom);
        Rectangle area = new Rectangle(-padX, -padY, ScreenWidth + padX * 2, ScreenHeight + padY * 2);
        spriteBatch.Draw(AssetManager.BlankTexture, area, RenderUtility.SnowBackgroundColor);
    }

    private void DrawTrees(SpriteBatch spriteBatch)
    {
        Texture2D treeTex = AssetManager.GetDecorationTexture(PineDecorationKey);
        Vector2 origin = new Vector2(treeTex.Width / 2f, treeTex.Height);
        const float treeLayerDepth = RenderUtility.BackgroundLayer + RenderUtility.Eps;

        foreach (ScrollerTree t in trees)
        {
            spriteBatch.Draw(treeTex, t.TrunkBase, null, Color.White, 0f, origin, t.Scale,
                SpriteEffects.None, treeLayerDepth);
        }
    }

    public void Dispose()
    {
    }
}
