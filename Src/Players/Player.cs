using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Interactable;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Gamelab.Players;

public class Player
{
    public PlayerConfiguration PlayerConfiguration { get; private set; }
    public Body PhysicsBody { get; private set; }
    public Vector2 Position => PhysicsBody.Position.ToPixels();
    public Item HeldItem { get; set; }

    private HashSet<IInteractable> nearbyInteractables = [];

    private readonly TrainContext trainContext;
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


    public Player(World world, Vector2 startPosition, PlayerConfiguration playerConfig, TrainContext context)
    {
        PlayerConfiguration = playerConfig;
        trainContext = context;
        PhysicsBody = world.CreateCircle(Radius.ToMeters(), Density, startPosition.ToMeters(), BodyType.Dynamic);
        PhysicsBody.LinearDamping = LinearDampening;
        PhysicsBody.FixedRotation = true;
        PhysicsBody.Tag = this;
        Fixture sensorFixture = PhysicsBody.CreateCircle(InteractDistancePixels, 0f);
        sensorFixture.IsSensor = true;
        sensorFixture.OnCollision += HandleSensorCollision;
        sensorFixture.OnSeparation += HandleSensorSeparation;
    }

    private bool HandleSensorCollision(Fixture sender, Fixture other, Contact contact)
    {
        if (other.Body.Tag is IInteractable interactable)
        {
            nearbyInteractables.Add(interactable);
        }

        return true;
    }

    private void HandleSensorSeparation(Fixture sender, Fixture other, Contact contact)
    {
        if (other.Body.Tag is IInteractable interactable)
        {
            nearbyInteractables.Remove(interactable);
        }
    }

    public void Update(float dt)
    {
        Vector2 movement = PlayerConfiguration.Input.GetMovement();

        if (movement != Vector2.Zero)
        {
            PhysicsBody.Rotation = (float)Math.Atan2(movement.Y, movement.X);
        }

        Vector2 targetVelocity = movement * MaxVelocity;
        PhysicsBody.LinearVelocity = Vector2.Lerp(PhysicsBody.LinearVelocity, targetVelocity, LerpFactor);

        if (PlayerConfiguration.Input.IsActionJustPressed())
        {
            TryInteract();
        }
        else if (PlayerConfiguration.Input.IsRepairHeld())
        {
            TryHoldInteract(dt);
        }
    }

    public void TryInteract()
    {
        IInteractable target = GetTargetedInteractable();
        target?.Interact(this, trainContext);
    }

    public void TryHoldInteract(float dt)
    {
        IInteractable target = GetTargetedInteractable();
        target?.HoldInteract(this, trainContext, dt);
    }

    private IInteractable GetTargetedInteractable()
    {
        if (nearbyInteractables.Count == 0) return null;

        float reachInMeters = InteractDistancePixels.ToMeters();
        Vector2 targetPoint = PhysicsBody.Position + (LookDirection * reachInMeters);

        foreach (var interactable in nearbyInteractables)
        {
            foreach (var fixture in interactable.PhysicsBody.FixtureList)
            {
                if (fixture.TestPoint(ref targetPoint))
                {
                    return interactable;
                }
            }
        }

        return null;
    }

    public void Draw(SpriteBatch spriteBatch)
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