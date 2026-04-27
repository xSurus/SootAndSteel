using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class PiercingCasing : AbstractComponent
{
    public PiercingCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = ComponentIds.PiercingCasing;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Pierce += 3;
        bulletEntity.Stats.Speed *= 1.5f;
    }
}