using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets.Components.Propellants;

public class HeavyPropellant : AbstractComponent
{
    public HeavyPropellant()
    {
        Type = EComponentType.Propellant;
        ComponentId = ComponentIds.HeavyPropellant;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Damage *= 2.0f;
        bulletEntity.Stats.Speed *= 0.3f;
        bulletEntity.Stats.Size *= 2f;
    }
}