using System;
using Gamelab.Enemies.Core;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.Items.Bullets;
using Gamelab.Services.Bullet;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Types;

public enum RifleState
{
    ApproachingSideAttackSlot,
    HoldingSideAttackSlot
}

public class RifleEnemy : AbstractEnemy
{
    public override Color EnemyColor => Color.Red;
    private float ShootCooldown => GamelabGame.Instance.GameplayConfig.EnemyShootCooldown;
    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.RiflePreferredDistance;
    private float EnemyShootSpread => GamelabGame.Instance.GameplayConfig.EnemyShootSpread;

    private readonly Random random = Random.Shared;
    private float timeSinceLastShot;
    private RifleState currentState = RifleState.ApproachingSideAttackSlot;

    public RifleEnemy(Vector2 spawnPosition, EnemyTrainSlot slot)
        : base(new EnemyDefinition(EnemyType.Rifle),
            spawnPosition,
            slot,
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.RifleMaxSpeed))
    {
        timeSinceLastShot = random.NextSingle() * ShootCooldown;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vector2 slotAnchor = Slot.GetAnchor(Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(slotAnchor);

        switch (currentState)
        {
            case RifleState.ApproachingSideAttackSlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = RifleState.HoldingSideAttackSlot;
                }

                break;

            case RifleState.HoldingSideAttackSlot:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime);
                break;
        }

        timeSinceLastShot += deltaTime;
    }

    public override void TryShoot()
    {
        if (currentState == RifleState.ApproachingSideAttackSlot || timeSinceLastShot < ShootCooldown || !IsAlive ||
            ShouldRemove)
        {
            return;
        }

        timeSinceLastShot = 0f;

        Vector2 targetPoint = EnemyTargetingHelper.GetTargetPoint(gameplayContext, Slot.Side, Position);
        Vector2 direction = targetPoint - Position;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        float spread = (random.NextSingle() - 0.5f) * EnemyShootSpread;
        float angle = (float)Math.Atan2(direction.Y, direction.X) + spread;
        direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

        BulletItem ammo = new BulletItem("BasicProjectile", "BasicCasing", "BasicPropellant", "EnemyProjectile");
        GamelabGame.Instance.Services.GetService<IBulletService>().EmitBullet(
            ammo,
            Position,
            direction,
            this);
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