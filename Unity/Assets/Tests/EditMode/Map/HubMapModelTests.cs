using System;
using System.Linq;
using System.Numerics;
using Gamelab.Map;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class HubMapModelTests
    {
        private const float Tol = 1e-3f;

        private static HubMapModel Make() => new HubMapModel(2160, 2160);

        // Golden values: Src/Map/HubMap.cs (stubbed engine types, logic lines kept) at worldW = worldH = 2160.
        private static readonly (float x, float y, int variant)[] Stakes =
        {
            (120f, 1080f, 0),
            (215f, 1080f, 1),
            (310f, 1080f, 2),
            (405f, 1080f, 0),
            (500f, 1080f, 1),
            (595f, 1080f, 2),
            (690f, 1080f, 0),
            (785f, 1080f, 1),
            (880f, 1080f, 2),
            (1260f, 1080f, 0),
            (1355f, 1080f, 1),
            (1450f, 1080f, 2),
            (1545f, 1080f, 0),
            (1640f, 1080f, 1),
            (1735f, 1080f, 2),
            (1830f, 1080f, 0),
            (1925f, 1080f, 1),
            (2020f, 1080f, 2),
            (120f, 120f, 0),
            (120f, 215f, 1),
            (120f, 310f, 2),
            (120f, 405f, 0),
            (120f, 500f, 1),
            (120f, 595f, 2),
            (120f, 690f, 0),
            (120f, 785f, 1),
            (120f, 880f, 2),
            (120f, 975f, 0),
            (2040f, 120f, 1),
            (2040f, 215f, 2),
            (2040f, 310f, 0),
            (2040f, 405f, 1),
            (2040f, 500f, 2),
            (2040f, 595f, 0),
            (2040f, 690f, 1),
            (2040f, 785f, 2),
            (2040f, 880f, 0),
            (2040f, 975f, 1),
        };

        [Test]
        public void Stakes_MatchSrc_PositionsAndVariants()
        {
            HubMapModel m = Make();
            Assert.AreEqual(Stakes.Length, m.Stakes.Count);
            for (int i = 0; i < Stakes.Length; i++)
            {
                Assert.AreEqual(Stakes[i].x, m.Stakes[i].Feet.X, Tol, "x " + i);
                Assert.AreEqual(Stakes[i].y, m.Stakes[i].Feet.Y, Tol, "y " + i);
                Assert.AreEqual(Stakes[i].variant, m.Stakes[i].Variant, "variant " + i);
            }
            Assert.AreEqual("Stake2", m.Stakes[1].TextureKey);
        }

        [Test]
        public void StakeCollider_Is45x80_CentredHalfHeightAboveFeet()
        {
            HubMapModel.BoxPx c = Make().Stakes[0].Collider;
            Assert.AreEqual(120f, c.Center.X, Tol);
            Assert.AreEqual(1040f, c.Center.Y, Tol);
            Assert.AreEqual(45f, c.Size.X, Tol);
            Assert.AreEqual(80f, c.Size.Y, Tol);
        }

        [Test]
        public void Rects_MatchSrc()
        {
            HubMapModel m = Make();
            Assert.AreEqual(new int[] { 120, 120, 1920, 800 },
                new[] { m.ShoppingArea.X, m.ShoppingArea.Y, m.ShoppingArea.Width, m.ShoppingArea.Height });
            Assert.AreEqual(new int[] { 120, 120, 1920, 960 },
                new[] { m.VillageRect.X, m.VillageRect.Y, m.VillageRect.Width, m.VillageRect.Height });
        }

        [Test]
        public void BoundaryWalls_MatchSrc()
        {
            var w = Make().BoundaryWalls;
            var expect = new[]
            {
                (1080f, -0.5f, 2160f, 1f), (1080f, 2160.5f, 2160f, 1f),
                (-0.5f, 1080f, 1f, 2160f), (2160.5f, 1080f, 1f, 2160f)
            };
            Assert.AreEqual(4, w.Count);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(expect[i].Item1, w[i].Center.X, Tol);
                Assert.AreEqual(expect[i].Item2, w[i].Center.Y, Tol);
                Assert.AreEqual(expect[i].Item3, w[i].Size.X, Tol);
                Assert.AreEqual(expect[i].Item4, w[i].Size.Y, Tol);
            }
        }

        [Test]
        public void HouseColliders_MatchSrc()
        {
            var h = Make().HouseColliders;
            Assert.AreEqual(2, h.Count);
            Assert.AreEqual(540f, h[0].Center.X, Tol);
            Assert.AreEqual(150f, h[0].Center.Y, Tol);
            Assert.AreEqual(614.4f, h[0].Size.X, Tol);
            Assert.AreEqual(532.8f, h[0].Size.Y, Tol);
            Assert.AreEqual(1620f, h[1].Center.X, Tol);
            Assert.AreEqual(553.8f, h[1].Size.X, Tol);
            Assert.AreEqual(529.5f, h[1].Size.Y, Tol);
        }

        [Test]
        public void Trees_MatchSrc()
        {
            var t = Make().Trees;
            var expect = new[]
            {
                (60f, 264f, 0.55f), (20f, 600f, 0.495f), (90f, 936f, 0.605f),
                (2100f, 264f, 0.55f), (2140f, 600f, 0.5775f), (2080f, 936f, 0.5225f)
            };
            Assert.AreEqual(6, t.Count);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(expect[i].Item1, t[i].Feet.X, Tol);
                Assert.AreEqual(expect[i].Item2, t[i].Feet.Y, Tol);
                Assert.AreEqual(expect[i].Item3, t[i].Scale, Tol);
            }
        }

        [Test]
        public void Npcs_AndRails_MatchSrcFormulas()
        {
            HubMapModel m = Make();
            Assert.AreEqual(new Vector2(1260f, 700f), m.Vendor.Feet);
            Assert.AreEqual(new Vector2(500f, 580f), m.Town1.Feet);
            Assert.AreEqual(5400, m.ViewPad); // ceil(2160 / 0.4)
            Assert.AreEqual(326, m.RailTileWidth);
            Assert.AreEqual(1221f, m.RailDrawY, Tol); // 1620 - 409 + 40 - 30
            Assert.AreEqual(-17, m.RailStartColumn); // floor(-5400 / 326)
            Assert.AreEqual(42, m.RailColumnCount); // 12960 / 326 + 3
        }

        [Test]
        public void PrepTrainTopLeft_MatchesHubScreen()
        {
            Assert.AreEqual(new Vector2(680f, 1240f), HubMapModel.PrepTrainTopLeft(2160, 1080, 800));
        }

        [Test]
        public void OfferPositions_CentredOnShopRow()
        {
            Vector2[] p = Make().OfferPositions(4);
            Assert.AreEqual(new[] { 930f, 1030f, 1130f, 1230f }, p.Select(v => v.X).ToArray());
            Assert.IsTrue(p.All(v => v.Y == 570f));
        }

        [Test]
        public void SelectOfferIndices_MatchesSrcOrderBy_AndIsDeterministic()
        {
            // Src ran with a 10 item catalog, indices are the catalog order.
            Assert.AreEqual(new[] { 6, 3, 4, 0 }, HubMapModel.SelectOfferIndices(10, new Random(7), 4));
            Assert.AreEqual(new[] { 6, 5, 7, 8 }, HubMapModel.SelectOfferIndices(10, new Random(123), 4));
            Assert.AreEqual(new[] { 0, 1 }, HubMapModel.SelectOfferIndices(2, new Random(7), 4).OrderBy(i => i).ToArray());
        }

        [Test]
        public void RestockSeed_MatchesSrc()
        {
            Assert.AreEqual(1000 + 3 * 4242, HubMapModel.RestockSeed(1000, 3));
            Assert.AreEqual(unchecked(int.MaxValue + 2 * 4242), HubMapModel.RestockSeed(int.MaxValue, 2));
        }
    }
}
