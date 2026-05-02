using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class Counter(Vector2 position)
    : AbstractStation(StationIds.Counter, position)
{
    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        return HeldItem == null;
    }
}