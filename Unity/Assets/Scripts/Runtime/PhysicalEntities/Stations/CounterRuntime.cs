// Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/CounterRuntime.cs
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Vertical-slice concrete station: Src/PhysicalEntities/Stations/Counter.cs is the
    // simplest concrete station (one override, no sound/particle/shop coupling unlike
    // Workbench/CannonStation), so it's the one this agent ports behaviorally.
    public class CounterRuntime : StationRuntime
    {
        public override bool CanReceiveItem(Item item, IItemProvider source)
        {
            return HeldItem == null;
        }
    }
}
