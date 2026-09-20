using System;
using System.Linq;
using NUnit.Framework;
using Gamelab.Enemies.Core;
using Gamelab.Items.Bullets;

namespace Gamelab.Tests.Enemies
{
    public class EnemyAmmoCatalogTests
    {
        private static readonly (string id, int minLevel, int surcharge, float weight, string extra)[] Expected =
        {
            (EnemyAmmoIds.Basic, 1, 0, 1.6f, null), (EnemyAmmoIds.Heavy, 3, 1, 0.8f, ComponentIds.HeavyPropellant),
            (EnemyAmmoIds.Scatter, 3, 1, 0.8f, ComponentIds.ScatterCasing), (EnemyAmmoIds.Frangible, 4, 1, 0.7f, ComponentIds.FrangibleProjectile),
            (EnemyAmmoIds.Burst, 5, 2, 0.55f, ComponentIds.BurstCasing), (EnemyAmmoIds.Piercing, 5, 2, 0.5f, ComponentIds.PiercingProjectile),
            (EnemyAmmoIds.RapidFire, 6, 3, 0.35f, ComponentIds.RapidFireCasing), (EnemyAmmoIds.Homing, 7, 3, 0.3f, ComponentIds.HomingPropellant),
            (EnemyAmmoIds.Matryoshka, 8, 4, 0.2f, ComponentIds.MatryoshkaProjectile)
        };

        [Test]
        public void NineDefinitions_HaveExpectedData()
        {
            Assert.AreEqual(9, EnemyAmmoCatalog.All.Count);
            foreach (var e in Expected)
            {
                var d = EnemyAmmoCatalog.GetAmmoDefinition(e.id);
                Assert.AreEqual(e.minLevel, d.MinLevel, e.id);
                Assert.AreEqual(e.surcharge, d.AmmoSurcharge, e.id);
                Assert.AreEqual(e.weight, d.ProceduralWeight, 1e-5f, e.id);
                Assert.AreEqual(ComponentIds.EnemyCasing, d.OrderedComponentIds[3], e.id);
                // Src: Basic has only the four base ids; the other eight add one extra at [4].
                Assert.AreEqual(e.extra == null ? 4 : 5, d.OrderedComponentIds.Count, e.id);
                if (e.extra != null) Assert.AreEqual(e.extra, d.OrderedComponentIds[4], e.id);
            }
        }

        [Test]
        public void SpawnOptions_ByLevel()
        {
            string[] Ids(int level) => EnemyAmmoCatalog.GetProceduralSpawnOptions(level).Select(o => o.AmmoDefinition.Id).ToArray();
            CollectionAssert.AreEqual(new[] { "Basic" }, Ids(1));
            CollectionAssert.AreEqual(new[] { "Basic", "Heavy", "Scatter" }, Ids(3));
            CollectionAssert.AreEqual(new[] { "Basic", "Heavy", "Scatter", "Frangible", "Burst", "Piercing", "RapidFire", "Homing", "Matryoshka" }, Ids(8));
            Assert.AreEqual(1, Ids(0).Length);
        }

        [Test]
        public void Weight_ScalesWithLevel()
        {
            var option = EnemyAmmoCatalog.GetProceduralSpawnOptions(1)[0];
            Assert.AreEqual(1.6f * 1.1f, EnemyAmmoCatalog.GetProceduralWeight(option, 2), 1e-5f);
        }

        [Test]
        public void Weight_HeavyAtLevel3()
        {
            var heavy = EnemyAmmoCatalog.GetProceduralSpawnOptions(3)[1];
            Assert.AreEqual(EnemyAmmoIds.Heavy, heavy.AmmoDefinition.Id);
            Assert.AreEqual(0.8f * 1.15f, EnemyAmmoCatalog.GetProceduralWeight(heavy, 3), 1e-5f);
        }

        [Test]
        public void Cost_IsBasePlusSurcharge()
        {
            Assert.AreEqual(10, EnemyAmmoCatalog.GetCost(10, EnemyType.Rifle));
            Assert.AreEqual(14, EnemyAmmoCatalog.GetCost(10, EnemyType.Rifle, EnemyAmmoIds.Matryoshka));
            Assert.Throws<ArgumentOutOfRangeException>(() => EnemyAmmoCatalog.GetCost(10, EnemyType.Dummy));
        }

        [Test]
        public void UnknownId_Throws_BlankIsBasic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EnemyAmmoCatalog.GetAmmoDefinition("Nope"));
            Assert.AreEqual(EnemyAmmoIds.Basic, EnemyAmmoCatalog.GetAmmoDefinition("").Id);
            Assert.AreEqual(EnemyAmmoIds.Basic, EnemyAmmoCatalog.GetAmmoDefinition(null).Id);
        }
    }
}
