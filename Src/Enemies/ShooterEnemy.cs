using System;
using Gamelab.Config;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Bullets.Components.Casings;
using Gamelab.PhysicalEntities.Bullets.Components.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Components.Propellants;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Services.Bullet;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Enemies;

public enum ShooterState
{
    ApproachingSlot,
    HoldingSlot,
    FallingBehind
}

public class ShooterEnemy : AbstractEnemy
{
    public override Color EnemyColor => Color.Red;

    private float ShootCooldown => GamelabGame.Instance.GameplayConfig.EnemyShootCooldown;
    private float HorseSpeed => GamelabGame.Instance.GameplayConfig.ShooterMaxSpeed;
    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.ShooterPreferredDistance;
    private float EnemyShootSpread => GamelabGame.Instance.GameplayConfig.EnemyShootSpread;
    private float timeSinceLastShot;
    private readonly Random random;
    private ShooterState currentState = ShooterState.ApproachingSlot;
    
    public ShooterEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, Random random, EnemyTrainSlot slot) 
        : base(gameplayContext, spawnPosition, slot)
    {
        this.random = random;
        // TODO create min and max duration so that the enemy doesnt shoot immediately shoot
        timeSinceLastShot = random.NextSingle() * ShootCooldown;
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        Vector2 slotAnchor = Slot.GetAnchor(gameplayContext, Size + PreferredDistance);
        switch (currentState)
        {
            case ShooterState.ApproachingSlot:
                Vector2 approachAnchor = GetApproachAnchor(slotAnchor);
                MoveTowards(approachAnchor, HorseSpeed * deltaTime);

                float approachSnapDistance = Size * 0.4f;
                if (Vector2.DistanceSquared(Position, approachAnchor) <= approachSnapDistance * approachSnapDistance)
                {
                    currentState = ShooterState.HoldingSlot;
                }
                break;

            case ShooterState.HoldingSlot:
                MoveTowards(slotAnchor, HorseSpeed * deltaTime);
                if (gameplayContext.State.actualSpeed > HorseSpeed)
                {
                    currentState = ShooterState.FallingBehind;
                }
                break;

            case ShooterState.FallingBehind:
                UpdateFallingBehind(deltaTime, gameplayContext.State.actualSpeed, slotAnchor);
                if (gameplayContext.State.actualSpeed <= HorseSpeed)
                {
                    currentState = ShooterState.HoldingSlot;
                }
                break;
        }


        timeSinceLastShot += deltaTime;
    }
    
    public void TryShoot()
    {
        if (currentState == ShooterState.ApproachingSlot || timeSinceLastShot < ShootCooldown || !IsAlive || ShouldRemove)
        {
            return;
        }
        
        timeSinceLastShot = 0f;
        
        Vector2 direction = gameplayContext.Map.GetBounds().Center.ToVector2() - Position;
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

    private void UpdateFallingBehind(float deltaTime, float trainSpeed, Vector2 slotAnchor)
    {
        float fallBehindSpeed = trainSpeed - HorseSpeed;
        float yDifference = slotAnchor.Y - Position.Y;
        float maxVerticalMovement = HorseSpeed * 0.5f * deltaTime;
        float verticalMovement = Math.Clamp(yDifference, -maxVerticalMovement, maxVerticalMovement);

        Position = new Vector2(
            Position.X - fallBehindSpeed * deltaTime,
            Position.Y + verticalMovement
        );
    }
}
