using System.Linq;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Conveyors;

public class UpgradedComponentConveyor(Vector2 position, GridDirection facingDirection)
    : Conveyor(position, facingDirection, StationIds.UpgradedComponentConveyor)
{
    protected override bool AcceptsItem(Item item)
    {
        if (item is not BulletItem component ||
            component.Type == EComponentType.Bullet ||
            !component.HasBasic ||
            component.GetEffects().All(e => e.IsBasic))
        {
            return false;
        }

        return base.AcceptsItem(item);
    }
}