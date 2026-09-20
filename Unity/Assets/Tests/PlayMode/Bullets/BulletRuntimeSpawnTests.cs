using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using Gamelab.PhysicalEntities.Bullets.Projectiles;

namespace Gamelab.Tests.Bullets
{
    public class BulletRuntimeSpawnTests
    {
        [UnityTest]
        public IEnumerator Spawn_FromCatalogDefinition_AppliesStatsAndVelocity()
        {
            var definition = BulletTestUtil.MakeBasicDefinition();
            typeof(BulletDefinitionAsset).GetField("spread",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(definition, 0f); // zero spread => deterministic velocity direction

            BulletRuntime bullet = BulletRuntime.Spawn(
                definition, Vector2.zero, Vector2.right, BulletFaction.Player);

            yield return null; // let physics settle one frame

            Assert.AreEqual(800f, bullet.Stats.Speed);
            Assert.AreEqual(BulletFaction.Player, bullet.Faction);
            Assert.Greater(bullet.PhysicsBody.linearVelocity.x, 0f);
            Assert.AreEqual(0f, bullet.PhysicsBody.linearVelocity.y, 0.01f);

            Object.Destroy(bullet.gameObject);
        }
    }
}
