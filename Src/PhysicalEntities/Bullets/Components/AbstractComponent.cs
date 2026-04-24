using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Bullets.Components;

public abstract class AbstractComponent : IBulletEffect
{
    public EComponentType Type { get; protected set; }
    public string ComponentId { get; protected set; }
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