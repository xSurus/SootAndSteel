using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class BasicProjectile : AbstractComponent
{
    public BasicProjectile()
    {
        Type = EComponentType.Projectile;
        IsBasic = true;
        ComponentId = ComponentIds.BasicProjectile;
    }
}