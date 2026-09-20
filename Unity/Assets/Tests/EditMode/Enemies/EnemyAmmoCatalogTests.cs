using System;
using System.Linq;
using NUnit.Framework;
using Gamelab.Enemies.Core;
using Gamelab.Items.Bullets;

namespace Gamelab.Tests.Enemies
{
    public class EnemyAmmoCatalogTests
    {
        private static readonly (string id, int minLevel, int surcharge, float weight)[] Expected =
        {
            (EnemyAmmoIds.Basic, 1, 0, 1.6f), (EnemyAmmoIds.Heavy, 3, 1, 0.8f),
            (EnemyAmmoIds.Scatter, 3, 1, 0.8f), (EnemyAmmoIds.Frangible, 4, 1, 0.7f),
            (EnemyAmmoIds.Burst, 5, 2, 0.55f), (EnemyAmmoIds.Piercing, 5, 2, 0.5f),
            (EnemyAmmoIds.RapidFire, 6, 3, 0.35f), (EnemyAmmoIds.Homing, 7, 3, 0.3f),
            (EnemyAmmoIds.Matryoshka, 8, 4, 0.2f)
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
            }
        }

        [Test]
        public void SpawnOptions_ByLevel()
        {
            string[] Ids(int level) => EnemyAmmoCatalog.GetProceduralSpawnOptions(level).Select(o => o.AmmoDefinition.Id).ToArray();
            CollectionAssert.AreEqual(new[] { "Basic" }, Ids(1));
            CollectionAssert.AreEqual(new[] { "Basic", "Heavy", "Scatter" }, Ids(3));
            Assert.AreEqual(9, Ids(8).Length);
            Assert.AreEqual(1, Ids(0).Length);
        }

        [Test]
        public void Weight_ScalesWithLevel()
        {
            var option = EnemyAmmoCatalog.GetProceduralSpawnOptions(1)[0];
            Assert.AreEqual(1.6f * 1.1f, EnemyAmmoCatalog.GetProceduralWeight(option, 2), 1e-5f);
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
