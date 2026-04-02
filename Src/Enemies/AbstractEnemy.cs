using Gamelab.Assets;
using Gamelab.Entities;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Enemies;

public abstract class AbstractEnemy : AbstractPhysicalEntity, IDamageable
{
    public EnemyTrainSlot Slot { get; }
    public float Health { get; protected set; }
    public bool IsAlive => Health > 0;
    public bool ShouldRemove { get; protected set; }
    public abstract Color EnemyColor { get; }

    protected float Size => GamelabGame.Instance.GameplayConfig.EnemySize;
    protected readonly GameplayContext gameplayContext;

    protected AbstractEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, EnemyTrainSlot slot)
    {
        Health = GamelabGame.Instance.GameplayConfig.EnemyHealth;
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
    }

    public virtual void Update(float deltaTime)
    {
        // TODO: Implement off-screen removal, for now just remove if off screen left.
        if (IsOffScreenLeft())
        {
            ShouldRemove = true;
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        var vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();
        vfxService.EmitBurst(ParticleFactory.CreateBloodSplatter(this.Position));
        if (Health <= 0)
        {
            ShouldRemove = true;
        }
    }

    public void OnHit(AbstractProjectile projectile)
    {
        if (projectile is not CannonProjectile || !IsAlive || ShouldRemove)
        {
            return;
        }

        TakeDamage(projectile.Damage);
        projectile.Deactivate();
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

    protected void MoveTowards(Vector2 targetPosition, float maxDistance)
    {
        Vector2 offset = targetPosition - Position;
        float distance = offset.Length();

        if (distance <= maxDistance || distance <= 0.001f)
        {
            Position = targetPosition;
            return;
        }

        Position += offset / distance * maxDistance;
    }
}