using System;
using System.Collections.Generic;
using System.Numerics;
using Gamelab.Enemies.Core;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Utils;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class TrainGeometryTests
    {
        private static TrainLayout Make(params DoorSpec[] doors)
        {
            Vector2 tl = TrainLayout.DefaultTopLeftPx(1920, 1080, 10, 6, 80);
            return new TrainLayout(10, 6, 80, tl, doors);
        }

        [Test]
        public void DefaultTopLeft()
        {
            Assert.AreEqual(new Vector2(560, 300), TrainLayout.DefaultTopLeftPx(1920, 1080, 10, 6, 80));
            Assert.AreEqual(new Vector2(560, 340), TrainLayout.DefaultTopLeftPx(1920, 1080, 10, 5, 80));
            Assert.AreEqual(5, TrainTuning.Height);
        }

        [Test]
        public void TileCenters()
        {
            TrainLayout l = Make();
            Assert.AreEqual(new Vector2(600, 340), l.GetTileCenterPixels(0, 0));
            Assert.AreEqual(new Vector2(6.0f, 3.4f), l.GetTileCenterMeters(0, 0));
            Assert.AreEqual(new Vector2(560 + 3 * 80, 300 + 2 * 80), l.GetTileTopLeftPixels(3, 2));
        }

        [Test]
        public void IndexFloorsNegatives()
        {
            TrainLayout l = Make();
            Assert.AreEqual(new TrainPoint(-1, 0), l.GetTileIndexFromPixels(new Vector2(559, 300)));
            Assert.AreEqual(new TrainPoint(0, -1), l.GetTileIndexFromPixels(new Vector2(560, 299.9f)));
            Assert.AreEqual(new TrainPoint(1, 1), l.GetTileIndexFromPixels(new Vector2(640, 380)));
        }

        [Test]
        public void Bounds()
        {
            RectPx b = Make().GetBounds();
            Assert.AreEqual(new RectPx(560, 300, 800, 480), b);
            Assert.AreEqual(new TrainPoint(960, 540), b.Center);
            Assert.AreEqual(1360, b.Right);
            Assert.AreEqual(780, b.Bottom);
        }

        [Test]
        public void RectContainsIsHalfOpen()
        {
            var r = new RectPx(10, 10, 5, 5);
            Assert.IsTrue(r.Contains(10, 10));
            Assert.IsTrue(r.Contains(14, 14));
            Assert.IsFalse(r.Contains(15, 12));
            Assert.IsFalse(r.Contains(12, 15));
            Assert.IsFalse(r.Contains(9, 12));
            Assert.AreEqual(new TrainPoint(12, 12), r.Center);
        }

        [Test]
        public void IsOnTrainWithWagon()
        {
            TrainLayout l = Make();
            Assert.IsTrue(l.IsOnTrain(new Vector2(560.9f, 300.9f)));
            Assert.IsFalse(l.IsOnTrain(new Vector2(1360, 400)));
            Assert.IsFalse(l.IsOnTrain(new Vector2(400, 400)));
            l.CannonWagonBounds = new RectPx(320, 300, 240, 480);
            Assert.IsTrue(l.IsOnTrain(new Vector2(400, 400)));
        }

        [Test]
        public void WallSpecs()
        {
            TrainLayout l = Make();
            Assert.AreEqual(20, l.WallSpecs.Count);
            WallSpec top = l.WallSpecs[0];
            Assert.IsTrue(top.IsTop);
            Assert.AreEqual(WallKind.ShootHole, top.Kind);
            Assert.AreEqual(new Vector2(600, 280.5f), top.CenterPx);
            Assert.AreEqual(new Vector2(80, 40), top.SizePx);
            WallSpec bottom = l.WallSpecs[1];
            Assert.IsFalse(bottom.IsTop);
            Assert.AreEqual(new Vector2(600, 800), bottom.CenterPx);
            Assert.AreEqual(new Vector2(680, 280.5f), l.WallSpecs[2].CenterPx);
        }

        [Test]
        public void DoorReplacesShootHole()
        {
            TrainLayout l = Make(new DoorSpec(true, 4), new DoorSpec(false, 7));
            int doors = 0;
            foreach (WallSpec w in l.WallSpecs) if (w.Kind == WallKind.Door) doors++;
            Assert.AreEqual(2, doors);
            Assert.AreEqual(WallKind.Door, l.WallSpecs[4 * 2 + 1].Kind);
            Assert.AreEqual(WallKind.ShootHole, l.WallSpecs[4 * 2].Kind);
            Assert.AreEqual(WallKind.Door, l.WallSpecs[7 * 2].Kind);
            Assert.AreEqual(WallKind.ShootHole, l.WallSpecs[7 * 2 + 1].Kind);
        }

        [Test]
        public void LeftWallExcludesDoorRows()
        {
            TrainLayout l = Make();
            Assert.AreEqual(2, l.DoorRowMin);
            Assert.AreEqual(3, l.DoorRowMax);
            var ys = new List<int>();
            foreach (TrainPoint p in l.LeftWallTiles)
            {
                Assert.AreEqual(-1, p.X);
                ys.Add(p.Y);
            }
            CollectionAssert.AreEqual(new[] { 0, 1, 4, 5 }, ys);
        }

        [Test]
        public void VariantsMatchXMajorRandom()
        {
            TrainLayout l = Make();
            l.RollTileVariants(new Random(42));
            var rng = new Random(42);
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 6; y++)
                    Assert.AreEqual(rng.Next(0, 2), l.GetTileVariant(x, y));
            TrainLayout m = Make();
            m.RollTileVariants(new Random(42));
            Assert.AreEqual(l.GetTileVariant(9, 5), m.GetTileVariant(9, 5));
        }

        [Test]
        public void SpawnPreferenceOrder()
        {
            TrainLayout l = Make();
            var chosen = new HashSet<TrainPoint>();
            Func<TrainPoint, bool> free = p => false;
            Assert.AreEqual(l.GetTileCenterPixels(0, 3), l.GetFreeSpawnTile(0, chosen, free));
            Assert.AreEqual(l.GetTileCenterPixels(2, 3), l.GetFreeSpawnTile(2, chosen, free));
            Assert.AreEqual(l.GetTileCenterPixels(9, 3), l.GetFreeSpawnTile(9, chosen, free));
            // Same start again: shifts along the row.
            Assert.AreEqual(l.GetTileCenterPixels(3, 3), l.GetFreeSpawnTile(2, chosen, free));
        }

        [Test]
        public void SpawnFallsToRowsAboveThenBelow()
        {
            TrainLayout l = Make();
            var chosen = new HashSet<TrainPoint>();
            Func<TrainPoint, bool> centerRowTaken = p => p.Y == 3;
            Assert.AreEqual(l.GetTileCenterPixels(0, 2), l.GetFreeSpawnTile(0, chosen, centerRowTaken));
            Func<TrainPoint, bool> rows23 = p => p.Y == 3 || p.Y == 2;
            Assert.AreEqual(l.GetTileCenterPixels(1, 4), l.GetFreeSpawnTile(1, chosen, rows23));
        }

        [Test]
        public void SpawnFallbackWhenFull()
        {
            TrainLayout l = Make();
            var chosen = new HashSet<TrainPoint>();
            Vector2 v = l.GetFreeSpawnTile(5, chosen, p => true);
            Assert.AreEqual(l.GetTileCenterPixels(5, 3), v);
            Assert.IsTrue(chosen.Contains(new TrainPoint(5, 3)));
        }

        [Test]
        public void AnchorTopAndBottom()
        {
            var slotTop = new EnemyTrainSlot(EnemySlotSide.Top, 0.25f);
            var slotBot = new EnemyTrainSlot(EnemySlotSide.Bottom, 0.5f);
            var b = new RectPx(560, 300, 800, 480);
            Assert.AreEqual(new Vector2(760, 300 - 50 - 80), slotTop.GetAnchor(b, 50, 80));
            Assert.AreEqual(new Vector2(960, 780 + 50), slotBot.GetAnchor(b, 50, 80));
        }

        [Test]
        public void MapBoundsValues()
        {
            var m = new MapBounds();
            Assert.AreEqual(0f, m.MinX);
            Assert.AreEqual(1920f, m.MaxX);
        }

        [Test]
        public void TargetPointPicksNearestOnSide()
        {
            var b = new RectPx(560, 300, 800, 480);
            var walls = new List<Vector2> { new Vector2(600, 280), new Vector2(900, 280), new Vector2(600, 800), new Vector2(900, 800) };
            Assert.AreEqual(new Vector2(900, 280), EnemyWorldMath.GetTargetPoint(walls, EnemySlotSide.Top, new Vector2(950, 0), b));
            Assert.AreEqual(new Vector2(600, 800), EnemyWorldMath.GetTargetPoint(walls, EnemySlotSide.Bottom, new Vector2(500, 1000), b));
        }

        [Test]
        public void TargetPointTieFirstWinsAndFallback()
        {
            var b = new RectPx(560, 300, 800, 480);
            var walls = new List<Vector2> { new Vector2(600, 280), new Vector2(800, 280) };
            Assert.AreEqual(new Vector2(600, 280), EnemyWorldMath.GetTargetPoint(walls, EnemySlotSide.Top, new Vector2(700, 0), b));
            // Wall exactly at centre Y counts for neither side.
            var mid = new List<Vector2> { new Vector2(600, 540) };
            Assert.AreEqual(new Vector2(960, 540), EnemyWorldMath.GetTargetPoint(mid, EnemySlotSide.Top, Vector2.Zero, b));
            Assert.AreEqual(new Vector2(960, 540), EnemyWorldMath.GetTargetPoint(new List<Vector2>(), EnemySlotSide.Bottom, Vector2.Zero, b));
        }

        private class Station { }

        [Test]
        public void GridRegisterMovesCell()
        {
            var g = new TrainGrid<Station>(Make());
            var s = new Station();
            g.Register(new TrainPoint(1, 1), s);
            g.Register(new TrainPoint(2, 1), s);
            Assert.IsFalse(g.IsOccupied(new TrainPoint(1, 1)));
            Assert.IsTrue(g.TryGet(new TrainPoint(2, 1), out Station got));
            Assert.AreSame(s, got);
            g.Remove(s);
            Assert.IsFalse(g.IsOccupied(new TrainPoint(2, 1)));
        }

        [Test]
        public void GridAdjacentAllDirections()
        {
            TrainLayout l = Make();
            var g = new TrainGrid<Station>(l);
            var up = new Station(); var down = new Station(); var left = new Station(); var right = new Station();
            g.Register(new TrainPoint(3, 1), up);
            g.Register(new TrainPoint(3, 3), down);
            g.Register(new TrainPoint(2, 2), left);
            g.Register(new TrainPoint(4, 2), right);
            Vector2 pos = l.GetTileCenterPixels(3, 2);
            Assert.AreSame(up, g.GetAdjacent(pos, GridDirection.Up));
            Assert.AreSame(down, g.GetAdjacent(pos, GridDirection.Down));
            Assert.AreSame(left, g.GetAdjacent(pos, GridDirection.Left));
            Assert.AreSame(right, g.GetAdjacent(pos, GridDirection.Right));
            Assert.IsNull(g.GetAdjacent(l.GetTileCenterPixels(9, 5), GridDirection.Right));
        }

        [Test]
        public void SnapCellMainGridWagonAndOutside()
        {
            TrainLayout l = Make();
            var wagon = new RectPx(320, 300, 240, 480); // x -3..-1 tiles
            Assert.AreEqual(new TrainPoint(2, 1), l.SnapCell(l.GetTileCenterPixels(2, 1), wagon));
            // Tile x=-1, y=5 lies in the wagon; clamps to (-2, 3).
            TrainPoint? c = l.SnapCell(l.GetTileCenterPixels(-1, 5), wagon);
            Assert.AreEqual(new TrainPoint(-2, 3), c);
            // Tile x=-3, y=0 stays put in x, y stays 0.
            Assert.AreEqual(new TrainPoint(-3, 0), l.SnapCell(l.GetTileCenterPixels(-3, 0), wagon));
            Assert.IsNull(l.SnapCell(l.GetTileCenterPixels(-6, 1), wagon));
            Assert.IsNull(l.SnapCell(l.GetTileCenterPixels(-1, 1), null));
        }
    }
}
