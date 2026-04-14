using System;
using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Entities;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

public class Player : AbstractPhysicalEntity, IInteractable, IDamageable
{
    public PlayerConfiguration PlayerConfiguration { get; private set; }
    public Item HeldItem { get; set; }
    public IGrabbable GrabbedObject { get; private set; }
    public PlayerCondition Condition { get; private set; } = PlayerCondition.Active;
    public bool IsStunned => Condition == PlayerCondition.Stunned;
    public float ReviveProgress { get; private set; }

    public Vector2 LookDirection => new((float)Math.Cos(PhysicsBody.Rotation), (float)Math.Sin(PhysicsBody.Rotation));

    private float LerpFactor => GamelabGame.Instance.GameplayConfig.PlayerVelocityLerpFactor;
    private float MaxVelocity => GamelabGame.Instance.GameplayConfig.PlayerMaxVelocity;
    private float InteractDistancePixels => GamelabGame.Instance.GameplayConfig.PlayerInteractDistancePixels;
    private float PlayerStunDurationSeconds => GamelabGame.Instance.GameplayConfig.PlayerStunDurationSeconds;
    private float PlayerReviveDurationSeconds => GamelabGame.Instance.GameplayConfig.PlayerReviveDurationSeconds;

    private float HeldItemOffsetRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerHeldItemOffsetRadiusMultiplier;

    private float HeldItemSizeRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerHeldItemSizeRadiusMultiplier;

    private float Radius => GamelabGame.Instance.GameplayConfig.PlayerRadiusPixels;
    private float Density => GamelabGame.Instance.GameplayConfig.PlayerDensity;
    private float LinearDampening => GamelabGame.Instance.GameplayConfig.PlayerLinearDamping;
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    private float stunTimer;
    private bool revivedThisFrame;


    private string[] idleFrames = {"IdleA","IdleB","IdleC","IdleD"};

    private int currentFrame = 0;
    private float animationTimer = 0f;
    private float timePerFrame = 0.2f;

    public Player(Vector2 startPosition, PlayerConfiguration playerConfig)
    {
        PlayerConfiguration = playerConfig;
        PhysicsBody = gameplayContext.PhysicsWorld.CreateCircle(Radius.ToMeters(), Density, startPosition.ToMeters(),
            BodyType.Dynamic);
        PhysicsBody.LinearDamping = LinearDampening;
        PhysicsBody.FixedRotation = true;
        PhysicsBody.Tag = this;
    }

    public void Update(float dt)
    {   
        animationTimer += dt;
        if (animationTimer >= timePerFrame)
        {
            currentFrame++;
            if (currentFrame >= idleFrames.Length)
            {
                currentFrame = 0;
            }
            
            animationTimer -= timePerFrame; 
        }

        if (IsStunned)
        {
            UpdateStunned(dt);
            return;
        }

        Vector2 movement = PlayerConfiguration.Input.GetMovement();

        float maxTemperature = GamelabGame.Instance.GameplayConfig.TrainMaxTemperature;
        float temperatureRatio = gameplayContext == null || maxTemperature <= 0f
            ? 0f
            : gameplayContext.State.Temperature / maxTemperature;

        float speedScale = Math.Clamp(temperatureRatio, 0f, 1f);
        float effectiveMaxVelocity = MaxVelocity * speedScale;

        if (GrabbedObject == null)
        {
            if (movement != Vector2.Zero) PhysicsBody.Rotation = (float)Math.Atan2(movement.Y, movement.X);
            Vector2 targetVelocity = movement * effectiveMaxVelocity;
            PhysicsBody.LinearVelocity = Vector2.Lerp(PhysicsBody.LinearVelocity, targetVelocity, LerpFactor);
        }
        else if (movement != Vector2.Zero)
        {
            PhysicsBody.ApplyForce(movement * 100f * speedScale);
        }

        if (TryGrab()) return;
        if (TryPickup(dt)) return;
        if (TryInteract(dt)) return;
    }

    public void Stun(float durationSeconds)
    {
        if (durationSeconds <= 0f)
        {
            durationSeconds = PlayerStunDurationSeconds;
        }

        Condition = PlayerCondition.Stunned;
        stunTimer = Math.Max(stunTimer, durationSeconds);
        ReviveProgress = 0f;
        revivedThisFrame = false;
        PhysicsBody.LinearVelocity = Vector2.Zero;

        if (GrabbedObject != null)
        {
            GrabbedObject.OnRelease(this);
            GrabbedObject = null;
        }

        HeldItem = null;
    }

    public void OnInteract(Player interactingPlayer)
    {
    }

    public void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (!IsStunned || interactingPlayer == this)
        {
            return;
        }

        revivedThisFrame = true;
        ReviveProgress += dt / Math.Max(0.01f, PlayerReviveDurationSeconds);
        if (ReviveProgress >= 1f)
        {
            Condition = PlayerCondition.Active;
            stunTimer = 0f;
            ReviveProgress = 0f;
            revivedThisFrame = false;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        Stun(PlayerStunDurationSeconds);
    }

    public bool OnHit(BulletEntity bullet)
    {
        if (bullet.Owner is not AbstractEnemy || IsStunned)
        {
            return false;
        }
        TakeDamage(bullet.Stats.Damage);
        return true;
    }

    private void UpdateStunned(float dt)
    {
        PhysicsBody.LinearVelocity = Vector2.Zero;

        if (!revivedThisFrame)
        {
            ReviveProgress = 0f;
        }

        revivedThisFrame = false;
        stunTimer = Math.Max(0f, stunTimer - dt);
    }

    private bool TryGrab()
    {
        if (PlayerConfiguration.Input.IsGrabJustPressed() && GrabbedObject == null)
        {
            IPhysicalEntity target = GetTargetedEntity();
            if (target == null) return false;
            float reachInMeters = InteractDistancePixels.ToMeters();
            Vector2 grabPointWorldMeters = PhysicsBody.Position + (LookDirection * reachInMeters);
            if (target is IGrabbable grabbable)
            {
                if (grabbable.OnGrab(this, grabPointWorldMeters))
                {
                    GrabbedObject = grabbable;
                    return true;
                }
            }
        }
        else if (!PlayerConfiguration.Input.IsGrabHeld() && GrabbedObject != null)
        {
            GrabbedObject.OnRelease(this);
            GrabbedObject = null;
            return true;
        }

        return false;
    }

    private bool TryInteract(float dt)
    {
        IPhysicalEntity target = GetTargetedEntity();
        if (target == null || target is not IInteractable interactable) return false;

        bool interactJust = PlayerConfiguration.Input.IsInteractJustPressed();
        bool interactHeld = PlayerConfiguration.Input.IsInteractHeld();

        if (interactJust)
        {
            interactable.OnInteract(this);
            return true;
        }

        if (interactHeld)
        {
            interactable.OnInteractHeld(this, dt);
            return true;
        }

        return false;
    }

    private bool TryPickup(float dt)
    {
        IPhysicalEntity target = GetTargetedEntity();
        if (target == null || target is not IPickable pickable) return false;
        if (PlayerConfiguration.Input.IsPickupJustPressed())
        {
            pickable.OnPickup(this);
        }
        else if (PlayerConfiguration.Input.IsPickupHeld())
        {
            pickable.OnPickupHeld(this, dt);
            return true;
        }

        return false;
    }

    private IPhysicalEntity GetTargetedEntity()
    {
        float reachInMeters = InteractDistancePixels.ToMeters();
        Vector2 startPoint = PhysicsBody.Position;
        Vector2 targetPoint = startPoint + (LookDirection * reachInMeters);

        IPhysicalEntity closestEntity = null;
        PhysicsBody.World.RayCast((fixture, point, normal, fraction) =>
        {
            if (fixture.Body == PhysicsBody) return -1;
            if (fixture.Body.Tag is IPhysicalEntity physicalEntity)
            {
                closestEntity = physicalEntity;
                return fraction;
            }

            return -1;
        }, startPoint, targetPoint);

        return closestEntity;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Texture2D texture = AssetManager.PlayerTexture;
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Texture2D texture = AssetManager.GetPlayerTexture($"{idleFrames[(currentFrame+PlayerConfiguration.PlayerIndex) % idleFrames.Length]}{PlayerConfiguration.PlayerIndex}");
        // float scale = tileSize / texture.Width;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        Color drawColor = IsStunned ? Color.Goldenrod : Color.White;

        bool IsFacingRight = LookDirection.X > 0;
        SpriteEffects flipEffect = IsFacingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        spriteBatch.Draw(texture, Position + new Vector2(0, -tileSize * 0.75f), null, drawColor, 0f, origin, 0.3f, flipEffect,
            0f);
        DrawInteractionTarget(spriteBatch);
        DrawHeldItem(spriteBatch);
        DrawStunProgress(spriteBatch);
    }

    private void DrawHeldItem(SpriteBatch spriteBatch)
    {
        if (HeldItem != null)
        {
            Vector2 itemOffset = LookDirection * (Radius * HeldItemOffsetRadiusMultiplier);
            Vector2 itemPosition = Position + itemOffset;
            int itemSize = (int)(Radius * HeldItemSizeRadiusMultiplier);

            HeldItem.Draw(spriteBatch, itemPosition, itemSize);
        }
    }

    private void DrawInteractionTarget(SpriteBatch spriteBatch)
    {
        Vector2 targetPointPixels = Position + (LookDirection * InteractDistancePixels);
        spriteBatch.DrawCircle(targetPointPixels, 5f, 12, Color.Red, 2f);
    }

    private void DrawStunProgress(SpriteBatch spriteBatch)
    {
        if (!IsStunned)
        {
            return;
        }

        int barWidth = 42;
        int barHeight = 6;
        Vector2 anchor = Position + new Vector2(-barWidth / 2f, -(Radius + 18f));
        var bg = new Rectangle((int)anchor.X, (int)anchor.Y, barWidth, barHeight);
        var fill = new Rectangle(bg.X, bg.Y, (int)(barWidth * Math.Clamp(ReviveProgress, 0f, 1f)), barHeight);
        spriteBatch.Draw(AssetManager.BlankTexture, bg, Color.Black);
        spriteBatch.Draw(AssetManager.BlankTexture, fill, Color.LimeGreen);
    }
}
