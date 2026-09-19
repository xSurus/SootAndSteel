using System;
using NUnit.Framework;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    public class BulletRecipeTests
    {
        [Test]
        public void Constructor_StoresOrderedComponentIds()
        {
            var recipe = new BulletRecipe(EComponentType.Bullet,
                new[] { ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant });

            Assert.AreEqual(EComponentType.Bullet, recipe.Type);
            CollectionAssert.AreEqual(
                new[] { ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant },
                recipe.ComponentIds);
        }

        [Test]
        public void Constructor_WithNoComponents_Throws()
        {
            Assert.Throws<ArgumentException>(() => new BulletRecipe(EComponentType.Bullet, Array.Empty<string>()));
        }
    }
}
