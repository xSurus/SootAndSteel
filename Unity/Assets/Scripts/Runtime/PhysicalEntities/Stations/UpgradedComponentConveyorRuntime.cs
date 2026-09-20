using Gamelab.Items;
using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src UpgradedComponentConveyor: components that are basic-plus-upgrade
    // (Src: HasBasic and not all effects basic, i.e. HasUpgrade).
    public class UpgradedComponentConveyorRuntime : ConveyorRuntime
    {
        protected override string DefaultStationId => StationIds.UpgradedComponentConveyor;

        protected override bool AcceptsItem(Item item) =>
            item is BulletItem c && c.Type != EComponentType.Bullet && c.HasBasic && c.HasUpgrade &&
            base.AcceptsItem(item);
    }
}
