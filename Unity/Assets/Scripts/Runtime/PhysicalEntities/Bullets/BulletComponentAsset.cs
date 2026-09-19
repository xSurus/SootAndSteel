using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    public abstract class BulletComponentAsset : ScriptableObject
    {
        public abstract EComponentType Type { get; }
        public abstract string ComponentId { get; }

        public virtual void OnCreate(BulletRuntime bullet)
        {
        }

        public virtual void OnSpawn(BulletRuntime bullet)
        {
        }

        public virtual void OnUpdate(BulletRuntime bullet, float deltaTime)
        {
        }

        public virtual void OnHit(BulletRuntime bullet, IDamageable hitEntity)
        {
        }

        public virtual void OnCleanup(BulletRuntime bullet)
        {
        }
    }
}
