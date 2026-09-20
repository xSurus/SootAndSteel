using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;

namespace Gamelab.Tests.Bullets
{
    public static class BulletTestUtil
    {
        public static BulletDefinitionAsset MakeBasicDefinition(
            float spread = 0f, float lifetime = 5f, float speed = 800f, float damage = 50f,
            params BulletComponentAsset[] extra)
        {
            var definition = ScriptableObject.CreateInstance<BulletDefinitionAsset>();
            var list = new List<BulletComponentAsset>
            {
                ScriptableObject.CreateInstance<BasicCasingAsset>(),
                ScriptableObject.CreateInstance<BasicPropellantAsset>(),
                ScriptableObject.CreateInstance<BasicProjectileAsset>()
            };
            list.AddRange(extra);
            SetPrivate(definition, "components", list);
            SetPrivate(definition, "spread", spread);
            SetPrivate(definition, "lifetime", lifetime);
            SetPrivate(definition, "speed", speed);
            SetPrivate(definition, "damage", damage);
            return definition;
        }

        public static void SetPrivate<T>(object target, string field, T value)
        {
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }
    }
}
