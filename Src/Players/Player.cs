using System;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Input;
using Gamelab.Items;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

public class Player(
    int playerIndex,
    Body body,
    IInputProvider input,
    TrainContext context,
    GameplayConfig gameplayConfig)
{
    public IInputProvider Input { get; private set; } = input;
    public Body Body { get; private set; } = body;
    private readonly float pixelsPerMeter = gameplayConfig.PixelsPerMeter;
    private readonly float maxVelocity = gameplayConfig.PlayerMaxVelocity;
    private readonly float interactDistancePixels = gameplayConfig.PlayerInteractDistancePixels;
    private readonly float heldItemOffsetRadiusMultiplier = gameplayConfig.PlayerHeldItemOffsetRadiusMultiplier;
    private readonly float heldItemSizeRadiusMultiplier = gameplayConfig.PlayerHeldItemSizeRadiusMultiplier;
    private readonly float visionConeLengthRadiusMultiplier = gameplayConfig.PlayerVisionConeLengthRadiusMultiplier;
    private readonly float visionConeAngleRadians = MathHelper.ToRadians(gameplayConfig.PlayerVisionConeAngleDegrees);
    public float Radius { get; private set; } = gameplayConfig.PlayerRadiusPixels;
    public Vector2 Position => Body.Position * pixelsPerMeter;
    public Item HeldItem { get; set; }

    private TrainContext trainContext = context;
    public Vector2 LookDirection => new Vector2((float)Math.Cos(Body.Rotation), (float)Math.Sin(Body.Rotation));

    private readonly float lerpFactor = gameplayConfig.PlayerVelocityLerpFactor;

    public void Update()
    {
        Vector2 movement = Input.GetMovement();

        if (movement != Vector2.Zero)
        {
            Body.Rotation = (float)Math.Atan2(movement.Y, movement.X);
        }

        Vector2 targetVelocity = movement * maxVelocity;
        Body.LinearVelocity = Vector2.Lerp(Body.LinearVelocity, targetVelocity, lerpFactor);

        if (Input.IsActionJustPressed())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Vector2 pixelPosition = Body.Position * pixelsPerMeter;
        Vector2 targetPoint = pixelPosition + (LookDirection * interactDistancePixels);
        Rectangle trainBounds = trainContext.Map.GetBounds();
        if (trainBounds.Contains(targetPoint))
        {
            int gridX = (int)((targetPoint.X - trainBounds.X) / trainContext.Map.TileSize);
            int gridY = (int)((targetPoint.Y - trainBounds.Y) / trainContext.Map.TileSize);
            TileCell targetCell = trainContext.Map.GetTile(gridX, gridY);
            targetCell?.AbstractStation?.Interact(this, trainContext);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = AssetManager.PlayerTexture;
        float scale = (Radius * 2) / texture.Width;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        spriteBatch.Draw(texture, Position, null, Color.White, Body.Rotation, origin, scale, SpriteEffects.None, 0f);
        DrawVisionCone(spriteBatch);
        DrawHeldItem(spriteBatch);
    }

    private void DrawHeldItem(SpriteBatch spriteBatch)
    {
        if (HeldItem != null)
        {
            Vector2 itemOffset = LookDirection * (Radius * heldItemOffsetRadiusMultiplier);
            Vector2 itemPosition = Position + itemOffset;
            int itemSize = (int)(Radius * heldItemSizeRadiusMultiplier);

            HeldItem.Draw(spriteBatch, itemPosition - new Vector2(itemSize / 2f), itemSize);
        }
    }

    private void DrawVisionCone(SpriteBatch spriteBatch)
    {
        float coneLength = Radius * visionConeLengthRadiusMultiplier;

        Vector2 leftSide = Vector2.Transform(LookDirection, Matrix.CreateRotationZ(-visionConeAngleRadians)) *
                           coneLength;
        Vector2 rightSide = Vector2.Transform(LookDirection, Matrix.CreateRotationZ(visionConeAngleRadians)) *
                            coneLength;

        spriteBatch.DrawLine(Position, Position + leftSide, Color.Red, 2f);
        spriteBatch.DrawLine(Position, Position + rightSide, Color.Red, 2f);
        spriteBatch.DrawLine(Position + leftSide, Position + rightSide, Color.Red, 2f);
    }
}