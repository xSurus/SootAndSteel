using System;
using System.Collections.Generic;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BulletRuntime : MonoBehaviour
    {
        /// <summary>Public field so components can mutate stats in OnCreate like Src (bullet.Stats.Damage *= 0.5f).</summary>
        public BulletStats Stats;
        public BulletFaction Faction { get; private set; }
        public Vector2 AimDirection { get; private set; }
        public bool IsActive { get; private set; } = true;
        public Rigidbody2D PhysicsBody { get; private set; }
        public float Age { get; private set; }

        private BulletDefinitionAsset definition;
        private CircleCollider2D circle;
        private int pierceCount;
        private const float HitCooldownSeconds = 1.0f;
        private readonly Dictionary<IDamageable, float> hitCooldown = new Dictionary<IDamageable, float>();
        private readonly List<IDamageable> cooldownKeys = new List<IDamageable>();

        public static BulletRuntime Spawn(
            BulletDefinitionAsset definition,
            Vector2 position,
            Vector2 aimDirection,
            BulletFaction faction)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            definition.Validate();

            var go = new GameObject($"Bullet_{definition.name}");
            go.transform.position = position;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var bullet = go.AddComponent<BulletRuntime>();
            bullet.PhysicsBody = rb;
            bullet.definition = definition;
            bullet.Stats = definition.ToStats();
            var entity = go.AddComponent<PhysicalEntity>();
            bullet.circle = entity.ConfigureAsDynamicCircle(
                WorldUnits.ToMeters(bullet.Stats.Size / 2f), 1f, 0f, true);
            bullet.circle.isTrigger = true;
            bullet.Faction = faction;
            bullet.AimDirection = aimDirection.normalized;

            definition.Casing.OnCreate(bullet);
            definition.Propellant.OnCreate(bullet);
            definition.Projectile.OnCreate(bullet);
            bullet.RefreshCollider();

            definition.Casing.OnSpawn(bullet);
            definition.Propellant.OnSpawn(bullet);
            definition.Projectile.OnSpawn(bullet);
            bullet.RefreshCollider();

            return bullet;
        }

        private void FixedUpdate() => Tick(Time.fixedDeltaTime);

        /// <summary>Re-applies the trigger radius from the current Stats.Size (pixels).</summary>
        public void RefreshCollider() => circle.radius = WorldUnits.ToMeters(Stats.Size / 2f);

        public void AddHitCooldown(IDamageable hitEntity, float cooldown) => hitCooldown[hitEntity] = cooldown;

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
        private void OnTriggerStay2D(Collider2D other) => TryHit(other);

        // Mirrors Src BulletEntity.OnCollision.
        private void TryHit(Collider2D other)
        {
            if (!IsActive) return;
            Rigidbody2D otherBody = other.attachedRigidbody;
            if (otherBody == null || otherBody.gameObject == gameObject) return;
            var target = otherBody.GetComponent<IDamageable>() ?? otherBody.GetComponentInParent<IDamageable>();
            if (target == null || hitCooldown.ContainsKey(target) || !target.OnHit(this)) return;

            definition.Casing.OnHit(this, target);
            definition.Propellant.OnHit(this, target);
            definition.Projectile.OnHit(this, target);
            pierceCount++;
            hitCooldown[target] = HitCooldownSeconds;

            if (Stats.Pierce <= pierceCount)
            {
                Deactivate();
            }
        }


        // Src also removes bullets that leave the screen to the left. Not ported: it needs the
        // map/camera origin from wave B1 (plan ruling 6). Lifetime already bounds bullets.
        public void Tick(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            definition.Casing.OnUpdate(this, deltaTime);
            definition.Propellant.OnUpdate(this, deltaTime);
            definition.Projectile.OnUpdate(this, deltaTime);

            // Src rebuilds the dictionary each tick; a reused key list avoids the allocation.
            cooldownKeys.Clear();
            cooldownKeys.AddRange(hitCooldown.Keys);
            foreach (IDamageable key in cooldownKeys)
            {
                float left = hitCooldown[key] - deltaTime;
                if (left > 0f) hitCooldown[key] = left;
                else hitCooldown.Remove(key);
            }

            // Src checks lifetime in BasicPropellant.OnUpdate; the runtime owns it now (plan ruling 4).
            Age += deltaTime;
            if (Age > Stats.Lifetime)
            {
                Deactivate();
            }
        }

        public void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            definition.Casing.OnCleanup(this);
            definition.Propellant.OnCleanup(this);
            definition.Projectile.OnCleanup(this);
            Destroy(gameObject);
        }
    }
}
