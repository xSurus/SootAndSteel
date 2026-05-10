using System;
using FmodForFoxes.Studio;
using Gamelab.Assets;
using Gamelab.Enemies.Core;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Animation;
using Gamelab.Services.Sound;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

public class Player : AbstractPhysicalEntity, IInteractable, IDamageable, IItemProvider, IItemReceiver
{
    public PlayerConfiguration PlayerConfiguration { get; private set; }
    public Item HeldItem { get; set; }
    public IGrabbable GrabbedObject { get; private set; }
    public ICannonSeat SeatedAt { get; private set; }
    public PlayerCondition Condition { get; private set; } = PlayerCondition.Active;
    public bool IsStunned => Condition == PlayerCondition.Stunned;
    public float ReviveProgress { get; private set; }

    public Vector2 LookDirection => new((float)Math.Cos(PhysicsBody.Rotation), (float)Math.Sin(PhysicsBody.Rotation));
    private IHighlightable highlightedEntity;

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

    private float stunTimer;
    private float stunAnimTimer;
    private float patchRemoveTimer;
    private bool revivedThisFrame;

    private AnimatedSprite playerSprite;
    private IAnimationService animationService;
    private ISoundService soundService = GamelabGame.Instance.Services.GetService<ISoundService>();

    private EventInstance walkSound;

    public Player(Vector2 startPosition, PlayerConfiguration playerConfig)
    {
        PlayerConfiguration = playerConfig;
        PhysicsBody = gameplayContext.PhysicsWorld.CreateCircle(Radius.ToMeters(), Density, startPosition.ToMeters(),
            BodyType.Dynamic);
        PhysicsBody.LinearDamping = LinearDampening;
        PhysicsBody.FixedRotation = true;
        PhysicsBody.Tag = this;

        soundService.LoadSound(Sounds.Walk);
        walkSound = soundService.GetSoundInstance(Sounds.Walk);
        soundService.RegisterParameter(walkSound, "Walk Speed",
            () => PhysicsBody.LinearVelocity.Length() / MaxVelocity);
        soundService.RegisterParameter(walkSound, "Material",
            () => PlayerIsInTrain() ? (int)WalkMaterial.Wood : (int)WalkMaterial.Snow);
        walkSound?.Start();

        animationService = GamelabGame.Instance.Services.GetService<IAnimationService>();
        playerSprite = new AnimatedSprite(AssetManager.PlayerSpriteSheet);
        playerSprite.SetAnimation($"Player{PlayerConfiguration.PlayerIndex}_Idle");
        animationService.Register(playerSprite);
    }

    public void Update(float dt)
    {
        if (IsStunned)
        {
            UpdateStunned(dt);
            return;
        }

        if (SeatedAt != null)
        {
            UpdateSeated();
            return;
        }

        Vector2 movement = PlayerConfiguration.Input.GetMovement();

        var patches = gameplayContext?.PatchManager;
        bool onSnow = patches?.IsOnSnow(Position, gameplayContext.Map) ?? false;
        bool onIce = patches?.IsOnIce(Position, gameplayContext.Map) ?? false;

        PhysicsBody.LinearDamping = onIce
            ? GamelabGame.Instance.GameplayConfig.IceLinearDamping
            : LinearDampening;

        float speedScale = onSnow ? GamelabGame.Instance.GameplayConfig.SnowSpeedFactor : 1f;

        if (GrabbedObject == null)
        {
            if (movement != Vector2.Zero) PhysicsBody.Rotation = (float)Math.Atan2(movement.Y, movement.X);

            if (onIce)
            {
                // Force-based: input accelerates gradually and momentum is preserved by low damping.
                PhysicsBody.ApplyForce(movement * GamelabGame.Instance.GameplayConfig.IceForce);
                float maxSpeed = MaxVelocity;
                float speedSq = PhysicsBody.LinearVelocity.LengthSquared();
                if (speedSq > maxSpeed * maxSpeed)
                    PhysicsBody.LinearVelocity = Vector2.Normalize(PhysicsBody.LinearVelocity) * maxSpeed;
            }
            else
            {
                Vector2 targetVelocity = movement * MaxVelocity * speedScale;
                PhysicsBody.LinearVelocity = Vector2.Lerp(PhysicsBody.LinearVelocity, targetVelocity, LerpFactor);
            }
        }
        else if (movement != Vector2.Zero)
        {
            if (onIce)
            {
                PhysicsBody.ApplyForce(movement * GamelabGame.Instance.GameplayConfig.IceForce);
                float maxSpeed = MaxVelocity;
                float speedSq = PhysicsBody.LinearVelocity.LengthSquared();
                if (speedSq > maxSpeed * maxSpeed)
                    PhysicsBody.LinearVelocity = Vector2.Normalize(PhysicsBody.LinearVelocity) * maxSpeed;
            }
            else
            {
                PhysicsBody.ApplyForce(movement * 100f * speedScale);
            }
        }

        if (onIce && PhysicsBody.LinearVelocity.LengthSquared() > 0.5f)
        {
            float tripChance = GamelabGame.Instance.GameplayConfig.IceTripChancePerSecond * dt;
            if (Random.Shared.NextDouble() < tripChance)
            {
                Stun(0);
                return;
            }
        }

        UpdateHighlightedEntity();
        if (TryGrab()) return;
        if (TryPickup(dt)) return;
        if (TryRemovePatch(dt)) return;
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
        stunAnimTimer = 0f;
        ReviveProgress = 0f;
        revivedThisFrame = false;
        PhysicsBody.LinearVelocity = Vector2.Zero;

        if (GrabbedObject != null)
        {
            GrabbedObject.OnRelease(this);
            GrabbedObject = null;
        }

        var seat = SeatedAt;
        SeatedAt = null;
        seat?.OnRelease(this);

        HeldItem = null;
    }

    public void SeatAt(ICannonSeat seat)
    {
        SeatedAt?.OnRelease(this);
        SeatedAt = seat;
        PhysicsBody.LinearVelocity = Vector2.Zero;
        PhysicsBody.Position = seat.PhysicsBody.Position;
        PhysicsBody.BodyType = BodyType.Static;
    }

    public void UnseatFrom(ICannonSeat seat)
    {
        if (SeatedAt != null && SeatedAt != seat) return;
        SeatedAt = null;
        if (GrabbedObject is not null && ReferenceEquals(GrabbedObject, seat)) GrabbedObject = null;
        PhysicsBody.BodyType = BodyType.Dynamic;
        PhysicsBody.LinearVelocity = Vector2.Zero;
    }

    private void UpdateSeated()
    {
        PhysicsBody.Position = SeatedAt.PhysicsBody.Position;
        PhysicsBody.LinearVelocity = Vector2.Zero;
        PhysicsBody.Rotation = SeatedAt.PhysicsBody.Rotation;

        var input = PlayerConfiguration.Input;

        if (input.IsGrabJustPressed())
        {
            SeatedAt.OnRelease(this);
            return;
        }

        if (input.IsInteractJustPressed())
        {
            SeatedAt.OnInteract(this);
        }

        if (input.IsPickupJustPressed())
        {
            SeatedAt.OnPickup(this);
        }
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
        if (bullet.InitialShooter is not AbstractEnemy || IsStunned)
        {
            return false;
        }

        TakeDamage(bullet.Stats.Damage);
        return true;
    }

    private void UpdateStunned(float dt)
    {
        PhysicsBody.LinearVelocity = Vector2.Zero;
        stunAnimTimer += dt;

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
            if (highlightedEntity is not IGrabbable grabbable) return false;
            float reachInMeters = InteractDistancePixels.ToMeters();
            Vector2 grabPointWorldMeters = PhysicsBody.Position + (LookDirection * reachInMeters);
            if (grabbable.OnGrab(this, grabPointWorldMeters))
            {
                GrabbedObject = grabbable;
                return true;
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
        if (highlightedEntity is not IInteractable interactable) return false;

        bool interactJust = PlayerConfiguration.Input.IsInteractJustPressed();
        bool interactHeld = PlayerConfiguration.Input.IsInteractHeld();
        bool interactReleased = PlayerConfiguration.Input.IsInteractJustReleased();

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

        if (interactReleased)
        {
            interactable.OnInteractReleased(this);
        }

        return false;
    }

    private bool TryPickup(float dt)
    {
        if (highlightedEntity == null || highlightedEntity is not IPickable pickable) return false;
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

    private bool TryRemovePatch(float dt)
    {
        var patches = gameplayContext?.PatchManager;
        bool onPatch = patches != null &&
                       (patches.IsOnSnow(Position, gameplayContext.Map) ||
                        patches.IsOnIce(Position, gameplayContext.Map));

        if (!onPatch)
        {
            patchRemoveTimer = 0f;
            return false;
        }

        if (highlightedEntity is IInteractable interactable)
        {
            Point playerTile = gameplayContext.Map.GetTileIndexFromPixels(Position);
            Point interactableTile = gameplayContext.Map.GetTileIndexFromPixels(interactable.Position);
            if (interactableTile == playerTile)
            {
                patchRemoveTimer = 0f;
                return false;
            }
        }

        if (!PlayerConfiguration.Input.IsInteractHeld())
        {
            patchRemoveTimer = 0f;
            return false;
        }

        patchRemoveTimer += dt;
        if (patchRemoveTimer < GamelabGame.Instance.GameplayConfig.PatchRemoveDurationSeconds)
            return false;

        patchRemoveTimer = 0f;
        return patches.TryRemovePatchAt(Position, gameplayContext.Map);
    }

    private void UpdateHighlightedEntity()
    {
        float reachInMeters = InteractDistancePixels.ToMeters();
        Vector2 startPoint = PhysicsBody.Position;
        Vector2 targetPoint = startPoint + (LookDirection * reachInMeters);

        IHighlightable closestEntity = null;
        PhysicsBody.World.RayCast((fixture, point, normal, fraction) =>
        {
            if (fixture.Body == PhysicsBody) return -1;
            if (fixture.Body.Tag is IHighlightable highlightableEntity)
            {
                if (highlightableEntity is AbstractPhysicalEntity physicalEntity && !physicalEntity.CanHighlight)
                    return -1;

                closestEntity = highlightableEntity;
                return fraction;
            }

            return -1;
        }, startPoint, targetPoint);

        if (highlightedEntity != closestEntity)
        {
            highlightedEntity?.OnHighlightRemoved(this);
            closestEntity?.OnHighlight(this);
        }

        highlightedEntity = closestEntity;
    }

    public Item PeekNextItem() => HeldItem;

    public bool CanProvideItem(IItemReceiver consumer)
    {
        return true;
    }

    public virtual bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        item = HeldItem;
        if (HeldItem != null && CanProvideItem(consumer))
        {
            HeldItem = null;
            soundService.PlayOnce(ItemIsGranular(item) ? Sounds.ShovelDown : Sounds.DropItem);
            return true;
        }

        item = null;
        return false;
    }

    public bool CanReceiveItem(Item item, IItemProvider source) => HeldItem == null;

    public void ReceiveItem(Item item, IItemProvider source)
    {
        HeldItem = item;
    }

    private void DrawSeated(SpriteBatch spriteBatch)
    {
        int idx = PlayerConfiguration.PlayerIndex;
        Texture2D topTex = AssetManager.PlayerCannonTopTextures[idx];
        Texture2D bottomTex = AssetManager.PlayerCannonBottomTextures[idx];
        Vector2 drawPos = Position + SeatedAt.DrawOffset;
        float depth = RenderUtility.CalculateDepth(drawPos.Y);
        const float scale = 0.3f;
        Vector2 topOrigin = new Vector2(topTex.Width / 2f, topTex.Height / 2f);
        Vector2 bottomOrigin = new Vector2(bottomTex.Width / 2f, bottomTex.Height / 2f);
        spriteBatch.Draw(bottomTex, drawPos, null, Color.White, 0f, bottomOrigin, scale, SpriteEffects.None, depth - RenderUtility.Eps);
        spriteBatch.Draw(topTex, drawPos, null, Color.White, 0f, topOrigin, scale, SpriteEffects.None, depth + RenderUtility.Eps);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (playerSprite == null) return;

        if (SeatedAt != null)
        {
            DrawSeated(spriteBatch);
            return;
        }

        Vector2 feetPosition = Position + new Vector2(0, Radius);
        Color drawColor = IsStunned ? Color.Goldenrod : Color.White;
        bool isFacingRight = LookDirection.X > 0;
        SpriteEffects flipEffect = isFacingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        float renderDepth = RenderUtility.CalculateDepth(feetPosition.Y);

        playerSprite.Color = drawColor;
        playerSprite.Depth = renderDepth;
        playerSprite.Effect = flipEffect;

        if (IsStunned)
        {
            playerSprite.Origin =
                new Vector2(playerSprite.TextureRegion.Width / 2f, playerSprite.TextureRegion.Height / 2f);
            spriteBatch.Draw(playerSprite, Position, MathHelper.PiOver2, new Vector2(0.3f));
        }
        else
        {
            playerSprite.Origin = new Vector2(playerSprite.TextureRegion.Width / 2f, playerSprite.TextureRegion.Height);
            spriteBatch.Draw(playerSprite, feetPosition, 0f, new Vector2(0.3f));
        }

        DrawHeldItem(spriteBatch, renderDepth + RenderUtility.Eps);

        float playerVisualHeight = playerSprite.TextureRegion.Height * 0.3f;
        DrawConcussionStars(spriteBatch, playerVisualHeight);
        DrawStunProgress(spriteBatch, playerVisualHeight);
    }

    private void DrawHeldItem(SpriteBatch spriteBatch, float renderDepth)
    {
        if (HeldItem != null)
        {
            Vector2 itemOffset = LookDirection * (Radius * HeldItemOffsetRadiusMultiplier);
            Vector2 itemPosition = Position + itemOffset;
            int itemSize = (int)(Radius * HeldItemSizeRadiusMultiplier);
            HeldItem.Draw(spriteBatch, itemPosition, itemSize, renderDepth);
        }
    }

    private void DrawConcussionStars(SpriteBatch spriteBatch, float playerVisualHeight)
    {
        if (!IsStunned) return;

        const int starCount = 5;
        const float orbitRadius = 16f;
        const float starScale = 7f;

        // Orbit center sits just above the player's head
        Vector2 center = Position + new Vector2(0, -(playerVisualHeight + orbitRadius + 4f));
        float baseAngle = stunAnimTimer * 3f;

        for (int i = 0; i < starCount; i++)
        {
            float angle = baseAngle + MathHelper.TwoPi / starCount * i;
            Vector2 starPos = center + new Vector2(
                (float)Math.Cos(angle) * orbitRadius,
                (float)Math.Sin(angle) * orbitRadius * 0.4f // flatten into an ellipse
            );
            Color color = i % 2 == 0 ? Color.Yellow : Color.Gold;
            spriteBatch.Draw(
                AssetManager.BlankTexture,
                starPos,
                null,
                color,
                angle, // each star rotates with its orbit angle
                new Vector2(0.5f, 0.5f),
                starScale,
                SpriteEffects.None,
                RenderUtility.OverlayTopLayer
            );
        }
    }

    private void DrawStunProgress(SpriteBatch spriteBatch, float playerVisualHeight)
    {
        if (!IsStunned)
        {
            return;
        }

        int barWidth = 42;
        int barHeight = 6;
        Vector2 feetPosition = Position + new Vector2(0, Radius);
        Vector2 anchor = feetPosition + new Vector2(-barWidth / 2f, -playerVisualHeight - 15f);

        var bg = new Rectangle((int)anchor.X, (int)anchor.Y, barWidth, barHeight);
        var fill = new Rectangle(bg.X, bg.Y, (int)(barWidth * Math.Clamp(ReviveProgress, 0f, 1f)), barHeight);
        spriteBatch.Draw(AssetManager.BlankTexture, bg, null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None,
            RenderUtility.OverlayBackLayer);
        spriteBatch.Draw(AssetManager.BlankTexture, fill, null, Color.LimeGreen, 0f, Vector2.Zero, SpriteEffects.None,
            RenderUtility.OverlayTopLayer);
    }

    protected bool ItemIsGranular(Item item)
    {
        return (item is BulletItem && ((BulletItem) item).Type == EComponentType.Propellant) || item.Id == "Coal";
    }

    protected bool PlayerIsInTrain()
    {
        return gameplayContext.Map.GetBounds().Contains(Position);
    }
    
    public void Dispose()
    {
        animationService?.Unregister(playerSprite);
        if (walkSound != null)
        {
            walkSound.Stop();
            walkSound.Dispose();
            walkSound = null;
        }
    }
}