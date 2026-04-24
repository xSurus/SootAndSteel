using System;
using Gamelab.Config;
using Gamelab.Assets;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Bullets.Components.Casings;
using Gamelab.PhysicalEntities.Bullets.Components.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Components.Propellants;
using Gamelab.Services.Bullet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gamelab.Utils.Logging;

namespace Gamelab.Enemies;

public enum RifleState
{
    ApproachingSideAttackSlot,
    HoldingSideAttackSlot

}

public enum SpriteState
{
    Idle,
    Attack
}


public class RifleEnemy : AbstractEnemy
{
    public override Color EnemyColor => Color.Red;
    private static readonly Logger logger = new ("Rifle");

    private float ShootCooldown => GamelabGame.Instance.GameplayConfig.EnemyShootCooldown;
    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.RiflePreferredDistance;
    private float EnemyShootSpread => GamelabGame.Instance.GameplayConfig.EnemyShootSpread;

    private readonly Random random;
    private float timeSinceLastShot;
    private RifleState currentState = RifleState.ApproachingSideAttackSlot;

    private Texture2D[] horseSprite;
    private int currentFrame = 0;
    private float animationTimer = 0f;
    private float timePerFrame = 1f / 12;

    private SpriteState currentSprite;
    private float sprite_angle = 0f; 
    private float attackTimer = 0f;
    private Vector2 direction;

    public RifleEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, Random random, EnemyTrainSlot slot)
        : base(
            gameplayContext,
            new EnemyDefinition(EnemyType.Rifle),
            spawnPosition,
            slot,
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.RifleMaxSpeed))
    {
        this.random = random;
        timeSinceLastShot = random.NextSingle() * ShootCooldown;
        horseSprite = new Texture2D[10];
        for (int i = 0; i < 10; i++){
            horseSprite[i] = AssetManager.GetEnemyTexture($"horse0{i+1}");
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        animationTimer += deltaTime;
        if (animationTimer >= timePerFrame)
        {
            currentFrame++;
            if (currentFrame >= horseSprite.Length)
            {
                currentFrame = 0;
            }
            
            animationTimer -= timePerFrame; 
        }

        Vector2 slotAnchor = Slot.GetAnchor(gameplayContext, Size + PreferredDistance);
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
        attackTimer += deltaTime;
        
    }
    
    public override void TryShoot()
    {
        if (currentState == RifleState.ApproachingSideAttackSlot || timeSinceLastShot < ShootCooldown || !IsAlive || ShouldRemove)
        {
            if (attackTimer > 6 * timePerFrame) {
                currentSprite = SpriteState.Idle;
                attackTimer = 0;
            }
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

        sprite_angle = angle;
        currentSprite = SpriteState.Attack;
        logger.Info($"Rifle shot has angle '{sprite_angle}'");
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

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (ShouldRemove) return;

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 DrawPos = Position + new Vector2(-3f * tileSize, -tileSize * 1.5f);
        Vector2 Shake = new Vector2(0,currentFrame / 3);
        Vector2 FlipOffset = new Vector2(tileSize * 0.5f,0);
        spriteBatch.Draw(horseSprite[currentFrame], DrawPos, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        

        // Texture2D texture = AssetManager.EnemyTexture;
        // Rectangle destRect = new Rectangle(
        //     (int)(Position.X - Size / 2),
        //     (int)(Position.Y - Size / 2),
        //     (int)Size,
        //     (int)Size
        // );

        // spriteBatch.Draw(texture, destRect, EnemyColor);

        if (currentSprite == SpriteState.Idle){
            spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleIdle"), DrawPos + Shake, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        } else if (currentSprite == SpriteState.Attack){

            spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleHeadless"), DrawPos + Shake, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
            
            if (sprite_angle > 5 * Math.PI / 6)
                spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleWide"), DrawPos + Shake, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
            else if (sprite_angle > 4 * Math.PI / 6)
                spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleSemi"), DrawPos + Shake, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
            else if (sprite_angle >= 3 * Math.PI / 6)
                spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleMiddle"), DrawPos + Shake, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
            else if (sprite_angle >= 2 * Math.PI / 6)
                spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleMiddle"), DrawPos + Shake+ FlipOffset, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.FlipHorizontally, 0f);
            else if (sprite_angle >= 1 * Math.PI / 6)
                spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleSemi"), DrawPos + Shake + FlipOffset, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.FlipHorizontally, 0f);
            else {
                spriteBatch.Draw(AssetManager.GetEnemyTexture("RifleWide"), DrawPos + Shake+ FlipOffset, null, 
                            Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.FlipHorizontally, 0f);
            }

        }
        
    }
}
