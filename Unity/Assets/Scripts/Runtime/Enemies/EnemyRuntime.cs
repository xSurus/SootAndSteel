using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities;
using UnityEngine;

namespace Gamelab.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyRuntime : MonoBehaviour, IDamageable, IBulletEmitter
    {
        public float Health { get; protected set; }
        public bool IsAlive => Health > 0;
        public bool ShouldRemove { get; protected set; }
        public Rigidbody2D PhysicsBody { get; private set; }

        public virtual void Initialize(EnemyCatalogEntry catalogEntry)
        {
            Health = catalogEntry.health;
            PhysicsBody = GetComponent<Rigidbody2D>();
            PhysicsBody.gravityScale = 0f;
            PhysicsBody.freezeRotation = true;

            // Src enemy fixtures are sensors: trigger circle so bullets detect it without pushing it.
            var entity = GetComponent<PhysicalEntity>() ?? gameObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(WorldUnits.ToMeters(catalogEntry.size / 2f), 1f, 0f, true)
                .isTrigger = true;
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                ShouldRemove = true;
            }
        }

        // ponytail: Src/Enemies/Core/AbstractEnemy.cs's OnHit also checks
        // "bullet.InitialShooter is CannonStation or CannonSlot" — the station-specific
        // shooter check is dropped here since it needs the concrete station types
        // (Task 10/13). Any bullet with Faction == Player currently damages any enemy.
        public virtual bool OnHit(BulletRuntime bullet)
        {
            if (bullet.Faction != BulletFaction.Player || !IsAlive || ShouldRemove)
            {
                return false;
            }

            TakeDamage(bullet.Stats.Damage);
            return true;
        }
    }
}
