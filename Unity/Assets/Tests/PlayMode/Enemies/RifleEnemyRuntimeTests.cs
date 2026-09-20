using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Config;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using NVector2 = System.Numerics.Vector2;

namespace Gamelab.Tests.Enemies
{
    public class RifleEnemyRuntimeTests
    {
        private sealed class FakeWorld : IEnemyWorld
        {
            public float MinX => 0f;
            public float MaxX => 1000f;
            public NVector2 Anchor = new NVector2(500f, 300f);
            public NVector2 Target = new NVector2(500f, 100f);
            public NVector2 GetSlotAnchor(EnemyTrainSlot slot, float d) => Anchor;
            public NVector2 GetTargetPoint(EnemySlotSide side, NVector2 p) => Target;
        }

        private sealed class RecordingSpawner : IBulletItemSpawner
        {
            public readonly List<(Vector2 pos, Vector2 dir, BulletFaction faction, BulletRecipe recipe)> Calls =
                new List<(Vector2, Vector2, BulletFaction, BulletRecipe)>();

            public BulletRuntime Emit(BulletRecipe recipe, Vector2 pos, Vector2 dir, BulletFaction faction)
            {
                Calls.Add((pos, dir, faction, recipe));
                return null;
            }
        }

        // NextDouble() == 0: initial timeSinceLastShot is 0, so cooldown timing is exact.
        private sealed class ZeroRandom : System.Random
        {
            protected override double Sample() => 0.0;
        }

        private readonly List<Object> created = new List<Object>();
        private readonly FakeWorld world = new FakeWorld();
        private readonly RecordingSpawner spawner = new RecordingSpawner();

        private T Make<T>() where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            created.Add(o);
            return o;
        }

        private BulletComponentCatalogAsset MakeCatalog()
        {
            BulletComponentAsset[] assets =
            {
                Make<BasicProjectileAsset>(), Make<FrangibleProjectileAsset>(), Make<PiercingProjectileAsset>(),
                Make<MatryoshkaProjectileAsset>(), Make<BasicCasingAsset>(), Make<ScatterCasingAsset>(),
                Make<BurstCasingAsset>(), Make<EnemyCasingAsset>(), Make<RapidFireCasingAsset>(),
                Make<BasicPropellantAsset>(), Make<HomingPropellantAsset>(), Make<HeavyPropellantAsset>(),
                Make<BoomerangPropellantAsset>()
            };
            var catalog = Make<BulletComponentCatalogAsset>();
            catalog.Configure(assets);
            return catalog;
        }

        // xPx/yPx in pixels. Update is disabled so only Step advances time.
        private T MakeEnemy<T>(float xPx, float yPx, float health = 100f) where T : RifleEnemyRuntime
        {
            var go = new GameObject("Rifle");
            go.AddComponent<Rigidbody2D>();
            go.transform.position = new Vector3(xPx / 100f, yPx / 100f, 0f);
            var e = go.AddComponent<T>();
            e.Initialize(new EnemyCatalogEntry { type = EnemyType.Dummy, health = health, size = 72f });
            e.Configure(EnemyTuning.Default, EnemyAmmoCatalog.GetAmmoDefinition(EnemyAmmoIds.Basic),
                new EnemyTrainSlot(EnemySlotSide.Top, 0.5f), world, spawner, new ZeroRandom());
            e.enabled = false;
            return e;
        }

        // Step is synchronous, so physics is stepped by hand (Script mode) for the velocity to move the body.
        private SimulationMode2D savedMode;

        [SetUp]
        public void SetUp()
        {
            savedMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        private static void Run(RifleEnemyRuntime e, float seconds)
        {
            for (float t = 0; t < seconds; t += 0.02f)
            {
                e.Step(0.02f);
                Physics2D.Simulate(0.02f);
            }
        }

        private BulletRuntime PlayerBullet()
        {
            var def = BulletDefinitionAsset.BuildFromRecipe(
                new BulletRecipe(EComponentType.Bullet,
                    new[] { ComponentIds.BasicCasing, ComponentIds.BasicPropellant, ComponentIds.BasicProjectile }),
                MakeCatalog(), BulletStats.CannonDefault());
            created.Add(def);
            return BulletRuntime.Spawn(def, new Vector2(0f, 50f), Vector2.right, BulletFaction.Player);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                if (go.name.StartsWith("Bullet_") || go.name == "Rifle") Object.Destroy(go);
            yield return null; // bullets are gone before their definition and components are destroyed
            foreach (var o in created) Object.Destroy(o);
            created.Clear();
            yield return null;
            Physics2D.simulationMode = savedMode;
        }

        [UnityTest]
        public IEnumerator ReachesApproachThenHolds_AtSlotAnchor()
        {
            var e = MakeEnemy<RifleEnemyRuntime>(900f, 300f);
            Assert.AreEqual(HorseState.ApproachingSideAttackSlot, e.Horse);
            Run(e, 6f);
            Assert.AreEqual(HorseState.HoldingSideAttackSlot, e.Horse);
            Run(e, 4f); // holding drives on to the slot anchor (500, 300) px = (5, 3) m
            Assert.Less(Vector2.Distance(e.PhysicsBody.position, new Vector2(5f, 3f)), 0.25f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FiresAfterOneSecondAim_TowardTarget()
        {
            var e = MakeEnemy<RifleEnemyRuntime>(900f, 300f);
            Run(e, 6f); // holding, cooldown (4 s) elapsed
            Assert.IsTrue(e.TryShoot());
            Assert.AreEqual(RiderState.Aiming, e.Rider);
            Run(e, 0.9f);
            Assert.AreEqual(0, spawner.Calls.Count);
            Run(e, 0.2f);
            Assert.AreEqual(1, spawner.Calls.Count);
            var call = spawner.Calls[0];
            Assert.AreEqual(BulletFaction.Enemy, call.faction);
            Assert.AreEqual(1f, call.dir.magnitude, 1e-4f);
            Assert.Less(call.dir.y, 0f); // target has the smaller Y (Src is y-down)
            // ZeroRandom gives spread = -0.5 * 0.2 = -0.1 rad off the aim direction.
            float aim = Mathf.Atan2(-1f, 0f);
            Assert.AreEqual(aim - 0.1f, Mathf.Atan2(call.dir.y, call.dir.x), 0.12f); // the body is still a few px off the anchor
            CollectionAssert.AreEqual(EnemyAmmoCatalog.GetAmmoDefinition(EnemyAmmoIds.Basic).OrderedComponentIds,
                call.recipe.ComponentIds);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CannotShootBeforeHolding()
        {
            var e = MakeEnemy<RifleEnemyRuntime>(900f, 300f);
            Assert.IsFalse(e.TryShoot());
            yield return null;
        }

        [UnityTest]
        public IEnumerator LethalPlayerHit_KeepsHealthOne_ThenFleesAfterDelay()
        {
            var e = MakeEnemy<RifleEnemyRuntime>(900f, 300f, 10f);
            var bullet = PlayerBullet();

            Assert.IsTrue(e.OnHit(bullet));
            Assert.AreEqual(1f, e.Health);
            Assert.IsTrue(e.IsAlive);
            Assert.IsTrue(e.WasNeutralized);
            Assert.AreEqual(RiderState.Dead, e.Rider);
            Assert.IsFalse(e.OnHit(bullet)); // rider already dead

            Run(e, 0.7f);
            Assert.AreNotEqual(HorseState.Fleeing, e.Horse);
            Run(e, 0.1f);
            Assert.AreEqual(HorseState.Fleeing, e.Horse);
            e.Step(0.02f);
            Assert.AreEqual(7.7f, e.PhysicsBody.linearVelocity.x, 1e-3f); // 700 * 1.1 px/s
            yield return null;
        }

        [UnityTest]
        public IEnumerator Fleeing_PastMaxXPlus600_IsRemoved()
        {
            var e = MakeEnemy<RifleEnemyRuntime>(900f, 300f, 10f);
            e.OnHit(PlayerBullet());
            Run(e, 0.8f);
            e.PhysicsBody.position = new Vector2(15.9f, 3f); // 1590 px < 1000 + 600
            e.Step(0.02f);
            Assert.IsFalse(e.ShouldRemove);
            e.PhysicsBody.position = new Vector2(16.1f, 3f); // 1610 px > 1600
            e.Step(0.02f);
            Assert.IsTrue(e.ShouldRemove);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyFactionBullet_IsIgnored()
        {
            var e = MakeEnemy<RifleEnemyRuntime>(900f, 300f);
            var def = BulletDefinitionAsset.BuildFromRecipe(
                EnemyAmmoCatalog.GetAmmoDefinition(EnemyAmmoIds.Basic).BuildRecipe(), MakeCatalog(),
                BulletStats.CannonDefault());
            created.Add(def);
            var bullet = BulletRuntime.Spawn(def, Vector2.zero, Vector2.right, BulletFaction.Enemy);
            Assert.IsFalse(e.OnHit(bullet));
            Assert.AreEqual(100f, e.Health);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TutorialEnemy_UsesTutorialHealthAndCooldown()
        {
            var e = MakeEnemy<TutorialEnemyRuntime>(900f, 300f);
            Assert.AreEqual(1f, e.Health);
            Run(e, 5f); // holding by now, but the tutorial cooldown is 6 s (rifle: 4 s)
            Assert.AreEqual(HorseState.HoldingSideAttackSlot, e.Horse);
            Assert.IsFalse(e.TryShoot());
            Run(e, 1.1f);
            Assert.IsTrue(e.TryShoot());
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealCatalogSpawner_EmitsEnemyBulletWithDamage10_AndDisposeCleansUp()
        {
            int before = Object.FindObjectsByType<BulletDefinitionAsset>(FindObjectsSortMode.None).Length;
            var sp = new CatalogBulletSpawner(MakeCatalog());
            var recipe = EnemyAmmoCatalog.GetAmmoDefinition(EnemyAmmoIds.Basic).BuildRecipe();
            BulletRuntime b = sp.Emit(recipe, Vector2.zero, Vector2.right, BulletFaction.Enemy);
            Assert.AreEqual(BulletFaction.Enemy, b.Faction);
            Assert.AreEqual(10f, b.Stats.Damage);
            BulletRuntime b2 = sp.Emit(recipe, Vector2.zero, Vector2.right, BulletFaction.Enemy);
            Assert.IsNotNull(b2);
            Object.Destroy(b.gameObject);
            Object.Destroy(b2.gameObject);
            yield return null;
            Assert.AreEqual(before + 1, Object.FindObjectsByType<BulletDefinitionAsset>(FindObjectsSortMode.None).Length);
            sp.Dispose();
            yield return null;
            Assert.AreEqual(before, Object.FindObjectsByType<BulletDefinitionAsset>(FindObjectsSortMode.None).Length);
        }
    }
}
