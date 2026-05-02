using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Conveyors;

public class BulletConveyor(Vector2 position, GridDirection facingDirection)
    : Conveyor(position, facingDirection, StationIds.BulletConveyor)
{
    protected override bool AcceptsItem(Item item)
    {
        if (item is not BulletItem { Type: EComponentType.Bullet })
        {
            return false;
        }

        return base.AcceptsItem(item);
    }
}