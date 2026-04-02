using System;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

public class Player : AbstractPhysicalEntity
{
    public PlayerConfiguration PlayerConfiguration { get; private set; }
    public Item HeldItem { get; set; }
    public IGrabbable GrabbedObject { get; private set; }

    public Vector2 LookDirection => new((float)Math.Cos(PhysicsBody.Rotation), (float)Math.Sin(PhysicsBody.Rotation));

    private float LerpFactor => GamelabGame.Instance.GameplayConfig.PlayerVelocityLerpFactor;
    private float MaxVelocity => GamelabGame.Instance.GameplayConfig.PlayerMaxVelocity;
    private float InteractDistancePixels => GamelabGame.Instance.GameplayConfig.PlayerInteractDistancePixels;

    private float HeldItemOffsetRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerHeldItemOffsetRadiusMultiplier;

    private float HeldItemSizeRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerHeldItemSizeRadiusMultiplier;

    private float Radius => GamelabGame.Instance.GameplayConfig.PlayerRadiusPixels;
    private float Density => GamelabGame.Instance.GameplayConfig.PlayerDensity;
    private float LinearDampening => GamelabGame.Instance.GameplayConfig.PlayerLinearDamping;
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();


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
            // When carrying something, apply a scaled force so "movement speed" still feels slower.
            PhysicsBody.ApplyForce(movement * 100f * speedScale);
        }

        if (TryGrab()) return;
        if (TryInteract(dt)) return;
        TryPickup(dt);
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
        if (target == null || !(target is IInteractable interactable)) return false;
        if (PlayerConfiguration.Input.IsInteractJustPressed())
        {
            interactable.OnInteract(this);
            return true;
        }
        else if (PlayerConfiguration.Input.IsInteractHeld())
        {
            interactable.OnInteractHeld(this, dt);
            return true;
        }

        return false;
    }

    private bool TryPickup(float dt)
    {
        IPhysicalEntity target = GetTargetedEntity();
        if (target == null || !(target is IPickable pickable)) return false;
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
        Texture2D texture = AssetManager.PlayerTexture;
        float scale = (Radius * 2) / texture.Width;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        spriteBatch.Draw(texture, Position, null, Color.White, PhysicsBody.Rotation, origin, scale, SpriteEffects.None,
            0f);
        DrawInteractionTarget(spriteBatch);
        DrawHeldItem(spriteBatch);
    }

    private void DrawHeldItem(SpriteBatch spriteBatch)
    {
        if (HeldItem != null)
        {
            Vector2 itemOffset = LookDirection * (Radius * HeldItemOffsetRadiusMultiplier);
            Vector2 itemPosition = Position + itemOffset;
            int itemSize = (int)(Radius * HeldItemSizeRadiusMultiplier);

            HeldItem.Draw(spriteBatch, itemPosition - new Vector2(itemSize / 2f), itemSize);
        }
    }

    private void DrawInteractionTarget(SpriteBatch spriteBatch)
    {
        Vector2 targetPointPixels = Position + (LookDirection * InteractDistancePixels);
        spriteBatch.DrawCircle(targetPointPixels, 5f, 12, Color.Red, 2f);
    }
}