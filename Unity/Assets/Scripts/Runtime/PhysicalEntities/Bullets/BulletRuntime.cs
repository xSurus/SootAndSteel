using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BulletRuntime : MonoBehaviour
    {
        public BulletStats Stats { get; private set; }
        public BulletFaction Faction { get; private set; }
        public Vector2 AimDirection { get; private set; }
        public bool IsActive { get; private set; } = true;
        public Rigidbody2D PhysicsBody { get; private set; }

        private BulletDefinitionAsset definition;

        public static BulletRuntime Spawn(
            BulletDefinitionAsset definition,
            Vector2 position,
            Vector2 aimDirection,
            BulletFaction faction)
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
            bullet.Faction = faction;
            bullet.AimDirection = aimDirection.normalized;

            definition.Casing.OnCreate(bullet);
            definition.Propellant.OnCreate(bullet);
            definition.Projectile.OnCreate(bullet);

            definition.Casing.OnSpawn(bullet);
            definition.Propellant.OnSpawn(bullet);
            definition.Projectile.OnSpawn(bullet);

            return bullet;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            definition.Casing.OnUpdate(this, deltaTime);
            definition.Propellant.OnUpdate(this, deltaTime);
            definition.Projectile.OnUpdate(this, deltaTime);
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
