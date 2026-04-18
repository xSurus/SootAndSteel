using System;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.Services.Bullet;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public enum ShieldState
{
    ApproachingSideAttackSlot,
    Shielding,
    Aiming,
    Recovering
}

public class ShieldEnemy(Vector2 spawnPosition, EnemyTrainSlot slot) : AbstractEnemy(
    new EnemyDefinition(EnemyType.Shield),
    spawnPosition,
    slot,
    EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.ShieldMaxSpeed))
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
    private float ShieldDurationSeconds => GamelabGame.Instance.GameplayConfig.ShieldDurationSeconds;
    private float ShieldAimDurationSeconds => GamelabGame.Instance.GameplayConfig.ShieldAimDurationSeconds;
    private float ShieldRecoverDurationSeconds => GamelabGame.Instance.GameplayConfig.ShieldRecoverDurationSeconds;
    private float ShieldBlockArcDegrees => GamelabGame.Instance.GameplayConfig.ShieldBlockArcDegrees;

    private ShieldState currentState = ShieldState.ApproachingSideAttackSlot;
    private float stateTimer;
    private bool pendingShot;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vector2 slotAnchor = Slot.GetAnchor(Size + PreferredDistance);
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

    public override void TryShoot()
    {
        if (!pendingShot || !IsAlive || ShouldRemove)
        {
            return;
        }

        pendingShot = false;

        Vector2 targetPoint = EnemyTargetingHelper.GetTargetPoint(gameplayContext, Slot.Side, Position);
        Vector2 direction = targetPoint - Position;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        float spread = (Random.Shared.NextSingle() - 0.5f) * EnemyShootSpread;
        float angle = (float)Math.Atan2(direction.Y, direction.X) + spread;
        direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

        BulletItem ammo = new BulletItem("BasicProjectile", "BasicCasing", "BasicPropellant", "EnemyProjectile");
        GamelabGame.Instance.Services.GetService<IBulletService>().EmitBullet(
            ammo,
            Position,
            direction,
            this);
    }

    public override bool OnHit(BulletEntity bullet)
    {
        if (bullet.Owner is not CannonStation || !IsAlive || ShouldRemove)
        {
            return false;
        }

        if (currentState == ShieldState.Shielding && IsProjectileBlocked(bullet))
        {
            return true;
        }

        return base.OnHit(bullet);
    }

    private bool IsProjectileBlocked(BulletEntity bullet)
    {
        Vector2 shieldForward = gameplayContext.Map.GetBounds().Center.ToVector2() - Position;
        if (shieldForward == Vector2.Zero)
        {
            return false;
        }

        shieldForward.Normalize();
        Vector2 incomingDirection = Position - bullet.Position;
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