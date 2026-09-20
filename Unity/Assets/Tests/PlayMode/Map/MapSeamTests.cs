using System.Collections;
using System.Collections.Generic;
using Gamelab.Config;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.Items;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Tests.Bullets;
using Gamelab.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NVector2 = System.Numerics.Vector2;

namespace Gamelab.Tests.PlayMode
{
    public class MapSeamTests
    {
        private sealed class RecordingSpawner : IBulletItemSpawner
        {
            public readonly List<(Vector2 pos, Vector2 dir)> Calls = new List<(Vector2, Vector2)>();

            public BulletRuntime Emit(BulletRecipe recipe, Vector2 pos, Vector2 dir, BulletFaction faction)
            {
                Calls.Add((pos, dir));
                return null;
            }
        }

        private sealed class ZeroRandom : System.Random
        {
            protected override double Sample() => 0.0;
        }

        private TrainLayout layout;
        private TrainMapRuntime map;
        private readonly List<GameObject> made = new List<GameObject>();
        private SimulationMode2D savedMode;

        [SetUp]
        public void SetUp()
        {
            savedMode = Physics2D.simulationMode;
            var topLeft = TrainLayout.DefaultTopLeftPx(TrainTuning.ScreenWidth, TrainTuning.ScreenHeight,
                TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize);
            layout = new TrainLayout(TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize, topLeft);
            map = TrainMapRuntime.Create(layout, new System.Random(7));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Physics2D.simulationMode = savedMode;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go != null && go.name.StartsWith("Bullet_")) Object.Destroy(go);
            foreach (var go in made) if (go != null) Object.Destroy(go);
            made.Clear();
            if (map != null) Object.Destroy(map.gameObject);
            yield return null;
        }

        private T NewStation<T>() where T : StationRuntime
        {
            var go = new GameObject("S");
            made.Add(go);
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<T>();
            s.Initialize(new StationCatalogEntry { stationId = "" });
            return s;
        }

        private Vector2 CellCentre(int x, int y) => MapSpace.ToUnity(layout.GetTileCenterMeters(x, y));

        [UnityTest]
        public IEnumerator Attach_SetsGridAndSnaps_AdjacencyUsesMeters()
        {
            var conveyor = NewStation<ConveyorRuntime>();
            var cannon = NewStation<CannonStationRuntime>();
            var counter = NewStation<CounterRuntime>();
            yield return null;
            conveyor.Position = CellCentre(2, 1) + new Vector2(0.1f, 0.1f);
            cannon.Position = CellCentre(3, 1);
            counter.Position = CellCentre(2, 2);

            Assert.IsTrue(map.Attach(conveyor));
            Assert.IsTrue(map.Attach(cannon));
            Assert.IsTrue(map.Attach(counter));
            Assert.AreSame(map, conveyor.Grid);
            Assert.AreSame(map, cannon.Grid);
            Assert.AreEqual(CellCentre(2, 1).x, conveyor.Position.x, 1e-4f);

            Assert.AreSame(cannon, map.GetAdjacentStation(conveyor.Position, GridDirection.Right));
            Assert.AreSame(counter, map.GetAdjacentStation(conveyor.Position, GridDirection.Down));
            Assert.IsNull(map.GetAdjacentStation(conveyor.Position, GridDirection.Left));
        }

        [UnityTest]
        public IEnumerator Rifle_AgainstRealMap_AimsAtNearestTopWall()
        {
            Physics2D.simulationMode = SimulationMode2D.Script;
            var spawner = new RecordingSpawner();
            var go = new GameObject("Rifle");
            made.Add(go);
            go.AddComponent<Rigidbody2D>();
            go.transform.position = new Vector3(15f, 1f, 0f);
            var e = go.AddComponent<RifleEnemyRuntime>();
            e.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f });
            e.Configure(EnemyTuning.Default, EnemyAmmoCatalog.GetAmmoDefinition(EnemyAmmoIds.Basic),
                new EnemyTrainSlot(EnemySlotSide.Top, 0.5f), map, spawner, new ZeroRandom());
            e.Bounds = map;
            e.enabled = false;
            yield return null;

            for (float t = 0; t < 8f; t += 0.02f) { e.Step(0.02f); Physics2D.Simulate(0.02f); }
            Assert.IsTrue(e.TryShoot());
            for (float t = 0; t < 1.2f; t += 0.02f) { e.Step(0.02f); Physics2D.Simulate(0.02f); }
            Assert.AreEqual(1, spawner.Calls.Count);

            NVector2 posPx = MapSpace.ToNumerics(spawner.Calls[0].pos * WorldUnits.PixelsPerMeter);
            NVector2 best = default;
            float bestSq = float.MaxValue;
            foreach (WallSpec w in layout.WallSpecs)
            {
                if (!w.IsTop || w.Kind != WallKind.ShootHole) continue;
                float sq = NVector2.DistanceSquared(posPx, w.CenterPx);
                if (sq < bestSq) { bestSq = sq; best = w.CenterPx; }
            }
            Assert.AreEqual(best, map.GetTargetPoint(EnemySlotSide.Top, posPx));
            float expected = Mathf.Atan2(best.Y - posPx.Y, best.X - posPx.X) - 0.1f; // ZeroRandom spread
            Vector2 d = spawner.Calls[0].dir;
            Assert.AreEqual(expected, Mathf.Atan2(d.y, d.x), 1e-3f);
        }

        [Test]
        public void TargetPoint_SidesAndFallback_AndAnchor()
        {
            var enemy = new NVector2(700f, 100f);
            NVector2 top = map.GetTargetPoint(EnemySlotSide.Top, enemy);
            NVector2 bottom = map.GetTargetPoint(EnemySlotSide.Bottom, enemy);
            Assert.Less(top.Y, layout.GetBounds().Center.Y);
            Assert.Greater(bottom.Y, layout.GetBounds().Center.Y);
            var slot = new EnemyTrainSlot(EnemySlotSide.Top, 0.5f);
            Assert.AreEqual(slot.GetAnchor(layout.GetBounds(), 30f, layout.TileSize), map.GetSlotAnchor(slot, 30f));
            Assert.AreEqual(0f, map.MinX);
            Assert.AreEqual(1920f, map.MaxX);
            map.WorldBounds = new MapBounds(1000f);
            Assert.AreEqual(1000f, map.MaxX);
        }

        [UnityTest]
        public IEnumerator EnemyLeftOfMinX_IsFlaggedForRemoval_ViaMapBounds()
        {
            var go = new GameObject("Enemy_Test");
            made.Add(go);
            go.AddComponent<Rigidbody2D>();
            go.transform.position = new Vector3(-6f, 3f, 0f); // -600 px, past MinX - 72
            var e = go.AddComponent<DummyEnemyRuntime>();
            e.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f });
            e.PhysicsBody.position = new Vector2(-6f, 3f); // the cull reads the body position
            e.Bounds = map;
            yield return null;
            Assert.IsTrue(e.ShouldRemove);
        }

        [Test]
        public void SpeedLever_CyclesMembersOfTheSharedSet_AndLinkTracksBreaches()
        {
            var go = new GameObject("Train");
            made.Add(go);
            var train = go.AddComponent<TrainStateRuntime>();
            var lgo = new GameObject("Lever");
            made.Add(lgo);
            lgo.AddComponent<Rigidbody2D>();
            var lever = lgo.AddComponent<SpeedLeverRuntime>();
            lever.Initialize(new StationCatalogEntry { stationId = "" });
            lever.State = train;
            lever.Speeds = train.Speeds;

            Assert.AreSame(train.Speeds.Default, train.State.CurrentSpeed);
            ((IInteractable)lever).OnInteract(null);
            Assert.AreSame(train.Speeds.Fast, train.State.CurrentSpeed);
            ((IInteractable)lever).OnInteract(null);
            Assert.AreSame(train.Speeds.Slow, train.State.CurrentSpeed);
            Assert.AreSame(train.Speeds.Slow, train.CurrentSpeed);

            train.Tick(1f);
            Assert.Greater(train.State.DistanceTraveled, 0f);

            map.Link(train);
            ShootHoleWallRuntime wall = map.ShootHoleWalls[0];
            wall.TakeDamage(1000f);
            Assert.AreEqual(1, train.State.numberBreachedWalls);
            wall.Repair(10f);
            Assert.AreEqual(0, train.State.numberBreachedWalls);
        }

        private static BulletRuntime Bullet(BulletFaction faction, float damage) =>
            BulletRuntime.Spawn(BulletTestUtil.MakeBasicDefinition(damage: damage), new Vector2(0f, 50f), Vector2.right, faction);

        [UnityTest]
        public IEnumerator Wall_OnHit_OnlyEnemyBullets_SwapsSprite()
        {
            ShootHoleWallRuntime top = null, bottom = null;
            foreach (ShootHoleWallRuntime w in map.ShootHoleWalls)
            {
                if (w.IsTop && top == null) top = w;
                if (!w.IsTop && bottom == null) bottom = w;
            }
            Assert.AreEqual("WallTileTop", top.SpriteName);
            Assert.AreSame(MapSprites.Get("Walls/WallTileTop"), top.Visual.sprite);

            Assert.IsFalse(top.OnHit(Bullet(BulletFaction.Player, 20f)));
            Assert.AreEqual(50f, top.Health.Current);

            Assert.IsTrue(top.OnHit(Bullet(BulletFaction.Enemy, 20f))); // 40 percent
            Assert.AreEqual(30f, top.Health.Current, 1e-4f);
            Assert.AreEqual("WallTileTopBroken1_Variation" + top.Variation, top.SpriteName);
            Assert.AreSame(MapSprites.Get("Walls/" + top.SpriteName), top.Visual.sprite);
            // Feet stay at the collider bottom edge after the swap.
            Assert.AreEqual(top.GetComponent<BoxCollider2D>().bounds.max.y, top.Visual.bounds.max.y, 1e-3f);

            Assert.IsTrue(bottom.OnHit(Bullet(BulletFaction.Enemy, 50f)));
            Assert.AreEqual("WallTileBottomBroken3", bottom.SpriteName);
            Assert.IsFalse(bottom.OnHit(Bullet(BulletFaction.Enemy, 10f)), "broken wall does not consume");

            bottom.Repair(10f);
            Assert.AreEqual("WallTileBottom", bottom.SpriteName);
            yield return null;
        }
    }
}
