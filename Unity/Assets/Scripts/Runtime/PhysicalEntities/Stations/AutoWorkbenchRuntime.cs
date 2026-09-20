namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src AutoWorkbench: interacts with itself every update.
    public class AutoWorkbenchRuntime : WorkbenchRuntime
    {
        protected override string DefaultStationId => StationIds.AutoWorkbench;

        public override void Update(float dt)
        {
            base.Update(dt);
            crafting.InteractHeld(dt);
        }
    }
}
