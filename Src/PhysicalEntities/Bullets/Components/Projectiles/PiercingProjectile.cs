using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class PiercingProjectile : AbstractComponent
{
    public PiercingProjectile()
    {
        Type = EComponentType.Projectile;
        ComponentId = ComponentIds.PiercingProjectile;
    }

    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Pierce += 3;
        bulletEntity.Stats.Speed *= 1.5f;
    }
}