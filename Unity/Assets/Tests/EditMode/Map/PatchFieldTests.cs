using System;
using System.Collections.Generic;
using System.Numerics;
using Gamelab.Config;
using Gamelab.Map;
using Gamelab.Map.Train;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class PatchFieldTests
    {
        // 10 x 5 tiles of 80 px at the origin: 50 tiles. At temperature 0 the targets are
        // 50 * 0.4 = 20 snow and 50 * 0.3 = 15 ice.
        private static readonly TrainStateTuning T = TrainStateTuning.Default;

        private static PatchField Make(int seed = 1, Func<TrainPoint, bool> occ = null)
        {
            var layout = new TrainLayout(10, 5, 80, Vector2.Zero);
            return new PatchField(layout, T, new Random(seed), occ ?? (_ => false));
        }

        [Test]
        public void NoPatches_AtOrAboveThreshold()
        {
            var f = Make();
            for (int i = 0; i < 20; i++) f.Update(1f, 85f);
            Assert.AreEqual(0, f.SnowTiles.Count);
            Assert.AreEqual(0, f.IceTiles.Count);
        }

        [Test]
        public void TargetCount_Formula()
        {
            var f = Make();
            Assert.AreEqual(0, f.TargetCount(0.85f, 0.85f, 0.4f));
            Assert.AreEqual(20, f.TargetCount(0f, 0.85f, 0.4f));
            Assert.AreEqual(10, f.TargetCount(0.5f, 1f, 0.4f));
        }

        [Test]
        public void SpawnsOnePerInterval()
        {
            var f = Make();
            f.Update(1.9f, 0f);
            Assert.AreEqual(0, f.SnowTiles.Count);
            f.Update(0.1f, 0f);
            Assert.AreEqual(1, f.SnowTiles.Count);
            Assert.AreEqual(0, f.IceTiles.Count);
            f.Update(1f, 0f);
            Assert.AreEqual(1, f.IceTiles.Count);
        }

        [Test]
        public void MeltsOnePerInterval_WhenAboveTarget()
        {
            var f = Make();
            for (int i = 0; i < 6; i++) f.Update(2f, 0f);
            int snow = f.SnowTiles.Count;
            Assert.Greater(snow, 1);
            f.Update(3.9f, 100f);
            Assert.AreEqual(snow, f.SnowTiles.Count);
            f.Update(0.1f, 100f);
            Assert.AreEqual(snow - 1, f.SnowTiles.Count);
        }

        [Test]
        public void OccupiedTiles_AreNeverChosen_AndSnowIceDoNotOverlap()
        {
            var f = Make(3, t => t.X < 5);
            for (int i = 0; i < 200; i++) f.Update(3f, 0f);
            Assert.Greater(f.SnowTiles.Count, 0);
            foreach (var t in f.SnowTiles) Assert.GreaterOrEqual(t.X, 5);
            foreach (var t in f.IceTiles) Assert.GreaterOrEqual(t.X, 5);
            var seen = new HashSet<TrainPoint>(f.SnowTiles);
            foreach (var t in f.IceTiles) Assert.IsTrue(seen.Add(t));
        }

        [Test]
        public void Deterministic_ForSeed()
        {
            var a = Make(42);
            var b = Make(42);
            for (int i = 0; i < 30; i++)
            {
                float temp = i < 20 ? 0f : 100f;
                a.Update(2f, temp);
                b.Update(2f, temp);
            }
            CollectionAssert.AreEqual(a.SnowTiles, b.SnowTiles);
            CollectionAssert.AreEqual(a.IceTiles, b.IceTiles);
            Assert.AreEqual(a.Version, b.Version);
        }

        [Test]
        public void Removal_ByPixel_ClearsQueriesAndBumpsVersion()
        {
            var f = Make();
            f.Update(2f, 0f);
            var tile = f.SnowTiles[0];
            var px = new Vector2(tile.X * 80 + 40, tile.Y * 80 + 40);
            Assert.IsTrue(f.IsOnSnow(px));
            Assert.IsFalse(f.IsOnIce(px));
            int v = f.Version;
            Assert.IsTrue(f.TryRemovePatchAt(px));
            Assert.Greater(f.Version, v);
            Assert.IsFalse(f.IsOnSnow(px));
            Assert.AreEqual(0, f.SnowTiles.Count);
            Assert.IsFalse(f.TryRemovePatchAt(px));
        }
    }
}
