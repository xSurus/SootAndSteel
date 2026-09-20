using System;
using System.Collections.Generic;
using Gamelab.Map.Train.State;
using UnityEngine;

namespace Gamelab.Map
{
    /// <summary>
    /// Draws the scrolling snow world (Src/Map/WorldScroller.cs): backdrop, rail tiles and pines.
    /// The model runs in Src pixels (Y down), sprites are placed in meters through MapSpace.
    /// </summary>
    public sealed class WorldScrollerView : MonoBehaviour
    {
        private const int BackdropOrder = -32000;
        private const int TileOrder = -31000;

        private static readonly Color SnowColor = new Color(208f / 255f, 232f / 255f, 242f / 255f, 1f);

        private readonly List<SpriteRenderer> tileRenderers = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> treeRenderers = new List<SpriteRenderer>();
        private Sprite rail1, rail2, pine, white;
        private TrainStateRuntime train;

        public WorldScrollerModel Model { get; private set; }
        public SpriteRenderer Backdrop { get; private set; }
        public IReadOnlyList<SpriteRenderer> TileRenderers => tileRenderers;
        public IReadOnlyList<SpriteRenderer> TreeRenderers => treeRenderers;

        /// <summary>Scroll speed in px/s. Defaults to the linked train's actualSpeed, else 0.</summary>
        public Func<float> SpeedSource { get; set; }

        public void Link(TrainStateRuntime trainState) => train = trainState;

        private void Awake()
        {
            rail1 = MapSprites.Get("Rail_Tile_01");
            rail2 = MapSprites.Get("Rail_Tile_02");
            pine = MapSprites.Get("Decorations/Snow_Covered_Pine");
            Rebuild(new System.Random());
            SpeedSource = () => train != null ? train.State.actualSpeed : 0f;
        }

        /// <summary>Recreates the model (fixed random for tests) and resyncs.</summary>
        public void Rebuild(System.Random random)
        {
            Model = new WorldScrollerModel(TrainTuning.ScreenWidth, TrainTuning.ScreenHeight,
                (int)rail1.rect.width, (int)rail1.rect.height, (int)pine.rect.width,
                CameraTuning.MinZoom, CameraTuning.MaxZoom, random);
            Sync();
        }

        private void OnDestroy()
        {
            if (white == null) return;
            Destroy(white.texture);
            Destroy(white);
        }

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            Model.Update(dt, SpeedSource != null ? SpeedSource() : 0f);
            Sync();
        }

        public void Sync()
        {
            SyncBackdrop();
            Grow(tileRenderers, Model.TileCount, "Tile", TileOrder);
            float tileScale = MapSpace.SpriteScale(rail1.pixelsPerUnit, WorldScrollerModel.TrackScale);
            Vector2 tileSize = new Vector2(rail1.rect.width, rail1.rect.height) * WorldScrollerModel.TrackScale;
            for (int i = 0; i < Model.TileCount; i++)
            {
                SpriteRenderer r = tileRenderers[i];
                r.sprite = Model.TileTypes[i] == 0 ? rail1 : rail2;
                r.transform.localScale = new Vector3(tileScale, tileScale, 1f);
                // Src position is the top-left corner, the sprite pivot is the centre.
                Vector2 centre = MapSpace.ToUnity(Model.TilePositions[i]) + tileSize / 2f;
                r.transform.position = MapSpace.PxToMeters(centre);
            }

            Grow(treeRenderers, Model.Trees.Count, "Tree", 0);
            for (int i = 0; i < Model.Trees.Count; i++)
            {
                WorldScrollerModel.Tree t = Model.Trees[i];
                SpriteRenderer r = treeRenderers[i];
                r.sprite = pine;
                float s = MapSpace.SpriteScale(pine.pixelsPerUnit, t.Scale);
                r.transform.localScale = new Vector3(s, s, 1f);
                // Src origin is bottom-centre at TrunkBase, so the centre sits half a height above it.
                Vector2 centre = MapSpace.ToUnity(t.TrunkBase) - new Vector2(0f, pine.rect.height * t.Scale / 2f);
                r.transform.position = MapSpace.PxToMeters(centre);
                r.sortingOrder = Mathf.RoundToInt(t.TrunkBase.Y);
            }
        }

        private void SyncBackdrop()
        {
            if (Backdrop == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                Backdrop = NewRenderer("Backdrop", BackdropOrder);
                Backdrop.sprite = white;
                Backdrop.color = SnowColor;
            }

            int padX = (int)Mathf.Ceil(TrainTuning.ScreenWidth / CameraTuning.MinZoom);
            int padY = (int)Mathf.Ceil(TrainTuning.ScreenHeight / CameraTuning.MinZoom);
            var size = new Vector2(TrainTuning.ScreenWidth + padX * 2, TrainTuning.ScreenHeight + padY * 2);
            var centre = new Vector2(-padX, -padY) + size / 2f;
            Backdrop.transform.localScale = new Vector3(MapSpace.PxToMeters(size.x), MapSpace.PxToMeters(size.y), 1f);
            Backdrop.transform.position = MapSpace.PxToMeters(centre);
        }

        private void Grow(List<SpriteRenderer> list, int count, string label, int order)
        {
            while (list.Count < count) list.Add(NewRenderer(label, order));
            while (list.Count > count)
            {
                Destroy(list[list.Count - 1].gameObject);
                list.RemoveAt(list.Count - 1);
            }
        }

        private SpriteRenderer NewRenderer(string label, int order)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            SpriteRenderer r = go.AddComponent<SpriteRenderer>();
            MapSpace.ApplyToSprite(r);
            r.sortingOrder = order;
            return r;
        }
    }
}
