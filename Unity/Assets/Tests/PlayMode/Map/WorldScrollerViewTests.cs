using System.Linq;
using Gamelab.Map;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.PlayMode
{
    public class WorldScrollerViewTests
    {
        private GameObject go;
        private WorldScrollerView view;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("scroller");
            view = go.AddComponent<WorldScrollerView>();
            view.Rebuild(new System.Random(7));
            view.SpeedSource = () => 300f;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void RenderersMatchModelCountsAndSprites()
        {
            Assert.AreEqual(view.Model.TileCount, view.TileRenderers.Count);
            Assert.AreEqual(view.Model.Trees.Count, view.TreeRenderers.Count);
            for (int i = 0; i < view.TileRenderers.Count; i++)
            {
                string expected = view.Model.TileTypes[i] == 0 ? "Rail_Tile_01" : "Rail_Tile_02";
                Assert.AreSame(MapSprites.Get(expected), view.TileRenderers[i].sprite);
                Assert.IsTrue(view.TileRenderers[i].flipY);
            }

            Sprite pine = MapSprites.Get("Decorations/Snow_Covered_Pine");
            Assert.IsTrue(view.TreeRenderers.All(r => r.sprite == pine && r.flipY));
        }

        [Test]
        public void WorldSizesMatchSrcPixels()
        {
            SpriteRenderer tile = view.TileRenderers[0];
            Assert.AreEqual(3.262f, tile.bounds.size.x, 1e-2f);
            Assert.AreEqual(409 * 1.4f / 100f, tile.bounds.size.y, 1e-2f);

            float scale = view.Model.Trees[0].Scale;
            Assert.AreEqual(847 * scale / 100f, view.TreeRenderers[0].bounds.size.x, 1e-2f);
            Assert.AreEqual(1323 * scale / 100f, view.TreeRenderers[0].bounds.size.y, 1e-2f);
        }

        [Test]
        public void TilePlacedByTopLeftAndTreeByTrunkBase()
        {
            Vector2 tl = view.Model.TilePositions[0].ToUnity();
            Bounds tb = view.TileRenderers[0].bounds;
            Assert.AreEqual(tl.x / 100f, tb.min.x, 1e-2f);
            Assert.AreEqual(tl.y / 100f, tb.min.y, 1e-2f);

            Vector2 trunk = view.Model.Trees[0].TrunkBase.ToUnity();
            Bounds rb = view.TreeRenderers[0].bounds;
            Assert.AreEqual(trunk.x / 100f, rb.center.x, 1e-2f);
            Assert.AreEqual(trunk.y / 100f, rb.max.y, 1e-2f);
        }

        [Test]
        public void BackdropCoversSrcArea()
        {
            Bounds b = view.Backdrop.bounds;
            Assert.AreEqual(-4800f / 100f, b.min.x, 1e-2f);
            Assert.AreEqual(-2700f / 100f, b.min.y, 1e-2f);
            Assert.AreEqual((1920 + 9600) / 100f, b.size.x, 1e-2f);
            Assert.AreEqual((1080 + 5400) / 100f, b.size.y, 1e-2f);
        }

        [Test]
        public void ScrollMovesRenderersLeft()
        {
            float x0 = view.TileRenderers[0].transform.position.x;
            float t0 = view.TreeRenderers[0].transform.position.x;
            float y0 = view.TileRenderers[0].transform.position.y;
            view.Tick(0.1f);
            Assert.AreEqual(-300f * 0.1f / 100f, view.TileRenderers[0].transform.position.x - x0, 1e-3f);
            Assert.AreEqual(-300f * 0.1f / 100f, view.TreeRenderers[0].transform.position.x - t0, 1e-3f);
            // Src snaps tile Y to centerY - 15 on the first update.
            Assert.AreEqual(-15f / 100f, view.TileRenderers[0].transform.position.y - y0, 1e-3f);
        }

        [Test]
        public void RecycleKeepsRendererCounts()
        {
            int tiles = view.TileRenderers.Count;
            int trees = view.TreeRenderers.Count;
            for (int i = 0; i < 40; i++) view.Tick(0.5f);
            Assert.AreEqual(tiles, view.TileRenderers.Count);
            Assert.AreEqual(trees, view.TreeRenderers.Count);
            Assert.AreEqual(view.Model.Trees.Count, trees);
        }

        [Test]
        public void ZeroSpeedWithoutTrain()
        {
            view.SpeedSource = null;
            float x0 = view.TileRenderers[0].transform.position.x;
            view.Tick(1f);
            Assert.AreEqual(x0, view.TileRenderers[0].transform.position.x, 1e-4f);
        }
    }
}
