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
        bulletEntity.Stats.Damage *= 2f;
        bulletEntity.Stats.Speed *= 0.7f;
        bulletEntity.Stats.Size *= 1.5f;
    }
}