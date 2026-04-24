using Gamelab.Enemies.Core;
using Gamelab.Enemies.Hazards;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.PhysicalEntities.Projectiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Types;

public enum MolotovState
{
    ApproachingSideAttackSlot,
    AimingThrow,
    Recovering
}

public class MolotovEnemy(Vector2 spawnPosition, EnemyTrainSlot slot) : AbstractEnemy(
    new EnemyDefinition(EnemyType.Molotov),
    spawnPosition,
    slot,
    EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.MolotovMaxSpeed))
{
    public override Color EnemyColor => currentState switch
    {
        MolotovState.ApproachingSideAttackSlot => Color.DarkOrange,
        MolotovState.AimingThrow => Color.OrangeRed,
        MolotovState.Recovering => Color.Orange,
        _ => Color.DarkOrange
    };

    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.MolotovPreferredDistance;
    private float AimDurationSeconds => GamelabGame.Instance.GameplayConfig.MolotovAimDurationSeconds;
    private float RecoverDurationSeconds => GamelabGame.Instance.GameplayConfig.MolotovRecoverDurationSeconds;
    private float ProjectileSpeed => GamelabGame.Instance.GameplayConfig.MolotovProjectileSpeed;
    private float ProjectileLifetime => GamelabGame.Instance.GameplayConfig.MolotovProjectileLifetime;
    private float ProjectileSize => GamelabGame.Instance.GameplayConfig.MolotovProjectileSize;
    private float FireZoneRadius => GamelabGame.Instance.GameplayConfig.FireZoneRadius;
    private float FireZoneDurationSeconds => GamelabGame.Instance.GameplayConfig.FireZoneDurationSeconds;

    private MolotovState currentState = MolotovState.ApproachingSideAttackSlot;
    private float stateTimer;
    private MolotovProjectile pendingProjectile;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vector2 slotAnchor = Slot.GetAnchor(Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(slotAnchor);

        switch (currentState)
        {
            case MolotovState.ApproachingSideAttackSlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = MolotovState.AimingThrow;
                    stateTimer = AimDurationSeconds;
                }

                break;

            case MolotovState.AimingThrow:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f && pendingProjectile == null)
                {
                    pendingProjectile = CreateProjectile();
                    currentState = MolotovState.Recovering;
                    stateTimer = RecoverDurationSeconds;
                }

                break;

            case MolotovState.Recovering:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                stateTimer -= deltaTime;
                if (stateTimer <= 0f)
                {
                    currentState = MolotovState.AimingThrow;
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

        MolotovProjectile projectile = pendingProjectile;
        pendingProjectile = null;
        return projectile;
    }

    private MolotovProjectile CreateProjectile()
    {
        Vector2 target = gameplayContext.Map.GetBounds().Center.ToVector2();
        Vector2 direction = target - Position;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        return new MolotovProjectile(
            Position,
            direction * ProjectileSpeed,
            ProjectileLifetime,
            ProjectileSize,
            FireZoneRadius,
            FireZoneDurationSeconds);
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