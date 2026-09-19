using System;
using NUnit.Framework;
using Gamelab.Enemies.Core;
using Gamelab.Items.Bullets;

namespace Gamelab.Tests.Enemies
{
    public class EnemyAmmoDefinitionTests
    {
        [Test]
        public void Constructor_WithoutEnemyCasing_Throws()
        {
            Assert.Throws<ArgumentException>(() => new EnemyAmmoDefinition(
                "Basic", new[] { ComponentIds.BasicProjectile }, 0, 1, 1f));
        }

        [Test]
        public void IsUnlockedAtLevel_BelowMinLevel_ReturnsFalse()
        {
            var ammo = new EnemyAmmoDefinition(
                "Heavy",
                new[] { ComponentIds.BasicProjectile, ComponentIds.EnemyCasing, ComponentIds.HeavyPropellant },
                ammoSurcharge: 1, minLevel: 3, proceduralWeight: 0.8f);

            Assert.IsFalse(ammo.IsUnlockedAtLevel(2));
            Assert.IsTrue(ammo.IsUnlockedAtLevel(3));
        }

        [Test]
        public void BuildRecipe_ReturnsRecipeWithSameComponentIds()
        {
            var ammo = new EnemyAmmoDefinition(
                "Basic",
                new[] { ComponentIds.BasicProjectile, ComponentIds.EnemyCasing },
                0, 1, 1f);

            var recipe = ammo.BuildRecipe();

            CollectionAssert.AreEqual(ammo.OrderedComponentIds, recipe.ComponentIds);
        }
    }
}
