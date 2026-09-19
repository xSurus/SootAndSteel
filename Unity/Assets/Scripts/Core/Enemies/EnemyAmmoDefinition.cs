using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Enemies.Core
{
    public sealed class EnemyAmmoDefinition
    {
        public string Id { get; }
        public IReadOnlyList<string> OrderedComponentIds { get; }
        public int AmmoSurcharge { get; }
        public int MinLevel { get; }
        public float ProceduralWeight { get; }

        public EnemyAmmoDefinition(
            string id,
            IEnumerable<string> orderedComponentIds,
            int ammoSurcharge,
            int minLevel,
            float proceduralWeight)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Enemy ammo id must not be blank.", nameof(id));
            }

            if (orderedComponentIds == null)
            {
                throw new ArgumentNullException(nameof(orderedComponentIds));
            }

            string[] normalizedComponentIds = orderedComponentIds.ToArray();
            if (normalizedComponentIds.Length == 0)
            {
                throw new ArgumentException("Enemy ammo recipes must contain at least one component.",
                    nameof(orderedComponentIds));
            }

            if (!normalizedComponentIds.Contains(ComponentIds.EnemyCasing))
            {
                throw new ArgumentException("Enemy ammo recipes must include EnemyCasing.",
                    nameof(orderedComponentIds));
            }

            Id = id;
            OrderedComponentIds = normalizedComponentIds;
            AmmoSurcharge = Math.Max(0, ammoSurcharge);
            MinLevel = Math.Max(1, minLevel);
            ProceduralWeight = Math.Max(0f, proceduralWeight);
        }

        public bool IsUnlockedAtLevel(int levelNumber)
        {
            return Math.Max(1, levelNumber) >= MinLevel;
        }

        public BulletRecipe BuildRecipe()
        {
            return new BulletRecipe(EComponentType.Bullet, OrderedComponentIds);
        }
    }
}
