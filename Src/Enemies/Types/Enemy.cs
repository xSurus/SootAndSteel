using System;
using Gamelab.Assets;
using Gamelab.Enemies.Core;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.Items.Bullets;
using Gamelab.Services.Animation;
using Gamelab.Services.Bullet;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace Gamelab.Enemies.Types;

public enum HorseState
{
    ApproachingSideAttackSlot,
    HoldingSideAttackSlot,
    Fleeing
}

public enum RiderState
{
    Idle,
    Aiming,
    Recoil,
    Dead
}

public class Enemy : AbstractEnemy
{
    private const float HorseSpriteScale = 0.45f;
    private const float HorseFramesPerSecond = 12f;
    private const int HorseRunCycleFrames = 10;
    private const float AimDurationSeconds = 0.75f;
    private const float RecoilDurationSeconds = 0.4f;

    private float ShootCooldown => GamelabGame.Instance.GameplayConfig.EnemyShootCooldown;
    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.RiflePreferredDistance;
    private float EnemyShootSpread => GamelabGame.Instance.GameplayConfig.EnemyShootSpread;

    private readonly Random random = Random.Shared;
    private HorseState currentState = HorseState.ApproachingSideAttackSlot;
    private RiderState currentRiderState = RiderState.Idle;
    private float timeSinceLastShot;
    private float attackPoseTimer;
    private float attackAngle;
    private float aimTimer;
    private float recoilTimer;
    private float _fleeTimer;
    private float _fleeDirection;

    private readonly AnimatedSprite horseSprite;
    private readonly AnimatedSprite riderTorsoSprite;
    private readonly AnimatedSprite riderHeadSprite;
    private readonly IAnimationService animationService;
    private float animationTimer;
    private ParticleEmitter _neckBleedEmitter;

    public Enemy(Vector2 spawnPosition, EnemyTrainSlot slot)
        : base(spawnPosition, slot,
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.RifleMaxSpeed))
    {
        timeSinceLastShot = random.NextSingle() * ShootCooldown;

        animationService = GamelabGame.Instance.Services.GetService<IAnimationService>();
        horseSprite = new AnimatedSprite(AssetManager.EnemySpriteSheet);
        horseSprite.SetAnimation("Run");

        riderHeadSprite = new AnimatedSprite(AssetManager.EnemySpriteSheet);
        riderHeadSprite.SetAnimation("RifleIdle");

        riderTorsoSprite = new AnimatedSprite(AssetManager.EnemySpriteSheet);
        riderTorsoSprite.SetAnimation("RifleHeadless");

        animationService.Register(riderHeadSprite);
        animationService.Register(riderTorsoSprite);
        animationService.Register(horseSprite);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (ShouldRemove && _neckBleedEmitter != null)
        {
            _neckBleedEmitter.ShouldRemove = true;
            _neckBleedEmitter = null;
        }

        animationTimer += deltaTime;
        attackPoseTimer = Math.Max(0f, attackPoseTimer - deltaTime);
        timeSinceLastShot += deltaTime;

        UpdateRiderStateMachine(deltaTime);
        UpdateHorseStateMachine(deltaTime);
        UpdateAnimationStates();
    }

    private void UpdateRiderStateMachine(float deltaTime)
    {
        switch (currentRiderState)
        {
            case RiderState.Idle:
                break;

            case RiderState.Aiming:
                aimTimer -= deltaTime;
                UpdateAttackAngle();

                if (aimTimer <= 0)
                {
                    ExecuteFire();
                    currentRiderState = RiderState.Recoil;
                    recoilTimer = RecoilDurationSeconds;
                }

                break;

            case RiderState.Recoil:
                recoilTimer -= deltaTime;
                if (recoilTimer <= 0)
                {
                    currentRiderState = RiderState.Idle;
                }

                break;

            case RiderState.Dead:
                _fleeTimer -= deltaTime;
                if (_fleeTimer <= 0)
                {
                    _fleeDirection = 1f;
                    currentState = HorseState.Fleeing;
                }
                if (_neckBleedEmitter != null)
                    _neckBleedEmitter.Position = GetNeckPosition();
                break;
        }
    }

    private void UpdateHorseStateMachine(float deltaTime)
    {
        Vector2 slotAnchor = Slot.GetAnchor(Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(slotAnchor);

        switch (currentState)
        {
            case HorseState.ApproachingSideAttackSlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = HorseState.HoldingSideAttackSlot;
                }

                break;

            case HorseState.HoldingSideAttackSlot:
                EnemyMovement.UpdateHoldPosition(slotAnchor, deltaTime, includeTrainDrift: false);
                break;

            case HorseState.Fleeing:
                PhysicsBody.LinearVelocity = new Vector2(_fleeDirection * GamelabGame.Instance.GameplayConfig.RifleMaxSpeed * 1.1f, 0f).ToMeters();
                if (Position.X > gameplayContext.ScreenWidth + 600f || Position.X < -Size)
                {
                    ShouldRemove = true;
                }
                break;
        }
    }

    public override void TryShoot()
    {
        if (currentRiderState == RiderState.Dead) return;

        if (currentState != HorseState.HoldingSideAttackSlot ||
            timeSinceLastShot < ShootCooldown ||
            currentRiderState != RiderState.Idle)
            return;

        currentRiderState = RiderState.Aiming;
        aimTimer = AimDurationSeconds;
        timeSinceLastShot = 0f;
    }

    private void ExecuteFire()
    {
        Vector2 targetPoint = EnemyTargetingHelper.GetTargetPoint(gameplayContext, Slot.Side, Position);
        Vector2 direction = Vector2.Normalize(targetPoint - Position);

        float spread = (random.NextSingle() - 0.5f) * EnemyShootSpread;
        float finalAngle = (float)Math.Atan2(direction.Y, direction.X) + spread;
        Vector2 finalDir = new Vector2((float)Math.Cos(finalAngle), (float)Math.Sin(finalAngle));

        BulletItem ammo = new BulletItem(ComponentIds.BasicProjectile, ComponentIds.BasicCasing,
            ComponentIds.BasicPropellant, ComponentIds.EnemyProjectile);
        GamelabGame.Instance.Services.GetService<IBulletService>().EmitBullet(ammo, Position, finalDir, this);
    }

    public override bool OnHit(BulletEntity bullet)
    {
        if (currentRiderState == RiderState.Dead) return false;

        if (bullet.InitialShooter.GetType() == typeof(CannonStation) && IsAlive && !ShouldRemove)
        {
            if (Health - bullet.Stats.Damage <= 0)
            {
                Health = 1;
                var vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();
                vfxService.EmitBurst(ParticleFactory.CreateBloodSplatter(GetNeckPosition()));
                StartFleeingDeath(vfxService);
                return true;
            }
        }

        return base.OnHit(bullet);
    }

    private void StartFleeingDeath(IVfxService vfxService)
    {
        currentRiderState = RiderState.Dead;
        _fleeTimer = GamelabGame.Instance.GameplayConfig.EnemyFleeDelay;

        _neckBleedEmitter = ParticleFactory.CreateNeckBleed(GetNeckPosition());
        vfxService.AddContinuous(_neckBleedEmitter);
    }

    private Vector2 GetNeckPosition()
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        // RifleHeadless neck stump top at frame px (372, 174), drawn at scale 0.45
        // from draw offset (-2.25 * tileSize, -1.125 * tileSize)
        return Position + new Vector2(
            -2.25f * tileSize + 372 * HorseSpriteScale,
            -1.125f * tileSize + 174 * HorseSpriteScale);
    }

    private void UpdateAttackAngle()
    {
        Vector2 targetPoint = EnemyTargetingHelper.GetTargetPoint(gameplayContext, Slot.Side, Position);
        Vector2 direction = targetPoint - Position;
        attackAngle = (float)Math.Atan2(direction.Y, direction.X);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || ShouldRemove) return;

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 drawPosition = Position + new Vector2(-2.25f * tileSize, -tileSize * 1.125f);

        int simulatedFrame = (int)(animationTimer * HorseFramesPerSecond) % HorseRunCycleFrames;
        Vector2 rifleBob = new Vector2(0f, simulatedFrame / 3f);

        float horseDepth = RenderUtility.CalculateDepth(Position.Y + Size / 2f);
        float riderDepth = horseDepth + 0.0001f;
        float armDepth = horseDepth + 0.0002f;

        horseSprite.Depth = horseDepth;
        spriteBatch.Draw(horseSprite, drawPosition, 0f, new Vector2(HorseSpriteScale));

        riderTorsoSprite.Depth = riderDepth;
        spriteBatch.Draw(riderTorsoSprite, drawPosition + rifleBob, 0f, new Vector2(HorseSpriteScale));

        if (currentRiderState != RiderState.Dead)
        {
            riderHeadSprite.Depth = armDepth;
            riderHeadSprite.Effect = (attackAngle > MathF.PI / 2f || attackAngle < -MathF.PI / 2f)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;
            spriteBatch.Draw(riderHeadSprite, drawPosition + rifleBob, 0f, new Vector2(HorseSpriteScale));
        }
    }

    private void UpdateAnimationStates()
    {
        if (currentRiderState == RiderState.Dead) return;

        if (currentRiderState == RiderState.Idle)
        {
            riderHeadSprite.SetAnimation("RifleIdle");
        }
        else
        {
            string animationName = GetAimAnimationName();
            riderHeadSprite.SetAnimation(animationName);
        }
    }

    private string GetAimAnimationName()
    {
        float adjustedAngle = Slot.Side == EnemySlotSide.Bottom ? -attackAngle : attackAngle;

        if (adjustedAngle > 5f * MathF.PI / 6f) return "RifleWide";
        if (adjustedAngle > 4f * MathF.PI / 6f) return "RifleSemi";
        return "RifleMiddle";
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