using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Gamelab.Map
{
    /// <summary>
    /// Port of the geometry in Src/Map/HubMap.cs (no drawing, no physics). Pixels, Y down.
    /// Shop offer spawning (BuyableStationWrapper) stays with the caller: see SelectOfferIndices.
    /// </summary>
    public sealed class HubMapModel
    {
        /// <summary>Axis aligned box in pixels: centre and full size.</summary>
        public readonly struct BoxPx
        {
            public readonly Vector2 Center;
            public readonly Vector2 Size;

            public BoxPx(Vector2 center, Vector2 size)
            {
                Center = center;
                Size = size;
            }
        }

        public readonly struct Stake
        {
            public readonly Vector2 Feet;
            public readonly int Variant;

            public Stake(Vector2 feet, int variant)
            {
                Feet = feet;
                Variant = variant;
            }

            /// <summary>Texture key Stake1..Stake3.</summary>
            public string TextureKey => "Stake" + (Variant + 1);

            /// <summary>Src Stake physics body: 45 x 80 px, centred 40 px above the feet.</summary>
            public BoxPx Collider => new BoxPx(Feet - new Vector2(0f, CollisionHeightPixels / 2f),
                new Vector2(CollisionWidthPixels, CollisionHeightPixels));
        }

        public readonly struct Tree
        {
            public readonly Vector2 Feet;
            public readonly float Scale;

            public Tree(Vector2 feet, float scale)
            {
                Feet = feet;
                Scale = scale;
            }
        }

        public readonly struct Npc
        {
            public readonly string Name;
            public readonly Vector2 Feet;
            public readonly float Scale;

            public Npc(string name, Vector2 feet, float scale)
            {
                Name = name;
                Feet = feet;
                Scale = scale;
            }
        }

        public const int HubPad = 120;
        public const float FenceStakeSpacing = 95f;
        public const float SignGapWidth = 280f;
        public const float HubTreeBaseScale = 0.55f;
        public const float HouseScale = 0.3f;
        public const float HouseTop = 150f;
        public const float HouseColliderTrim = 120f;
        public const float CollisionWidthPixels = 45f;
        public const float CollisionHeightPixels = 80f;
        public const float StakeVisualWidthPixels = 50f;
        public const float StakeBuriedTipFraction = 0.13f;
        public const float TrackScale = 1.4f;
        public const float BoundaryThickness = 1f;
        public const int StakeVariants = 3;

        private readonly List<BoxPx> boundaryWalls = new List<BoxPx>();
        private readonly List<BoxPx> houseColliders = new List<BoxPx>();
        private readonly List<Stake> stakes = new List<Stake>();
        private readonly List<Tree> trees = new List<Tree>();
        private readonly int railTexWidth;
        private readonly int railTexHeight;

        public HubMapModel(int worldWidth, int worldHeight, int railTexWidth = 233, int railTexHeight = 409,
            int hubTexWidth = 3840, int hubTexHeight = 2160, int house1TexWidth = 2048, int house1TexHeight = 2176,
            int house2TexWidth = 1846, int house2TexHeight = 2165)
        {
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            this.railTexWidth = railTexWidth;
            this.railTexHeight = railTexHeight;
            Vendor = new Npc("Vendor", new Vector2(1260f, 700f), 0.3f);
            Town1 = new Npc("Town1", new Vector2(500f, 580f), 0.3f);

            ShoppingArea = new RectPx(HubPad, HubPad, worldWidth - HubPad * 2, worldHeight / 2 - HubPad - 160);

            float hubBgScale = worldWidth * 1.0f / hubTexWidth;
            int villageBottom = (int)MathF.Min(hubTexHeight * hubBgScale, worldHeight / 2f);
            VillageRect = new RectPx(HubPad, HubPad, worldWidth - HubPad * 2,
                Math.Max(ShoppingArea.Height, villageBottom - HubPad));

            BuildBoundaryWalls();
            BuildVillageFence();
            BuildTrees();
            House1Position = new Vector2(worldWidth / 4f, HouseTop);
            House2Position = new Vector2(worldWidth / 4f * 3f, HouseTop);
            houseColliders.Add(HouseBox(House1Position, house1TexWidth, house1TexHeight));
            houseColliders.Add(HouseBox(House2Position, house2TexWidth, house2TexHeight));
        }

        public int WorldWidth { get; }
        public int WorldHeight { get; }
        public RectPx ShoppingArea { get; }
        public RectPx VillageRect { get; }
        public Vector2 House1Position { get; }
        public Vector2 House2Position { get; }
        public Npc Vendor { get; }
        public Npc Town1 { get; }
        public IReadOnlyList<BoxPx> BoundaryWalls => boundaryWalls;
        public IReadOnlyList<BoxPx> HouseColliders => houseColliders;
        public IReadOnlyList<Stake> Stakes => stakes;
        public IReadOnlyList<Tree> Trees => trees;

        public int RailTileWidth => (int)(railTexWidth * TrackScale);

        /// <summary>Src pad for the snow backdrop and the rail row: ceil(max(W, H) / minZoom).</summary>
        public int ViewPad => (int)MathF.Ceiling(MathF.Max(WorldWidth, WorldHeight) / CameraTuning.MinZoom);

        /// <summary>Snow backdrop rectangle: (-pad, -pad, W + 2pad, H + 2pad).</summary>
        public RectPx Backdrop => new RectPx(-ViewPad, -ViewPad, WorldWidth + ViewPad * 2, WorldHeight + ViewPad * 2);

        /// <summary>Top-left Y of each rail tile (Src DrawRails centerY - 30).</summary>
        public float RailDrawY => (WorldHeight / 4f * 3) - railTexHeight + 40 - 30;

        public int RailStartColumn => (int)MathF.Floor(-ViewPad / (float)RailTileWidth);

        public int RailColumnCount => (WorldWidth + ViewPad * 2) / RailTileWidth + 3;

        private static BoxPx HouseBox(Vector2 position, int texW, int texH) =>
            new BoxPx(position, new Vector2(texW * HouseScale, texH * HouseScale - HouseColliderTrim));

        private void BuildBoundaryWalls()
        {
            float t = BoundaryThickness;
            float w = WorldWidth;
            float h = WorldHeight;
            boundaryWalls.Add(new BoxPx(new Vector2(w / 2f, -t / 2f), new Vector2(w, t)));
            boundaryWalls.Add(new BoxPx(new Vector2(w / 2f, h + t / 2f), new Vector2(w, t)));
            boundaryWalls.Add(new BoxPx(new Vector2(-t / 2f, h / 2f), new Vector2(t, h)));
            boundaryWalls.Add(new BoxPx(new Vector2(w + t / 2f, h / 2f), new Vector2(t, h)));
        }

        private void BuildVillageFence()
        {
            // Bottom edge with a sign-shaped gap centered horizontally.
            float gapHalf = SignGapWidth / 2f;
            float centerX = WorldWidth / 2f;
            for (float x = VillageRect.Left; x <= VillageRect.Right; x += FenceStakeSpacing)
            {
                if (MathF.Abs(x - centerX) < gapHalf) continue;
                AddStake(new Vector2(x, VillageRect.Bottom));
            }

            AddVertical(VillageRect.Left);
            AddVertical(VillageRect.Right);
        }

        private void AddVertical(float x)
        {
            for (float y = VillageRect.Top; y <= VillageRect.Bottom - FenceStakeSpacing; y += FenceStakeSpacing)
                AddStake(new Vector2(x, y));
        }

        private void AddStake(Vector2 feet) => stakes.Add(new Stake(feet, stakes.Count % StakeVariants));

        private void BuildTrees()
        {
            float leftMarginX = VillageRect.Left * 0.5f;
            float rightMarginX = (VillageRect.Right + WorldWidth) / 2f;
            float topY = VillageRect.Top + VillageRect.Height * 0.15f;
            float midY = VillageRect.Top + VillageRect.Height * 0.5f;
            float lowerY = VillageRect.Top + VillageRect.Height * 0.85f;

            Vector2[] handPlaced =
            {
                new Vector2(leftMarginX, topY),
                new Vector2(leftMarginX - 40f, midY),
                new Vector2(leftMarginX + 30f, lowerY),
                new Vector2(rightMarginX, topY),
                new Vector2(rightMarginX + 40f, midY),
                new Vector2(rightMarginX - 20f, lowerY),
            };
            float[] scales = { 1.0f, 0.9f, 1.1f, 1.0f, 1.05f, 0.95f };
            for (int i = 0; i < handPlaced.Length; i++)
                trees.Add(new Tree(handPlaced[i], HubTreeBaseScale * scales[i]));
        }

        /// <summary>Src HubScreen.InitializeMaps prep train top-left (worldWidth = 2 * screenHeight).</summary>
        public static Vector2 PrepTrainTopLeft(int worldWidth, int screenHeight, int trainWidthPx) =>
            new Vector2((worldWidth - trainWidthPx) / 2f, screenHeight - 200 + 360f);

        /// <summary>Src HubScreen hubSeed.</summary>
        public static int RestockSeed(int runSeed, int level) => unchecked(runSeed + level * 4242);

        /// <summary>Src RestockHubDragOffers positions, one per offer, centred on the shop row.</summary>
        public Vector2[] OfferPositions(int offerCount)
        {
            var shopCenter = new Vector2(ShoppingArea.Center.X, ShoppingArea.Bottom - 350);
            const float spacingX = 100f;
            float startX = shopCenter.X - (offerCount - 1) * spacingX / 2f;
            var result = new Vector2[offerCount];
            for (int i = 0; i < offerCount; i++) result[i] = new Vector2(startX + i * spacingX, shopCenter.Y);
            return result;
        }

        /// <summary>
        /// Src catalog.OrderBy(x => hubRandom.Next()).Take(offerCount) as catalog indices. Seam for the
        /// shop wave: catalog = IShopService.GenerateCatalog(), then spawn a BuyableStationWrapper for
        /// catalog[idx[i]].ItemId at OfferPositions(offerCount)[i].
        /// </summary>
        public static int[] SelectOfferIndices(int catalogCount, Random hubRandom, int offerCount) =>
            Enumerable.Range(0, catalogCount).OrderBy(x => hubRandom.Next()).Take(offerCount).ToArray();
    }
}
