using System;
using Gamelab.Assets;
using Gamelab.Enemies.Core;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.Items.Bullets;
using Gamelab.Services.Bullet;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Enemies.Types;

public enum EnemyState
{
    ApproachingSideAttackSlot,
    HoldingSideAttackSlot
}

public class Enemy : AbstractEnemy
{
    private const float HorseSpriteScale = 0.45f;
    private const float HorseFramesPerSecond = 12f;
    private const float AttackPoseDurationSeconds = 0.5f;

    private static readonly string[] HorseFrameKeys =
    [
        "horse01",
        "horse02",
        "horse03",
        "horse04",
        "horse05",
        "horse06",
        "horse07",
        "horse08",
        "horse09",
        "horse010"
    ];

    private float ShootCooldown => GamelabGame.Instance.GameplayConfig.EnemyShootCooldown;
    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.RiflePreferredDistance;
    private float EnemyShootSpread => GamelabGame.Instance.GameplayConfig.EnemyShootSpread;
    private float HorseFrameDuration => 1f / HorseFramesPerSecond;

    private readonly Random random = Random.Shared;
    private readonly Texture2D[] horseFrames;

    private float timeSinceLastShot;
    private EnemyState currentState = EnemyState.ApproachingSideAttackSlot;
    private int currentHorseFrame;
    private float horseAnimationTimer;
    private float attackPoseTimer;
    private float attackAngle;

    public Enemy(Vector2 spawnPosition, EnemyTrainSlot slot)
        : base(spawnPosition,
            slot,
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.RifleMaxSpeed))
    {
        timeSinceLastShot = random.NextSingle() * ShootCooldown;
        horseFrames = new Texture2D[HorseFrameKeys.Length];
        for (int i = 0; i < HorseFrameKeys.Length; i++)
        {
            horseFrames[i] = AssetManager.GetEnemyTexture(HorseFrameKeys[i]);
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        UpdateHorseAnimation(deltaTime);
        attackPoseTimer = Math.Max(0f, attackPoseTimer - deltaTime);

        Vector2 slotAnchor = Slot.GetAnchor(Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(slotAnchor);

        switch (currentState)
        {
            case EnemyState.ApproachingSideAttackSlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = EnemyState.HoldingSideAttackSlot;
                }

                break;

            case EnemyState.HoldingSideAttackSlot:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime, includeTrainDrift: false);
                break;
        }

        timeSinceLastShot += deltaTime;
    }

    public override void TryShoot()
    {
        if (currentState == EnemyState.ApproachingSideAttackSlot || timeSinceLastShot < ShootCooldown || !IsAlive ||
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

        attackAngle = angle;
        attackPoseTimer = AttackPoseDurationSeconds;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || ShouldRemove)
        {
            return;
        }

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 drawPosition = Position + new Vector2(-2.25f * tileSize, -tileSize * 1.125f);
        Vector2 rifleBob = new Vector2(0f, currentHorseFrame / 3f);
        Vector2 feetPosition = Position + new Vector2(0f, Size / 2f);
        float horseDepth = RenderUtility.CalculateDepth(feetPosition.Y);
        float riderDepth = MathF.Min(RenderUtility.TopEntityLayer - 3f * RenderUtility.Eps,
            horseDepth + RenderUtility.Eps);
        float weaponDepth = MathF.Min(RenderUtility.TopEntityLayer - 2f * RenderUtility.Eps,
            riderDepth + RenderUtility.Eps);

        DrawSprite(spriteBatch, horseFrames[currentHorseFrame], drawPosition, SpriteEffects.None, horseDepth);

        if (attackPoseTimer <= 0f)
        {
            DrawSprite(spriteBatch, AssetManager.GetEnemyTexture("RifleIdle"), drawPosition + rifleBob,
                SpriteEffects.None, riderDepth);
            return;
        }

        DrawSprite(spriteBatch, AssetManager.GetEnemyTexture("RifleHeadless"), drawPosition + rifleBob,
            SpriteEffects.None, riderDepth);

        (Texture2D poseTexture, SpriteEffects effects, Vector2 offset) = GetAttackPose(tileSize);
        DrawSprite(spriteBatch, poseTexture, drawPosition + rifleBob + offset, effects, weaponDepth);
    }

    private void UpdateHorseAnimation(float deltaTime)
    {
        horseAnimationTimer += deltaTime;
        while (horseAnimationTimer >= HorseFrameDuration)
        {
            currentHorseFrame = (currentHorseFrame + 1) % horseFrames.Length;
            horseAnimationTimer -= HorseFrameDuration;
        }
    }

    private (Texture2D Texture, SpriteEffects Effects, Vector2 Offset) GetAttackPose(int tileSize)
    {
        float adjustedAngle = Slot.Side == EnemySlotSide.Bottom ? -attackAngle : attackAngle;
        Vector2 flipOffset = new Vector2(tileSize * 0.5f, 0f);

        if (adjustedAngle > 5f * MathF.PI / 6f)
        {
            return (AssetManager.GetEnemyTexture("RifleWide"), SpriteEffects.None, Vector2.Zero);
        }

        if (adjustedAngle > 4f * MathF.PI / 6f)
        {
            return (AssetManager.GetEnemyTexture("RifleSemi"), SpriteEffects.None, Vector2.Zero);
        }

        if (adjustedAngle >= 3f * MathF.PI / 6f)
        {
            return (AssetManager.GetEnemyTexture("RifleMiddle"), SpriteEffects.None, Vector2.Zero);
        }

        if (adjustedAngle >= 2f * MathF.PI / 6f)
        {
            return (AssetManager.GetEnemyTexture("RifleMiddle"), SpriteEffects.FlipHorizontally, flipOffset);
        }

        if (adjustedAngle >= 1f * MathF.PI / 6f)
        {
            return (AssetManager.GetEnemyTexture("RifleSemi"), SpriteEffects.FlipHorizontally, flipOffset);
        }

        return (AssetManager.GetEnemyTexture("RifleWide"), SpriteEffects.FlipHorizontally, flipOffset);
    }

    private static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, SpriteEffects effects,
        float depth)
    {
        spriteBatch.Draw(
            texture ?? AssetManager.BlankTexture,
            position,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            HorseSpriteScale,
            effects,
            depth);
    }

    private Vector2 GetApproachAnchor(Vector2 slotAnchor)
    {
        float horizontalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize * 1.5f;
        float verticalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        return Slot.Side == EnemySlotSide.Top
            ? new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y - verticalOffset)
            : new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y + verticalOffset);
    }
}
