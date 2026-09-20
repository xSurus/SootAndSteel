using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src Workbench. Dropped presentation: sparks, craft sound, table light,
    // item drawing and the highlight-driven isCrafting (Src OnHighlightRemoved).
    // Re-declares IInteractable so the holds rebind the interface slot owned by StationRuntime.
    public class WorkbenchRuntime : StationRuntime, IInteractable
    {
        protected readonly WorkbenchCrafting crafting = new WorkbenchCrafting();

        public WorkbenchCrafting Crafting => crafting;

        public override void Initialize(StationCatalogEntry catalogEntry)
        {
            base.Initialize(catalogEntry);
            if (string.IsNullOrEmpty(StationId)) StationId = DefaultStationId;
        }

        protected virtual string DefaultStationId => StationIds.Workbench;

        public override bool CanReceiveItem(Item item, IItemProvider source) => crafting.CanPlace(item);

        public override void ReceiveItem(Item item, IItemProvider source) => crafting.Place((BulletItem)item);

        public override Item PeekNextItem() => crafting.Peek();

        public override bool CanProvideItem(IItemReceiver consumer) =>
            crafting.CanTake && IsConsumerFirstInLine(consumer);

        public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
        {
            item = null;
            if (!CanProvideItem(consumer)) return false;

            item = crafting.Take();
            if (consumer != null) consumerQueue.RemoveAll(t => t.Consumer == consumer);
            return true;
        }

        public void OnInteractHeld(IPlayerActor interactingPlayer, float dt) => crafting.InteractHeld(dt);

        public void OnInteractReleased(IPlayerActor interactingPlayer) => crafting.InteractReleased();
    }
}
