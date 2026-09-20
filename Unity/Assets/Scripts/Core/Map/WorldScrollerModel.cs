using System;
using System.Collections.Generic;
using System.Numerics;

namespace Gamelab.Map
{
    /// <summary>
    /// Port of Src/Map/WorldScroller.cs logic (tiles and trees, no drawing). Pixels, Y down.
    /// The RNG call order matches Src exactly.
    /// </summary>
    public sealed class WorldScrollerModel
    {
        public readonly struct Tree
        {
            public readonly Vector2 TrunkBase;
            public readonly float Scale;

            public Tree(Vector2 trunkBase, float scale)
            {
                TrunkBase = trunkBase;
                Scale = scale;
            }
        }

        public const float TrackScale = 1.4f;
        public const float TreeBaseScale = 0.22f;
        public const float TreeScaleVariance = 0.08f;
        public const float TreeHorizontalSpacingBase = 360f;
        public const float TreeHorizontalSpacingVariance = 180f;
        public const float MinTrunkBaseClearanceAboveTracks = 100f;
        public const float TrunkBaseOffsetBelowRailStrip = 300f;

        private readonly List<Vector2> tilePositions = new List<Vector2>();
        private readonly List<int> tileTypes = new List<int>();
        private readonly List<Tree> trees = new List<Tree>();
        private readonly Random random;
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly int trackTexHeight;
        private readonly int pineTexWidth;
        private readonly float cameraMinZoom;
        private readonly float cameraMaxZoom;
        private readonly float centerY;

        public WorldScrollerModel(int screenW, int screenH, int trackTexWidth, int trackTexHeight,
            int pineTexWidth, float cameraMinZoom, float cameraMaxZoom, Random random)
        {
            screenWidth = screenW;
            screenHeight = screenH;
            this.trackTexHeight = trackTexHeight;
            this.pineTexWidth = pineTexWidth;
            this.cameraMinZoom = cameraMinZoom;
            this.cameraMaxZoom = cameraMaxZoom;
            this.random = random;
            TileWidth = (int)(trackTexWidth * TrackScale);

            centerY = (screenHeight / 2.0f) - (trackTexHeight / 2.0f);
            float leftEdge = -LeftVisibleOverflow;
            int startTile = (int)MathF.Floor(leftEdge / TileWidth);
            int endTile = (screenWidth / TileWidth) + 3;
            for (int i = startTile; i < endTile; i++)
            {
                SpawnTile(i * TileWidth);
            }

            SpawnInitialTrees();
        }

        public int TileWidth { get; }
        public float CenterY => centerY;
        public int TileCount => tilePositions.Count;
        public IReadOnlyList<Vector2> TilePositions => tilePositions;
        public IReadOnlyList<int> TileTypes => tileTypes;
        public IReadOnlyList<Tree> Trees => trees;

        private void SpawnTile(float xOffset)
        {
            tilePositions.Add(new Vector2(xOffset, centerY));
            tileTypes.Add(random.Next(0, 2));
        }

        private float MaxVisibleWidth => screenWidth / cameraMinZoom;

        private float LeftVisibleOverflow => (screenWidth / cameraMaxZoom - screenWidth) / 2f;

        private float VerticalVisibleOverflow => (screenHeight / cameraMaxZoom - screenHeight) / 2f;

        private void SpawnInitialTrees()
        {
            float xCursorTop = -TreeHorizontalSpacingBase;
            float xCursorBottom = -TreeHorizontalSpacingBase * 0.5f;
            float endX = MaxVisibleWidth + TreeHorizontalSpacingBase;

            while (xCursorTop < endX)
            {
                SpawnTree(xCursorTop, true);
                xCursorTop += NextTreeSpacing();
            }

            while (xCursorBottom < endX)
            {
                SpawnTree(xCursorBottom, false);
                xCursorBottom += NextTreeSpacing();
            }
        }

        private float NextTreeSpacing()
        {
            return TreeHorizontalSpacingBase + (float)random.NextDouble() * TreeHorizontalSpacingVariance;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private void SpawnTree(float x, bool placeTrunkBaseAboveTracks)
        {
            float railTopY = centerY;
            float railBottomY = centerY + trackTexHeight;
            float scale = TreeBaseScale + ((float)random.NextDouble() - 0.5f) * TreeScaleVariance;

            float trunkBaseY;
            if (placeTrunkBaseAboveTracks)
            {
                float minTrunkBase = -VerticalVisibleOverflow;
                float maxTrunkBase = railTopY - MinTrunkBaseClearanceAboveTracks;
                trunkBaseY = Lerp(minTrunkBase, maxTrunkBase, (float)random.NextDouble());
            }
            else
            {
                float bandTopY = railBottomY + TrunkBaseOffsetBelowRailStrip;
                float bandBottomY = screenHeight + VerticalVisibleOverflow;
                trunkBaseY = Lerp(bandTopY, bandBottomY, (float)random.NextDouble());
            }

            trees.Add(new Tree(new Vector2(x, trunkBaseY), scale));
        }

        public void Update(float deltaTime, float actualSpeed)
        {
            float speed = actualSpeed;

            for (int i = 0; i < tilePositions.Count; i++)
            {
                tilePositions[i] = new Vector2(tilePositions[i].X - (speed * deltaTime), centerY - 15);
            }

            if (tilePositions.Count > 0 && tilePositions[0].X + TileWidth < -LeftVisibleOverflow)
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
            float despawnWhenPastThisLeftEdge = -pineTexWidth * 1.5f;

            for (int i = 0; i < trees.Count; i++)
            {
                Tree t = trees[i];
                trees[i] = new Tree(new Vector2(t.TrunkBase.X - speed * deltaTime, t.TrunkBase.Y), t.Scale);
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
                SpawnTree(newX, placeInUpperBand);
            }
        }
    }
}
