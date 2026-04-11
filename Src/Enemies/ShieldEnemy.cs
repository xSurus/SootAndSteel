using System;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Projectiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public enum ShieldState
{
    ApproachingSideAttackSlot,
    Shielding,
    Aiming,
    Recovering
}

public class ShieldEnemy : AbstractEnemy
{
    public override Color EnemyColor => currentState switch
    {
        ShieldState.ApproachingSideAttackSlot => Color.IndianRed,
        ShieldState.Shielding => Color.SteelBlue,
        ShieldState.Aiming => Color.OrangeRed,
        ShieldState.Recovering => Color.Orange,
        _ => Color.IndianRed
    };

    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.ShieldPreferredDistance;
    private float EnemyShootSpread => GamelabGame.Instance.GameplayConfig.EnemyShootSpread;
    private float ProjectileSpeed => GamelabGame.Instance.GameplayConfig.ProjectileSpeed;
    private float ProjectileDamage => GamelabGame.Instance.GameplayConfig.ProjectileDamage;
    private float ProjectileLifetime => GamelabGame.Instance.GameplayConfig.ProjectileLifetime;
    private float ProjectileSize => GamelabGame.Instance.GameplayConfig.ProjectileSize;
    private float ShieldDurationSeconds => GamelabGame.Instance.GameplayConfig.ShieldDurationSeconds;
    private float ShieldAimDurationSeconds => GamelabGame.Instance.GameplayConfig.ShieldAimDurationSeconds;
    private float ShieldRecoverDurationSeconds => GamelabGame.Instance.GameplayConfig.ShieldRecoverDurationSeconds;
    private float ShieldBlockArcDegrees => GamelabGame.Instance.GameplayConfig.ShieldBlockArcDegrees;

    private readonly Random random;
    private ShieldState currentState = ShieldState.ApproachingSideAttackSlot;
    private float stateTimer;
    private bool pendingShot;

    public ShieldEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, Random random, EnemyTrainSlot slot)
        : base(
            gameplayContext,
            new EnemyDefinition(EnemyType.Shield),
            spawnPosition,
            slot,
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.ShieldMaxSpeed))
    {
        this.random = random;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vector2 slotAnchor = Slot.GetAnchor(gameplayContext, Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(slotAnchor);

        switch (currentState)
        {
            case ShieldState.ApproachingSideAttackSlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = ShieldState.Shielding;
                    stateTimer = ShieldDurationSeconds;
                }
                break;

            case ShieldState.Shielding:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f)
                {
                    currentState = ShieldState.Aiming;
                    stateTimer = ShieldAimDurationSeconds;
                }
                break;

            case ShieldState.Aiming:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f)
                {
                    pendingShot = true;
                    currentState = ShieldState.Recovering;
                    stateTimer = ShieldRecoverDurationSeconds;
                }
                break;

            case ShieldState.Recovering:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f)
                {
                    currentState = ShieldState.Shielding;
                    stateTimer = ShieldDurationSeconds;
                }
                break;
        }
    }

    public override EnemyProjectile TryShoot()
    {
        if (!pendingShot || !IsAlive || ShouldRemove)
        {
            return null;
        }

        pendingShot = false;

        Vector2 targetPoint = EnemyTargetingHelper.GetTargetPoint(gameplayContext, Slot.Side, Position);
        Vector2 direction = targetPoint - Position;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        float spread = (random.NextSingle() - 0.5f) * EnemyShootSpread;
        float angle = (float)Math.Atan2(direction.Y, direction.X) + spread;
        direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

        return new EnemyProjectile(
            PhysicsBody.World,
            Position,
            direction * ProjectileSpeed,
            ProjectileDamage,
            ProjectileLifetime,
            ProjectileSize
        );
    }

    public override void OnHit(AbstractProjectile projectile)
    {
        if (projectile is not CannonProjectile || !IsAlive || ShouldRemove)
        {
            return;
        }

        if (currentState == ShieldState.Shielding && IsProjectileBlocked(projectile))
        {
            projectile.Deactivate();
            return;
        }

        base.OnHit(projectile);
    }

    private bool IsProjectileBlocked(AbstractProjectile projectile)
    {
        Vector2 shieldForward = gameplayContext.Map.GetBounds().Center.ToVector2() - Position;
        if (shieldForward == Vector2.Zero)
        {
            return false;
        }

        shieldForward.Normalize();
        Vector2 incomingDirection = Position - projectile.Position;
        if (incomingDirection == Vector2.Zero)
        {
            return false;
        }

        incomingDirection.Normalize();
        float cosThreshold = (float)Math.Cos(MathHelper.ToRadians(ShieldBlockArcDegrees * 0.5f));
        return Vector2.Dot(shieldForward, incomingDirection) >= cosThreshold;
    }

    private Vector2 GetApproachAnchor(Vector2 slotAnchor)
    {
        float horizontalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize * 1.5f;
        float verticalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        return Slot.Side switch
        {
            EnemySlotSide.Top => new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y - verticalOffset),
            EnemySlotSide.Bottom => new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y + verticalOffset),
            _ => slotAnchor
        };
    }
}
