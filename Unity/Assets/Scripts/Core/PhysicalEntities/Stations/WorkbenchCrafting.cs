using System.Collections.Generic;
using System.Linq;
using Gamelab.Items;
using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of the crafting rules in Src/PhysicalEntities/Stations/Workbench.cs.
    // Dropped presentation: sparks, craft sound, table light and the highlight-driven isCrafting.
    public class WorkbenchCrafting
    {
        public const float CraftSeconds = 2f;
        public const int MaxItems = 3;

        private readonly List<BulletItem> placedItems = new List<BulletItem>();

        public IReadOnlyList<BulletItem> PlacedItems => placedItems;
        public float CraftProgress { get; private set; }
        public bool IsCrafting { get; private set; }

        public bool CanPlace(Item item)
        {
            if (CraftProgress > 0f) return false;
            if (item == null) return false;
            if (placedItems.Count >= MaxItems) return false;
            if (item.Id != "Bullet") return false;
            BulletItem bullet = (BulletItem)item;

            // A component id may not be on the table already.
            if (placedItems.Any(p => p.ComponentIds.Any(id => bullet.ComponentIds.Contains(id)))) return false;

            List<EComponentType> types = ContainedTypes();

            // Upgrade: same type, at most two items.
            if (types.Count == 0 || (types.Count == 1 && placedItems.Count < 2 && types.Contains(bullet.Type)))
                return true;

            // Final bullet: a new type, everything on the table (and the new item) has a basic part.
            return !types.Contains(bullet.Type) && placedItems.All(x => x.HasBasic) && bullet.HasBasic;
        }

        public void Place(BulletItem item) => placedItems.Add(item);

        public bool CanTake => CraftProgress == 0f && placedItems.Count > 0;

        public BulletItem Peek() => CraftProgress > 0f ? null : placedItems.LastOrDefault();

        public BulletItem Take()
        {
            BulletItem item = placedItems[placedItems.Count - 1];
            placedItems.RemoveAt(placedItems.Count - 1);
            return item;
        }

        public bool CanCraft()
        {
            List<EComponentType> types = ContainedTypes();
            if (placedItems.Count == 2 && types.Count == 1 && types[0] != EComponentType.Bullet) return true;
            return placedItems.Count == 3 && types.Count == 3 && types.All(t => t != EComponentType.Bullet)
                   && placedItems.All(x => x.HasBasic);
        }

        // Returns true when a craft completed this call.
        public bool InteractHeld(float dt)
        {
            if (CraftProgress > 0f || CanCraft())
            {
                CraftProgress += dt;
                IsCrafting = true;
            }

            if (CraftProgress < CraftSeconds) return false;

            BulletItem crafted = new BulletItem(placedItems.ToArray());
            placedItems.Clear();
            placedItems.Add(crafted);
            CraftProgress = 0f;
            IsCrafting = false;
            return true;
        }

        // Src keeps craftProgress on release, so the table stays locked until finished.
        public void InteractReleased() => IsCrafting = false;

        private List<EComponentType> ContainedTypes() => placedItems.Select(x => x.Type).Distinct().ToList();
    }
}
