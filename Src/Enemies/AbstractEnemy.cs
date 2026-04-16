using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Enemies;

public abstract class AbstractEnemy : AbstractPhysicalEntity, IDamageable, IBulletEmitter
{
    public EnemyDefinition Definition { get; }
    public EnemyType EnemyType => Definition.Type;
    public EnemyTrainSlot Slot { get; }
    public float Health { get; protected set; }
    public bool IsAlive => Health > 0;
    public bool ShouldRemove { get; protected set; }
    public abstract Color EnemyColor { get; }

    protected float Size => GamelabGame.Instance.GameplayConfig.EnemySize;
    protected readonly GameplayContext gameplayContext;
    protected readonly EnemyMovementController EnemyMovement;

    protected AbstractEnemy(
        GameplayContext gameplayContext,
        EnemyDefinition definition,
        Vector2 spawnPosition,
        EnemyTrainSlot slot,
        EnemyMovementProfile movementProfile)
    {
        Health = GamelabGame.Instance.GameplayConfig.EnemyHealth;
        Definition = definition;
        Slot = slot;
        this.gameplayContext = gameplayContext;
        PhysicsBody = gameplayContext.PhysicsWorld.CreateCircle((Size / 2f).ToMeters(), 1f, spawnPosition.ToMeters(),
            BodyType.Dynamic);
        PhysicsBody.IgnoreGravity = true;
        PhysicsBody.FixedRotation = true;

        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }

        EnemyMovement = new EnemyMovementController(PhysicsBody, gameplayContext, movementProfile);
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
        if (bullet.Owner.GetType() == typeof(CannonStation) && IsAlive && !ShouldRemove)
        {
            TakeDamage(bullet.Stats.Damage);
            return true;
        }
        return false;
    }

    public virtual void TryShoot()
    {
        return;
    }

    public virtual IEnemyHazard TryCreateHazard()
    {
        return null;
    }

    public void RemovePhysicsBody()
    {
        if (PhysicsBody == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || ShouldRemove) return;

        Texture2D texture = AssetManager.EnemyTexture;
        Rectangle destRect = new Rectangle(
            (int)(Position.X - Size / 2),
            (int)(Position.Y - Size / 2),
            (int)Size,
            (int)Size
        );

        spriteBatch.Draw(texture, destRect, EnemyColor);
    }

    protected bool HasReached(Vector2 targetPosition, float radius)
    {
        return Vector2.DistanceSquared(Position, targetPosition) <= radius * radius;
    }

    protected bool IsOffScreenLeft()
    {
        return Position.X < -Size;
    }

    protected bool IsOffScreenRight()
    {
        return Position.X > gameplayContext.ScreenWidth + Size;
    }

    protected bool IsOffScreenTop()
    {
        return Position.Y < -Size;
    }

    protected bool IsOffScreenBottom()
    {
        return Position.Y > gameplayContext.ScreenHeight + Size;
    }
}
