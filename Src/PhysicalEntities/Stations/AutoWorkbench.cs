using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class AutoWorkbench(Vector2 position) : Workbench(position, StationIds.AutoWorkbench)
{
    public override void Update(float dt)
    {
        base.Update(dt);
        //Interacts with the workbench itself
        OnInteractHeld(null, dt);
    }
}