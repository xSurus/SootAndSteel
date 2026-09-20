using System.Collections.Generic;
using Gamelab.Map.Train;
using UnityEngine;

namespace Gamelab.Map
{
    /// <summary>
    /// Unity build of Src/Map/HubMap.cs: static colliders (world boundary, houses, fence stakes) and the
    /// hub art. Physics stays in Src's pixel frame (Y down) in meters. Sprite pivots are the centre, so
    /// feet-origin art is placed half its height above the feet (smaller Y).
    /// Shop offers are not built here. Seam: the shop wave calls HubMapModel.SelectOfferIndices over
    /// IShopService.GenerateCatalog() and spawns a BuyableStationWrapper at each OfferPositions entry.
    /// </summary>
    public sealed class HubMapView : MonoBehaviour
    {
        private const int BackdropOrder = -32000;
        private const int RailOrder = -31000;
        private const int HouseOrder = -30000;
        private const string PineKey = "Decorations/Snow_Covered_Pine";

        private static readonly Color SnowColor = new Color(208f / 255f, 232f / 255f, 242f / 255f, 1f);

        private readonly List<BoxCollider2D> boundaryColliders = new List<BoxCollider2D>();
        private readonly List<BoxCollider2D> houseColliders = new List<BoxCollider2D>();
        private readonly List<BoxCollider2D> stakeColliders = new List<BoxCollider2D>();
        private readonly List<SpriteRenderer> stakeRenderers = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> railRenderers = new List<SpriteRenderer>();
        private readonly List<Object> owned = new List<Object>();

        public HubMapModel Model { get; private set; }
        public IReadOnlyList<BoxCollider2D> BoundaryColliders => boundaryColliders;
        public IReadOnlyList<BoxCollider2D> HouseColliders => houseColliders;
        public IReadOnlyList<BoxCollider2D> StakeColliders => stakeColliders;
        public IReadOnlyList<SpriteRenderer> StakeRenderers => stakeRenderers;
        public IReadOnlyList<SpriteRenderer> RailRenderers => railRenderers;

        public static HubMapView Create(HubMapModel model)
        {
            var root = new GameObject("HubMap");
            var view = root.AddComponent<HubMapView>();
            view.Build(model);
            return view;
        }

        /// <summary>Src HubScreen.InitializeMaps: prep train below the village, top door at the middle column.</summary>
        public static TrainMapRuntime CreatePrepTrain(HubMapModel model, System.Random tileRandom)
        {
            int trainW = TrainTuning.Width * TrainTuning.TileSize;
            System.Numerics.Vector2 topLeft = HubMapModel.PrepTrainTopLeft(model.WorldWidth, model.WorldHeight / 2, trainW);
            var layout = new TrainLayout(TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize, topLeft,
                new DoorSpec(false, TrainTuning.Width / 2));
            return TrainMapRuntime.Create(layout, tileRandom);
        }

        private void Build(HubMapModel model)
        {
            Model = model;
            foreach (HubMapModel.BoxPx b in model.BoundaryWalls) boundaryColliders.Add(AddBox("Boundary", b));
            foreach (HubMapModel.BoxPx b in model.HouseColliders) houseColliders.Add(AddBox("House", b));
            foreach (HubMapModel.Stake s in model.Stakes) stakeColliders.Add(AddBox("Stake", s.Collider));

            BuildBackdrop();
            BuildRails();
            BuildHouse("House1", model.House1Position);
            BuildHouse("House2", model.House2Position);
            foreach (HubMapModel.Tree t in model.Trees)
                PlaceFeet("Tree", MapSprites.Get(PineKey), t.Feet, t.Scale);
            foreach (HubMapModel.Npc n in new[] { model.Vendor, model.Town1 })
                PlaceFeet(n.Name, MapSprites.Get("NPCs/" + n.Name), n.Feet, n.Scale);
            foreach (HubMapModel.Stake s in model.Stakes) stakeRenderers.Add(BuildStake(s));
        }

        private BoxCollider2D AddBox(string label, HubMapModel.BoxPx box)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            go.transform.position = MapSpace.PxToMeters(MapSpace.ToUnity(box.Center));
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            BoxCollider2D c = go.AddComponent<BoxCollider2D>();
            c.size = MapSpace.PxToMeters(MapSpace.ToUnity(box.Size));
            return c;
        }

        private SpriteRenderer NewRenderer(string label, Sprite sprite, int order)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            SpriteRenderer r = go.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.sortingOrder = order;
            MapSpace.ApplyToSprite(r);
            return r;
        }

        private static void Scale(SpriteRenderer r, float srcScale)
        {
            float s = MapSpace.SpriteScale(r.sprite.pixelsPerUnit, srcScale);
            r.transform.localScale = new Vector3(s, s, 1f);
        }

        // Src origin is the sprite centre at the given position.
        private void BuildHouse(string name, System.Numerics.Vector2 pos)
        {
            SpriteRenderer r = NewRenderer(name, MapSprites.Get("Hub/" + name), HouseOrder);
            Scale(r, HubMapModel.HouseScale);
            r.transform.position = MapSpace.PxToMeters(MapSpace.ToUnity(pos));
        }

        // Src origin is bottom-centre at feet, depth by feet Y.
        private SpriteRenderer PlaceFeet(string name, Sprite sprite, System.Numerics.Vector2 feet, float scale)
        {
            SpriteRenderer r = NewRenderer(name, sprite, Mathf.RoundToInt(feet.Y));
            Scale(r, scale);
            Vector2 centre = MapSpace.ToUnity(feet) - new Vector2(0f, sprite.rect.height * scale / 2f);
            r.transform.position = MapSpace.PxToMeters(centre);
            return r;
        }

        // Src Stake.Draw shows only the top (1 - 0.13) of the art, 50 px wide, feet at the bottom of the crop.
        private SpriteRenderer BuildStake(HubMapModel.Stake s)
        {
            Sprite full = MapSprites.Get("Hub/" + s.TextureKey);
            float w = full.rect.width, h = full.rect.height;
            int visibleH = Mathf.RoundToInt(h * (1f - HubMapModel.StakeBuriedTipFraction));
            // Sprite rects count from the bottom, so the visible top part starts at h - visibleH.
            Sprite crop = Sprite.Create(full.texture, new Rect(full.rect.x, full.rect.y + h - visibleH, w, visibleH),
                new Vector2(0.5f, 0.5f), full.pixelsPerUnit);
            owned.Add(crop);
            SpriteRenderer r = NewRenderer("StakeArt", crop, Mathf.RoundToInt(s.Feet.Y));
            float scale = HubMapModel.StakeVisualWidthPixels / w;
            Scale(r, scale);
            r.transform.position = MapSpace.PxToMeters(MapSpace.ToUnity(s.Feet) - new Vector2(0f, visibleH * scale / 2f));
            return r;
        }

        private void BuildBackdrop()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            var white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            owned.Add(tex);
            owned.Add(white);
            SpriteRenderer r = NewRenderer("Backdrop", white, BackdropOrder);
            r.color = SnowColor;
            RectPx a = Model.Backdrop;
            r.transform.localScale = new Vector3(MapSpace.PxToMeters(a.Width), MapSpace.PxToMeters(a.Height), 1f);
            r.transform.position = MapSpace.PxToMeters(new Vector2(a.X + a.Width / 2f, a.Y + a.Height / 2f));
        }

        private void BuildRails()
        {
            Sprite rail = MapSprites.Get("Rail_Tile_01");
            int tileW = Model.RailTileWidth;
            var size = new Vector2(rail.rect.width, rail.rect.height) * HubMapModel.TrackScale;
            for (int i = 0; i < Model.RailColumnCount; i++)
            {
                SpriteRenderer r = NewRenderer("Rail", rail, RailOrder);
                Scale(r, HubMapModel.TrackScale);
                // Src position is the top-left corner, the sprite pivot is the centre.
                var topLeft = new Vector2((Model.RailStartColumn + i) * tileW, Model.RailDrawY);
                r.transform.position = MapSpace.PxToMeters(topLeft + size / 2f);
                railRenderers.Add(r);
            }
        }

        private void OnDestroy()
        {
            foreach (Object o in owned)
                if (o != null) Destroy(o);
        }
    }
}
