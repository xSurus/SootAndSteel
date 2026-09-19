using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets
{
    // ponytail: minimal stand-in for Items/Bullets/BulletItem.cs's ordered-component-id
    // shape. The full combine/upgrade-badge crafting logic (Workbench UI, Wave B) isn't
    // ported here — add it alongside that UI work if/when it's needed.
    public sealed class BulletRecipe
    {
        public EComponentType Type { get; }
        public IReadOnlyList<string> ComponentIds { get; }

        public BulletRecipe(EComponentType type, IEnumerable<string> componentIds)
        {
            if (componentIds == null)
            {
                throw new ArgumentNullException(nameof(componentIds));
            }

            string[] ids = componentIds.ToArray();
            if (ids.Length == 0)
            {
                throw new ArgumentException("A bullet recipe needs at least one component id.", nameof(componentIds));
            }

            Type = type;
            ComponentIds = ids;
        }
    }
}
