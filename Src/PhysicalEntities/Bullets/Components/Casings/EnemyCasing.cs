using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets.Components.Casings;

public class EnemyCasing : AbstractComponent
{
    public EnemyCasing()
    {
        Type = EComponentType.Casing;
        ComponentId = ComponentIds.EnemyCasing;
    }
    
    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Damage = 10f;
    }
}