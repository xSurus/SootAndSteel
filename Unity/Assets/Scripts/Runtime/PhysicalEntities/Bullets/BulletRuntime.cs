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

        public bool IsPending { get; private set; }

        private float pendingDelay;
        private readonly Dictionary<BulletComponentAsset, object> states = new Dictionary<BulletComponentAsset, object>();
        private readonly HashSet<BulletComponentAsset> nonRoot = new HashSet<BulletComponentAsset>();

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
            return Build(definition, position, aimDirection, faction, 0f, null, null);
        }

        /// <summary>Per-bullet state for a stateless component asset, created on first use.</summary>
        public T GetState<T>(BulletComponentAsset component) where T : class, new()
        {
            if (!states.TryGetValue(component, out object state))
            {
                state = new T();
                states[component] = state;
            }

            return (T)state;
        }

        public bool IsRoot(BulletComponentAsset c) => !nonRoot.Contains(c);

        public void MarkNonRoot(BulletComponentAsset c) => nonRoot.Add(c);

        /// <summary>
        /// Fresh bullet from the same definition and faction. The spawner is non-root in the child,
        /// so it cannot recurse. configure runs before the create phase. delay > 0 keeps the child
        /// unsimulated (create phase only) until Tick has counted the delay down.
        /// </summary>
        public BulletRuntime SpawnChild(
            BulletComponentAsset spawner, Vector2 position, Vector2 aim, float delay = 0f,
            Action<BulletRuntime> configure = null)
        {
            return Build(definition, position, aim, Faction, delay, spawner, configure);
        }

        private static BulletRuntime Build(
            BulletDefinitionAsset definition, Vector2 position, Vector2 aimDirection, BulletFaction faction,
            float delay, BulletComponentAsset nonRootSpawner, Action<BulletRuntime> configure)
        {
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
            if (nonRootSpawner != null) bullet.MarkNonRoot(nonRootSpawner);
            configure?.Invoke(bullet);

            foreach (BulletComponentAsset c in definition.Components) c.OnCreate(bullet);
            bullet.RefreshCollider();

            if (delay > 0f)
            {
                bullet.IsPending = true;
                bullet.pendingDelay = delay;
                rb.simulated = false; // also deactivates the collider
            }
            else
            {
                bullet.RunSpawnPhase();
            }

            return bullet;
        }

        private void RunSpawnPhase()
        {
            foreach (BulletComponentAsset c in definition.Components)
            {
                if (!IsActive) break;
                c.OnSpawn(this);
            }
            RefreshCollider();
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
            var target = otherBody.GetComponentInParent<IDamageable>();
            if (target == null || hitCooldown.ContainsKey(target) || !target.OnHit(this)) return;

            foreach (BulletComponentAsset c in definition.Components) c.OnHit(this, target);
            pierceCount++;
            hitCooldown[target] = HitCooldownSeconds;

            if (Stats.Pierce <= pierceCount)
            {
                Deactivate();
            }
        }


        public void Tick(float deltaTime)
        {
            if (definition == null) return;
            if (!IsActive)
            {
                return;
            }

            if (IsPending)
            {
                pendingDelay -= deltaTime;
                if (pendingDelay <= 0f)
                {
                    IsPending = false;
                    PhysicsBody.simulated = true; // before the spawn phase so the velocity lands on a simulated body
                    RunSpawnPhase();
                }

                return; // age, lifetime and OnUpdate start after the spawn phase
            }

            foreach (BulletComponentAsset c in definition.Components) c.OnUpdate(this, deltaTime);

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
            // OnCleanup also runs for pending bullets whose OnSpawn never ran.
            foreach (BulletComponentAsset c in definition.Components) c.OnCleanup(this);
            Destroy(gameObject);
        }
    }
}
