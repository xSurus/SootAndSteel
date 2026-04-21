using Gamelab.Assets;
using Gamelab.Particles;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Interfaces;
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
    protected readonly EnemyMovementController EnemyMovement;

    protected AbstractEnemy(
        EnemyDefinition definition,
        Vector2 spawnPosition,
        EnemyTrainSlot slot,
        EnemyMovementProfile movementProfile)
    {
        Health = GamelabGame.Instance.GameplayConfig.EnemyHealth;
        Definition = definition;
        Slot = slot;
        PhysicsBody = gameplayContext.PhysicsWorld.CreateCircle((Size / 2f).ToMeters(), 1f, spawnPosition.ToMeters(),
            BodyType.Dynamic);
        PhysicsBody.IgnoreGravity = true;
        PhysicsBody.FixedRotation = true;

        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }

        EnemyMovement = new EnemyMovementController(PhysicsBody, movementProfile);
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
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height);
        Vector2 feetPosition = Position + new Vector2(0, Size / 2f);
        float depth = RenderUtility.CalculateDepth(feetPosition.Y);
        float scale = Size / texture.Width;

        spriteBatch.Draw(
            texture: texture,
            position: feetPosition,
            sourceRectangle: null,
            color: EnemyColor,
            rotation: 0f,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: depth
        );
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