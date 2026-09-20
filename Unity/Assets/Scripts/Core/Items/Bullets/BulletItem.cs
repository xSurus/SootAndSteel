using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Items.Bullets
{
    // Port of Src/Items/Bullets/BulletItem.cs without draw code, colour and effect objects
    // (BulletStats.Color and BulletItem colour are draw-only and stay deferred).
    // Deviation: an empty component list throws here, where Src returns an empty item.
    public class BulletItem : Item
    {
        private readonly List<string> componentIds = new List<string>();

        public EComponentType Type { get; }
        public IReadOnlyList<string> ComponentIds => componentIds;
        public bool HasBasic => componentIds.Any(ComponentTraits.IsBasic);
        public bool HasUpgrade => componentIds.Any(id => !ComponentTraits.IsBasic(id));

        public BulletItem(string componentId) : base("Bullet")
        {
            Type = ComponentTraits.TypeOf(componentId);
            componentIds.Add(componentId);
        }

        public BulletItem(params BulletItem[] components) : base("Bullet")
        {
            if (components == null || components.Length == 0)
            {
                throw new Exception("Cannot build a bullet item from no components");
            }

            if (components.Length == 1)
            {
                Type = components[0].Type;
                componentIds.AddRange(components[0].componentIds);
                return;
            }

            List<EComponentType> types = components.Select(c => c.Type).Distinct().ToList();
            if (types.Count == 1)
            {
                Type = components[0].Type;
            }
            else if (types.Count == 3 && types.All(t => t != EComponentType.Bullet))
            {
                Type = EComponentType.Bullet;
            }
            else
            {
                throw new Exception("Invalid combination of components for combining bullet item");
            }

            foreach (BulletItem c in components) componentIds.AddRange(c.componentIds);

            // Basic components first, otherwise keeping order (Src EnsureBasicIsFirst).
            List<string> ordered = componentIds.Where(ComponentTraits.IsBasic)
                .Concat(componentIds.Where(id => !ComponentTraits.IsBasic(id))).ToList();
            componentIds.Clear();
            componentIds.AddRange(ordered);
        }

        public BulletRecipe ToRecipe() => new BulletRecipe(Type, componentIds);
    }
}
