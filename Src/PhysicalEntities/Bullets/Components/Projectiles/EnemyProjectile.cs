namespace Gamelab.PhysicalEntities.Bullets.Components.Projectiles;

public class EnemyProjectile : AbstractComponent
{
    public override void OnCreate(BulletEntity bulletEntity)
    {
        bulletEntity.Stats.Damage = 10f;
    }
}