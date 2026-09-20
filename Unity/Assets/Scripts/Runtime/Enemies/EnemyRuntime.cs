using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities;
using Gamelab.Enemies.Core;
using UnityEngine;

namespace Gamelab.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyRuntime : MonoBehaviour, IDamageable, IBulletEmitter
    {
        public float Health { get; protected set; }
        public bool IsAlive => Health > 0;
        // Set on death, or by Tick when Bounds is set and the enemy is past the left edge
        // (Src AbstractEnemy.IsOffScreenLeft). Enemy.cs's flee cull is EnemyCulling.IsFleeCulled.
        public bool ShouldRemove { get; protected set; }
        public Rigidbody2D PhysicsBody { get; private set; }
        // Null means no cull. Wave B supplies the map/camera bounds.
        public IWorldBounds Bounds { get; set; }
        protected virtual float SizePixels => size;
        private float size;

        private void Update() => Tick(Time.deltaTime);

        protected virtual void Tick(float dt)
        {
            if (Bounds != null &&
                EnemyCulling.IsOffScreenLeft(WorldUnits.ToPixels(transform.position.x), SizePixels, Bounds))
            {
                ShouldRemove = true;
            }
        }

        protected virtual void OnEnable() => BulletTargeting.Register(this);
        protected virtual void OnDisable() => BulletTargeting.Unregister(this);

        public virtual void Initialize(EnemyCatalogEntry catalogEntry)
        {
            Health = catalogEntry.health;
            size = catalogEntry.size;
            PhysicsBody = GetComponent<Rigidbody2D>();
            PhysicsBody.gravityScale = 0f;
            PhysicsBody.freezeRotation = true;

            // Src enemy fixtures are sensors: trigger circle so bullets detect it without pushing it.
            if (!TryGetComponent(out PhysicalEntity entity)) entity = gameObject.AddComponent<PhysicalEntity>();
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
