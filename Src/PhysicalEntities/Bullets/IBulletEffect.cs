
namespace Gamelab.PhysicalEntities.Bullets;

public interface IBulletEffect
{
    bool IsBasic { get; }
    
    void OnCreate(BulletEntity bulletEntity);
    void OnSpawn(BulletEntity bulletEntity);
    void OnUpdate(BulletEntity bulletEntity, float deltaTime);
    void OnHit(BulletEntity bulletEntity, IDamageable hitEntity);
    void OnCleanup(BulletEntity bulletEntity);
}