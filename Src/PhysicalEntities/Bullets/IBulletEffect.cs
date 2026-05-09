using System;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Bullets;

public interface IBulletEffect
{
    Guid Guid { get; }
    public EComponentType Type { get; }
    public string ComponentId { get; }
    bool IsBasic { get; }
    bool IsRootEffect { get; set; }
    
    void Copy(IBulletEffect effect);

    void OnCreate(BulletEntity bulletEntity);
    void OnSpawn(BulletEntity bulletEntity);
    void OnUpdate(BulletEntity bulletEntity, float deltaTime);
    void OnDraw(BulletEntity bulletEntity, SpriteBatch spriteBatch);
    void OnHit(BulletEntity bulletEntity, IDamageable hitEntity);
    void OnCleanup(BulletEntity bulletEntity);
}