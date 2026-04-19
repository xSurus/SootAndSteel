using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class BasicCasing : AbstractComponent
{
    public BasicCasing()
    {
        Type = EComponentType.Casing;
        IsBasic = true;
        ComponentId = ComponentIds.BasicCasing;
    }
}