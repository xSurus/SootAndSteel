using System.Collections.Generic;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Utils;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.Stations
{
    public class CannonStationRuntimeTests
    {
        private sealed class Spawner : IBulletItemSpawner
        {
            public readonly List<(Vector2 pos, Vector2 dir, BulletFaction faction, BulletRecipe recipe)> Calls =
                new List<(Vector2, Vector2, BulletFaction, BulletRecipe)>();

            public BulletRuntime Emit(BulletRecipe recipe, Vector2 pos, Vector2 dir, BulletFaction faction)
            {
                Calls.Add((pos, dir, faction, recipe));
                return null;
            }
        }

        private sealed class Grid : IStationGrid
        {
            public readonly Dictionary<GridDirection, StationRuntime> Map = new Dictionary<GridDirection, StationRuntime>();
            public StationRuntime GetAdjacentStation(Vector2 position, GridDirection d) =>
                Map.TryGetValue(d, out var s) ? s : null;
        }

        private sealed class Aim : ICannonAimSource
        {
            public System.Numerics.Vector2 Value;
            public System.Numerics.Vector2 GetMovement() => Value;
        }

        private readonly List<GameObject> made = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var g in made) Object.DestroyImmediate(g);
            made.Clear();
        }

        private T Make<T>(string id = "") where T : StationRuntime
        {
            var go = new GameObject(typeof(T).Name);
            made.Add(go);
            go.AddComponent<Rigidbody2D>();
            var s = go.AddComponent<T>();
            s.Initialize(new StationCatalogEntry { stationId = id });
            return s;
        }

        private BulletRackRuntime LoadedRack(string id)
        {
            var r = Make<BulletRackRuntime>();
            r.ReceiveItem(new BulletItem(id), null);
            return r;
        }

        private (CannonStationRuntime c, Spawner sp, Grid g) Setup()
        {
            var c = Make<CannonStationRuntime>();
            var sp = new Spawner();
            var g = new Grid();
            c.Spawner = sp;
            c.Grid = g;
            return (c, sp, g);
        }

        private static void Interact(CannonStationRuntime c) => ((IInteractable)c).OnInteract(null);

        [Test]
        public void Initialize_DefaultsStationId()
        {
            Assert.AreEqual(StationIds.Cannon, Make<CannonStationRuntime>().StationId);
        }

        [Test]
        public void Interact_LoadsFromRackAndFires_AtBarrelOffset_ThenCooldown()
        {
            var (c, sp, g) = Setup();
            c.Position = new Vector2(2f, 1f); // 200,100 px
            g.Map[GridDirection.Left] = LoadedRack(ComponentIds.BasicCasing);
            int fired = 0;
            c.Fired += () => fired++;

            Interact(c);

            Assert.AreEqual(1, sp.Calls.Count);
            var call = sp.Calls[0];
            Assert.AreEqual(BulletFaction.Player, call.faction);
            Assert.AreEqual(2.4f, call.pos.x, 1e-4f); // 200 + 40 px
            Assert.AreEqual(1f, call.pos.y, 1e-4f);
            Assert.AreEqual(1f, call.dir.x, 1e-5f);
            Assert.AreEqual(0.5f, c.CooldownTimer);
            Assert.IsNull(c.HeldItem);
            Assert.AreEqual(1, fired);
            Assert.IsNull(g.Map[GridDirection.Left].PeekNextItem());

            // cooldown refuses
            g.Map[GridDirection.Up] = LoadedRack(ComponentIds.BasicCasing);
            Interact(c);
            Assert.AreEqual(1, sp.Calls.Count);
            Assert.IsNotNull(g.Map[GridDirection.Up].PeekNextItem());
        }

        [Test]
        public void Fire_UsesAimAngle_YDown()
        {
            var (c, sp, g) = Setup();
            c.AimAngle = Mathf.PI / 2f; // +y
            c.HeldItem = new BulletItem(ComponentIds.BasicCasing);
            c.FireCannon();
            Assert.AreEqual(0f, sp.Calls[0].pos.x, 1e-4f);
            Assert.AreEqual(0.4f, sp.Calls[0].pos.y, 1e-4f);
            Assert.AreEqual(1f, sp.Calls[0].dir.y, 1e-5f);
        }

        [Test]
        public void Fire_NoHeldItem_IsNoOp()
        {
            var (c, sp, _) = Setup();
            int fired = 0;
            c.Fired += () => fired++;
            c.FireCannon();
            Interact(c); // no racks
            Assert.AreEqual(0, sp.Calls.Count);
            Assert.AreEqual(0, fired);
            Assert.AreEqual(0f, c.CooldownTimer);
        }

        [Test]
        public void Reload_LaterRackInEnumOrderOverwritesEarlier()
        {
            var (c, sp, g) = Setup();
            var up = LoadedRack(ComponentIds.BasicCasing);
            var right = LoadedRack(ComponentIds.ScatterCasing);
            g.Map[GridDirection.Up] = up;
            g.Map[GridDirection.Right] = right;
            Interact(c);
            Assert.AreEqual(1, sp.Calls.Count);
            Assert.AreEqual(ComponentIds.ScatterCasing, sp.Calls[0].recipe.ComponentIds[0]);
            Assert.IsNull(up.PeekNextItem()); // discarded bullet is gone from the rack
            Assert.IsNull(right.PeekNextItem());
        }

        [Test]
        public void Reload_IgnoresNonRackNeighbours()
        {
            var (c, sp, g) = Setup();
            g.Map[GridDirection.Down] = Make<WorkbenchRuntime>();
            Interact(c);
            Assert.AreEqual(0, sp.Calls.Count);
        }

        [Test]
        public void Update_DecrementsCooldownThenAims_AndNullSourceDoesNotAim()
        {
            var (c, _, _) = Setup();
            c.Update(1f);
            Assert.AreEqual(0f, c.AimAngle);

            var aim = new Aim { Value = new System.Numerics.Vector2(0, 1) };
            c.AimSource = aim;
            c.HeldItem = new BulletItem(ComponentIds.BasicCasing);
            c.FireCannon();
            c.Update(0.1f);
            Assert.AreEqual(0.4f, c.CooldownTimer, 1e-5f);
            Assert.AreEqual(0.6f, c.AimAngle, 1e-5f);
            aim.Value = System.Numerics.Vector2.Zero;
            c.Update(0.1f);
            Assert.AreEqual(0.6f, c.AimAngle, 1e-5f);
        }
    }
}
