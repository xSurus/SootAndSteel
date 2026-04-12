using Gamelab.Entities;

namespace Gamelab.PhysicalEntities.Bullets.Components;

public class AbstractComponent : IBulletEffect
{
    public EComponentType Type;
    public bool IsBasic { get; protected set; } = false;

    public virtual void OnCreate(BulletEntity bulletEntity)
    {
    }
    
    public virtual void OnSpawn(BulletEntity bulletEntity)
    {
    }

    public virtual void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
    }

    public virtual void OnHit(BulletEntity bulletEntity, IDamageable hitEntity)
    {
    }

    public virtual void OnCleanup(BulletEntity bulletEntity)
    {
    }
}