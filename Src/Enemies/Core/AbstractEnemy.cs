using System;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.Particles;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Enemies.Core;

public abstract class AbstractEnemy : AbstractPhysicalEntity, IDamageable, IBulletEmitter, IDisposable
{
    public EnemyTrainSlot Slot { get; }
    public float Health { get; protected set; }
    public bool IsAlive => Health > 0;
    public bool ShouldRemove { get; protected set; }

    protected float Size => GamelabGame.Instance.GameplayConfig.EnemySize;
    protected virtual float StartingHealth => GamelabGame.Instance.GameplayConfig.EnemyHealth;
    public virtual Vector2 FeetPosition => new(Position.X, Position.Y + Size / 2f);
    protected readonly EnemyMovementController enemyMovement;
    protected readonly ISoundService soundService;

    protected AbstractEnemy(
        Vector2 spawnPosition,
        EnemyTrainSlot slot,
        EnemyMovementProfile movementProfile)
    {
        Health = StartingHealth;
        Slot = slot;
        PhysicsBody = gameplayContext.PhysicsWorld.CreateCircle((Size / 2f).ToMeters(), 1f, spawnPosition.ToMeters(),
            BodyType.Dynamic);
        PhysicsBody.IgnoreGravity = true;
        PhysicsBody.FixedRotation = true;

        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }

        enemyMovement = new EnemyMovementController(PhysicsBody, movementProfile);
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.EnemyHit);
    }

    public virtual void Update(float deltaTime)
    {
        if (IsOffScreenLeft())
        {
            ShouldRemove = true;
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        var vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();
        vfxService.EmitBurst(ParticleFactory.CreateBloodSplatter(Position));
        if (Health <= 0)
        {
            ShouldRemove = true;
        }
    }

    public virtual bool OnHit(BulletEntity bullet)
    {
        if ((bullet.InitialShooter is CannonStation or CannonSlot) && IsAlive && !ShouldRemove)
        {
            soundService.PlayOnce(Sounds.EnemyHit);
            TakeDamage(bullet.Stats.Damage);
            var vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();
            vfxService.EmitBurst(ParticleFactory.CreateBloodHit(Position));
            return true;
        }

        return false;
    }

    public virtual void TryShoot()
    {
    }

    private void RemovePhysicsBody()
    {
        if (PhysicsBody == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public abstract override void Draw(SpriteBatch spriteBatch);

    protected bool HasReached(Vector2 targetPosition, float radius)
    {
        return Vector2.DistanceSquared(Position, targetPosition) <= radius * radius;
    }

    protected bool IsOffScreenLeft()
    {
        return Position.X < -Size;
    }

    public virtual void Dispose()
    {
        RemovePhysicsBody();
    }
}
