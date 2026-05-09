using System;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Bullets.Components;

public abstract class AbstractComponent : IBulletEffect
{
    public Guid Guid { get; protected set; } = Guid.NewGuid();
    public EComponentType Type { get; protected set; }
    public string ComponentId { get; protected set; }
    public bool IsBasic { get; protected set; } = false;
    public bool IsRootEffect { get; set; } = true;

    public void Copy(IBulletEffect effect)
    {
        Guid = effect.Guid;
        Type = effect.Type;
        ComponentId = effect.ComponentId;
        IsBasic = effect.IsBasic;
        IsRootEffect = effect.IsRootEffect;
    }

    public virtual void OnCreate(BulletEntity bulletEntity)
    {
    }

    public virtual void OnSpawn(BulletEntity bulletEntity)
    {
    }

    public virtual void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
    }

    public virtual void OnDraw(BulletEntity bulletEntity, SpriteBatch spriteBatch)
    {
    }

    public virtual void OnHit(BulletEntity bulletEntity, IDamageable hitEntity)
    {
    }

    public virtual void OnCleanup(BulletEntity bulletEntity)
    {
    }
}