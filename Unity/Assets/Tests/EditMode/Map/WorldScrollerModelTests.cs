using System;
using System.Linq;
using System.Numerics;
using Gamelab.Map;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class WorldScrollerModelTests
    {
        private static WorldScrollerModel Make(int seed) => new WorldScrollerModel(1920, 1080, 233, 409, 847,
            CameraTuning.MinZoom, CameraTuning.MaxZoom, new Random(seed));

        // Golden values: Src/Map/WorldScroller.cs run with seed 7 (Update 0.5 s at speed 300).
        private static readonly (float x, float y, int type)[] InitTiles =
        {
            (-326f, 335.5f, 0),
            (0f, 335.5f, 1),
            (326f, 335.5f, 1),
            (652f, 335.5f, 0),
            (978f, 335.5f, 0),
            (1304f, 335.5f, 1),
            (1630f, 335.5f, 0),
            (1956f, 335.5f, 1),
            (2282f, 335.5f, 1)
        };

        private static readonly (float x, float y, float scale)[] InitTrees =
        {
            (-360f, 31.361893f, 0.24839698f),
            (167.81683f, -93.76381f, 0.21558532f),
            (686.5686f, 163.20938f, 0.21216106f),
            (1106.9302f, -125.727875f, 0.2493339f),
            (1601.3905f, -70.18575f, 0.22523269f),
            (2047.4388f, -124.41659f, 0.21087855f),
            (2578.8662f, 42.95352f, 0.18199009f),
            (3068.219f, -24.89669f, 0.2417095f),
            (3448.3862f, -102.78583f, 0.19312304f),
            (3820.3257f, -78.87065f, 0.24630715f),
            (4277.8687f, 190.6698f, 0.25456613f),
            (4756.802f, -110.69539f, 0.18991877f),
            (-180f, 1167.04f, 0.25673962f),
            (282.03662f, 1069.4783f, 0.24645448f),
            (682.23584f, 1165.0662f, 0.2498904f),
            (1206.766f, 1183.9369f, 0.22772169f),
            (1692.1033f, 1158.1007f, 0.19463138f),
            (2112.9194f, 1047.492f, 0.23790368f),
            (2558.1787f, 1058.7527f, 0.18999517f),
            (2945.5107f, 1092.3727f, 0.2456221f),
            (3309.894f, 1142.2261f, 0.24524997f),
            (3787.504f, 1203.8489f, 0.24347498f),
            (4173.7886f, 1119.7628f, 0.18690097f),
            (4553.469f, 1117.2587f, 0.25042373f),
            (4969.243f, 1077.6102f, 0.21084027f)
        };

        private static readonly (float x, float y, int type)[] Step40Tiles =
        {
            (-458f, 320.5f, 1),
            (-132f, 320.5f, 1),
            (194f, 320.5f, 0),
            (520f, 320.5f, 1),
            (846f, 320.5f, 1),
            (1172f, 320.5f, 0),
            (1498f, 320.5f, 1),
            (1824f, 320.5f, 0),
            (2150f, 320.5f, 1)
        };

        private static readonly (float x, float y, float scale)[] Step40Trees =
        {
            (-1243.1982f, -110.69539f, 0.18991877f),
            (-1030.7568f, 1077.6102f, 0.21084027f),
            (245.63672f, -35.285095f, 0.22540136f),
            (426.60205f, 1127.2294f, 0.23167577f),
            (697.4907f, -65.687935f, 0.24891622f),
            (986.7612f, 1068.1942f, 0.21567237f),
            (1444.4146f, 1142.2522f, 0.18120451f),
            (1362.5894f, 151.22455f, 0.18641543f),
            (1814.436f, 82.47635f, 0.21011102f),
            (1847.5322f, 1102.179f, 0.18224101f),
            (2232.7065f, 1076.1882f, 0.23037131f),
            (2275.5605f, 129.2633f, 0.22978345f),
            (2690.2104f, 1073.1731f, 0.25417814f),
            (2741.6523f, -29.565987f, 0.23810531f),
            (3082.92f, 1093.5645f, 0.21475135f),
            (3258.5986f, -68.62915f, 0.24743873f),
            (3676.5645f, 1183.8848f, 0.24540368f),
            (3695.9902f, 195.76324f, 0.18458065f),
            (4098.3677f, 1074.5122f, 0.1996539f),
            (4212.588f, -117.50467f, 0.183048f),
            (4601.675f, 1132.5933f, 0.25072813f),
            (4703.0283f, 75.97865f, 0.24298434f),
            (5050.0176f, 1126.9298f, 0.18369755f),
            (5173.0215f, 35.299957f, 0.2547207f),
            (5581.337f, 1128.9501f, 0.19248708f)
        };

        private static void AssertTiles(WorldScrollerModel m, (float x, float y, int type)[] exp)
        {
            Assert.AreEqual(exp.Length, m.TileCount);
            for (int i = 0; i < exp.Length; i++)
            {
                Assert.AreEqual(exp[i].x, m.TilePositions[i].X, 1e-3f, "tile x " + i);
                Assert.AreEqual(exp[i].y, m.TilePositions[i].Y, 1e-3f, "tile y " + i);
                Assert.AreEqual(exp[i].type, m.TileTypes[i], "tile type " + i);
            }
        }

        private static void AssertTrees(WorldScrollerModel m, (float x, float y, float scale)[] exp)
        {
            Assert.AreEqual(exp.Length, m.Trees.Count);
            for (int i = 0; i < exp.Length; i++)
            {
                Assert.AreEqual(exp[i].x, m.Trees[i].TrunkBase.X, 1e-3f, "tree x " + i);
                Assert.AreEqual(exp[i].y, m.Trees[i].TrunkBase.Y, 1e-3f, "tree y " + i);
                Assert.AreEqual(exp[i].scale, m.Trees[i].Scale, 1e-3f, "tree scale " + i);
            }
        }

        [Test]
        public void ConstructionMatchesSrcGolden()
        {
            var m = Make(7);
            Assert.AreEqual(326, m.TileWidth);
            Assert.AreEqual(335.5f, m.CenterY, 1e-3f);
            AssertTiles(m, InitTiles);
            AssertTrees(m, InitTrees);
        }

        [Test]
        public void ScrollAndRecycleMatchesSrcGolden()
        {
            var m = Make(7);
            for (int i = 0; i < 40; i++) m.Update(0.5f, 300f);
            AssertTiles(m, Step40Tiles);
            AssertTrees(m, Step40Trees);
        }

        [Test]
        public void TilesRecycleWhenFirstLeaves()
        {
            var m = Make(3);
            int count = m.TileCount;
            float lastX = m.TilePositions[count - 1].X;
            int guard = 0;
            while (m.TilePositions[0].X + m.TileWidth >= -240f && guard++ < 1000) m.Update(0.1f, 300f);
            Assert.AreEqual(count, m.TileCount);
            Assert.Less(m.TilePositions[count - 1].X, lastX + m.TileWidth + 1f);
            for (int i = 1; i < count; i++)
                Assert.AreEqual(m.TileWidth, m.TilePositions[i].X - m.TilePositions[i - 1].X, 1e-3f);
        }

        [Test]
        public void TreesKeepBandOnRecycle()
        {
            var m = Make(5);
            int upper = m.Trees.Count(t => t.TrunkBase.Y < m.CenterY);
            int lower = m.Trees.Count - upper;
            for (int i = 0; i < 200; i++) m.Update(0.5f, 300f);
            Assert.AreEqual(upper, m.Trees.Count(t => t.TrunkBase.Y < m.CenterY));
            Assert.AreEqual(lower, m.Trees.Count(t => t.TrunkBase.Y >= m.CenterY));
        }

        [Test]
        public void DeterministicPerSeed()
        {
            var a = Make(11);
            var b = Make(11);
            var c = Make(12);
            for (int i = 0; i < 30; i++) { a.Update(0.5f, 300f); b.Update(0.5f, 300f); c.Update(0.5f, 300f); }
            for (int i = 0; i < a.Trees.Count; i++)
            {
                Assert.AreEqual(a.Trees[i].TrunkBase, b.Trees[i].TrunkBase);
                Assert.AreEqual(a.Trees[i].Scale, b.Trees[i].Scale);
            }
            Assert.IsTrue(a.TileTypes.SequenceEqual(b.TileTypes));
            Assert.IsFalse(a.Trees.Zip(c.Trees, (p, q) => p.TrunkBase == q.TrunkBase).All(x => x));
        }
    }
}
