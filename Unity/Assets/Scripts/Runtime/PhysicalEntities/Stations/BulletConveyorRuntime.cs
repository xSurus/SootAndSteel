using Gamelab.Items;
using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src BulletConveyor: only finished bullets.
    public class BulletConveyorRuntime : ConveyorRuntime
    {
        protected override string DefaultStationId => StationIds.BulletConveyor;

        protected override bool AcceptsItem(Item item) =>
            item is BulletItem b && b.Type == EComponentType.Bullet && base.AcceptsItem(item);
    }
}
