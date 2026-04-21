using Gamelab.Enemies.Core;
using Gamelab.Enemies.Hazards;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.PhysicalEntities.Projectiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Types;

public enum TarThrowerState
{
    ApproachingSideAttackSlot,
    AimingThrow,
    Recovering
}

public class TarThrowerEnemy(Vector2 spawnPosition, EnemyTrainSlot slot) : AbstractEnemy(
    new EnemyDefinition(EnemyType.TarThrower),
    spawnPosition,
    slot,
    EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.TarThrowerMaxSpeed))
{
    public override Color EnemyColor => currentState switch
    {
        TarThrowerState.ApproachingSideAttackSlot => Color.DarkSlateBlue,
        TarThrowerState.AimingThrow => Color.MediumPurple,
        TarThrowerState.Recovering => Color.MediumSlateBlue,
        _ => Color.DarkSlateBlue
    };

    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.TarThrowerPreferredDistance;
    private float AimDurationSeconds => GamelabGame.Instance.GameplayConfig.TarThrowerAimDurationSeconds;
    private float RecoverDurationSeconds => GamelabGame.Instance.GameplayConfig.TarThrowerRecoverDurationSeconds;
    private float ProjectileSpeed => GamelabGame.Instance.GameplayConfig.TarProjectileSpeed;
    private float ProjectileLifetime => GamelabGame.Instance.GameplayConfig.TarProjectileLifetime;
    private float ProjectileSize => GamelabGame.Instance.GameplayConfig.TarProjectileSize;
    private float CleanDurationSeconds => GamelabGame.Instance.GameplayConfig.TarCleanDurationSeconds;

    private TarThrowerState currentState = TarThrowerState.ApproachingSideAttackSlot;
    private float stateTimer;
    private TarProjectile pendingProjectile;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vector2 slotAnchor = Slot.GetAnchor(Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(slotAnchor);

        switch (currentState)
        {
            case TarThrowerState.ApproachingSideAttackSlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = TarThrowerState.AimingThrow;
                    stateTimer = AimDurationSeconds;
                }

                break;

            case TarThrowerState.AimingThrow:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f && pendingProjectile == null)
                {
                    pendingProjectile = CreateProjectile();
                    currentState = TarThrowerState.Recovering;
                    stateTimer = RecoverDurationSeconds;
                }

                break;

            case TarThrowerState.Recovering:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f)
                {
                    currentState = TarThrowerState.AimingThrow;
                    stateTimer = AimDurationSeconds;
                }

                break;
        }
    }

    public override IEnemyHazard TryCreateHazard()
    {
        if (pendingProjectile == null)
        {
            return null;
        }

        TarProjectile projectile = pendingProjectile;
        pendingProjectile = null;
        return projectile;
    }

    private TarProjectile CreateProjectile()
    {
        Vector2 target = gameplayContext.Map.GetBounds().Center.ToVector2();
        Vector2 direction = target - Position;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        return new TarProjectile(
            Position,
            direction * ProjectileSpeed,
            ProjectileLifetime,
            ProjectileSize,
            Slot.Side,
            CleanDurationSeconds);
    }

    private Vector2 GetApproachAnchor(Vector2 slotAnchor)
    {
        float horizontalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize * 1.3f;
        float verticalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.8f;

        return Slot.Side switch
        {
            EnemySlotSide.Top => new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y - verticalOffset),
            EnemySlotSide.Bottom => new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y + verticalOffset),
            _ => slotAnchor
        };
    }
}