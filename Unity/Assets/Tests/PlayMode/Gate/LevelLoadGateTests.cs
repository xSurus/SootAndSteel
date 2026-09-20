using System.Collections;
using System.Collections.Generic;
using Gamelab.Enemies.Core;
using Gamelab.Levels;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gamelab.Tests.PlayMode.Gate
{
    // B1 gate: a full level loads with correct tile placement and working collision, matching the
    // reference build's layout. Expected numbers are hand-computed from Src formulas for height 5.
    public class LevelLoadGateTests
    {
        // First 50 draws of new System.Random(7).Next(0, 2) (computed with a scratch .NET program), index x * Height + y.
        private const string Variants7 = "01100101110100101010110000100110000010111100111110";

        private TrainLayout layout;
        private TrainMapRuntime map;
        private GameObject scroller;
        private GameObject trainGo;
        private GameObject levelGo;
        private GameObject extra;
        private LevelDefinition def;

        [SetUp]
        public void SetUp()
        {
            // (a) Level 3, seed 12345, golden from LevelGoldenTests: distance 23000, levelSeed 36102, 2 events.
            def = new ProceduralLevelGenerator(new LevelGenerationConfig { RandomSeed = 12345 }, 1f, 1f).Generate(3);
            var topLeft = TrainLayout.DefaultTopLeftPx(TrainTuning.ScreenWidth, TrainTuning.ScreenHeight,
                TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize);
            layout = new TrainLayout(TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize, topLeft);
            map = TrainMapRuntime.Create(layout, new System.Random(7));
            scroller = new GameObject("Scroller", typeof(WorldScrollerView));
            trainGo = new GameObject("Train", typeof(TrainStateRuntime));
            levelGo = new GameObject("Level", typeof(LevelRuntime));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject g in new[] { extra, levelGo, trainGo, scroller })
                if (g != null) Object.Destroy(g);
            if (map != null) Object.Destroy(map.gameObject);
        }

        private PhysicalEntity NewEntity(Vector2 pos)
        {
            var go = new GameObject("Entity");
            extra = go;
            var e = go.AddComponent<PhysicalEntity>();
            e.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            e.Position = pos;
            return e;
        }

        private static IEnumerator Push(PhysicalEntity e, Vector2 velocity, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                e.Body.linearVelocity = velocity;
                yield return new WaitForFixedUpdate();
            }
        }

        [Test]
        public void Level_MatchesPinnedGolden_AndEventsSorted()
        {
            Assert.AreEqual(23000f, def.LevelDistance);
            Assert.AreEqual(36102, def.LevelSeed);
            Assert.AreEqual(2, def.SpawnEvents.Count);
            for (int i = 1; i < def.SpawnEvents.Count; i++)
                Assert.GreaterOrEqual(def.SpawnEvents[i].Distance, def.SpawnEvents[i - 1].Distance);
        }

        [UnityTest]
        public IEnumerator TilePlacement_EveryCellMatchesLayout_AndWallColliderLiterals()
        {
            yield return null;
            Assert.AreEqual(560f, layout.Position.X);
            Assert.AreEqual(340f, layout.Position.Y);
            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    string name = Variants7[x * layout.Height + y] == '0' ? "Train_Tile_A" : "Train_Tile_B";
                    Assert.AreSame(MapSprites.Get(name), map.Floor.GetSprite(cell), "sprite " + x + "," + y);
                    Vector3 c = map.Floor.GetCellCenterWorld(cell);
                    // Src GetTileCenterPixels / 100: (560 + x*80 + 40, 340 + y*80 + 40).
                    Assert.AreEqual((560f + x * 80f + 40f) / 100f, c.x, 1e-4f);
                    Assert.AreEqual((340f + y * 80f + 40f) / 100f, c.y, 1e-4f);
                }
            }

            Assert.AreEqual(layout.WallSpecs.Count, map.Walls.Count);
            for (int i = 0; i < map.Walls.Count; i++)
            {
                WallSpec spec = layout.WallSpecs[i];
                Bounds b = map.Walls[i].GetComponent<BoxCollider2D>().bounds;
                Assert.AreEqual(spec.CenterPx.X / 100f, b.center.x, 1e-4f);
                Assert.AreEqual(spec.CenterPx.Y / 100f, b.center.y, 1e-4f);
                Assert.AreEqual(spec.SizePx.X / 100f, b.size.x, 1e-4f);
                Assert.AreEqual(spec.SizePx.Y / 100f, b.size.y, 1e-4f);
            }

            // Src literals for height 5, top-left (560,340), bounds (560,340,800,400). Column 0 top wall centre
            // = (600, tileCenter(0,0).y - 59.5) = (600, 380 - 59.5 = 320.5). Bottom wall centre
            // = (600, tileCenter(0,4).y + 60) = (600, 700 + 60 = 760).
            Bounds top = map.Walls[0].GetComponent<BoxCollider2D>().bounds;
            Assert.AreEqual(6.0f, top.center.x, 1e-4f);
            Assert.AreEqual(3.205f, top.center.y, 1e-4f);
            Assert.IsTrue(layout.WallSpecs[0].IsTop);
            Bounds bottom = map.Walls[1].GetComponent<BoxCollider2D>().bounds;
            Assert.IsFalse(layout.WallSpecs[1].IsTop);
            Assert.AreEqual(6.0f, bottom.center.x, 1e-4f);
            Assert.AreEqual(7.6f, bottom.center.y, 1e-4f);
            Assert.AreEqual(5.6f, layout.GetBounds().X / 100f, 1e-4f);
            Assert.AreEqual(3.4f, layout.GetBounds().Y / 100f, 1e-4f);
            Assert.AreEqual(800, layout.GetBounds().Width);
            Assert.AreEqual(400, layout.GetBounds().Height);
        }

        [UnityTest]
        public IEnumerator Collision_LeftWallBlocks_DoorGapPasses_ShootHoleWallsBlock_StationSnaps()
        {
            yield return null;
            float wallEdge = layout.Position.X / 100f;
            Assert.AreEqual(1, layout.DoorRowMin);
            Assert.AreEqual(2, layout.DoorRowMax);

            PhysicalEntity e = NewEntity(MapSpace.ToUnity(layout.GetTileCenterMeters(0, 0)));
            yield return Push(e, new Vector2(-3f, 0f), 60);
            Assert.Greater(e.Position.x, wallEdge, "left wall row 0 blocks, at " + e.Position);
            Object.Destroy(extra);
            yield return null;

            foreach (int row in new[] { 1, 2 })
            {
                e = NewEntity(MapSpace.ToUnity(layout.GetTileCenterMeters(0, row)));
                yield return Push(e, new Vector2(-3f, 0f), 60);
                Assert.Less(e.Position.x, wallEdge - 0.2f, "gap row " + row + " passes, at " + e.Position);
                Object.Destroy(extra);
                yield return null;
            }

            // Top wall spans y 3.005..3.405 m: pushing up from row 0 stops below its lower edge.
            e = NewEntity(MapSpace.ToUnity(layout.GetTileCenterMeters(0, 0)));
            yield return Push(e, new Vector2(0f, -3f), 60);
            Assert.Greater(e.Position.y, 3.405f, "top shoot hole wall blocks, at " + e.Position);
            Object.Destroy(extra);
            yield return null;

            // Bottom wall centre 7.6 m, half height 0.2: spans 7.4..7.8. Push down from row 4 (centre 7.0 m).
            e = NewEntity(MapSpace.ToUnity(layout.GetTileCenterMeters(9, 4)));
            yield return Push(e, new Vector2(0f, 3f), 60);
            Assert.Less(e.Position.y, 7.4f, "bottom shoot hole wall blocks, at " + e.Position);
            Object.Destroy(extra);
            yield return null;

            extra = new GameObject("Counter");
            extra.AddComponent<Rigidbody2D>();
            var counter = extra.AddComponent<CounterRuntime>();
            counter.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            yield return null;
            counter.Position = MapSpace.ToUnity(layout.GetTileCenterMeters(3, 2)) + new Vector2(0.2f, -0.1f);
            Assert.IsTrue(map.SnapToNearestValidCell(counter));
            Vector2 centre = MapSpace.ToUnity(layout.GetTileCenterMeters(3, 2));
            Assert.AreEqual(centre.x, counter.Position.x, 1e-4f);
            Assert.AreEqual(centre.y, counter.Position.y, 1e-4f);
        }

        [UnityTest]
        public IEnumerator RunLevel_CompletesOnceAtDistanceWithNoThreats_AndDeliversEveryEventInOrder()
        {
            var train = trainGo.GetComponent<TrainStateRuntime>();
            var level = levelGo.GetComponent<LevelRuntime>();
            bool threats = true;
            int completions = 0;
            var delivered = new List<SpawnEvent>();
            level.Initialize(def, train, () => threats);
            level.LevelCompleted += () => completions++;
            level.SpawnDue += delivered.Add;
            Assert.AreSame(def, level.Definition);

            // The train ticks itself in Update, so this frame does it by hand and Tick() does not tick it.
            int guard = 0;
            while (level.DistanceTraveled < def.LevelDistance && guard++ < 10000)
            {
                train.Tick(1f);
                level.Tick();
            }
            Assert.GreaterOrEqual(level.DistanceTraveled, def.LevelDistance);
            Assert.AreEqual(0, completions, "threats active");
            Assert.AreEqual(def.SpawnEvents.Count, delivered.Count);
            for (int i = 0; i < delivered.Count; i++) Assert.AreSame(def.SpawnEvents[i], delivered[i]);

            threats = false;
            level.Tick();
            level.Tick();
            Assert.AreEqual(1, completions);
            Assert.AreEqual(def.SpawnEvents.Count, delivered.Count, "each event exactly once");
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemySeams_AnchorAndTargetLieOnEventSide()
        {
            yield return null;
            float centerY = layout.GetBounds().Center.Y;
            foreach (SpawnEvent e in def.SpawnEvents)
            {
                bool top = e.Side == "Top";
                EnemySlotSide side = top ? EnemySlotSide.Top : EnemySlotSide.Bottom;
                var slot = new EnemyTrainSlot(side, 0);
                System.Numerics.Vector2 anchor = map.GetSlotAnchor(slot, 300f);
                System.Numerics.Vector2 target = map.GetTargetPoint(side, anchor);
                if (top)
                {
                    Assert.Less(anchor.Y, centerY);
                    Assert.Less(target.Y, centerY);
                }
                else
                {
                    Assert.Greater(anchor.Y, centerY);
                    Assert.Greater(target.Y, centerY);
                }
            }
        }
    }
}
