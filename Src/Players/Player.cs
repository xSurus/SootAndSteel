using System;
using Gamelab.Assets;
using Gamelab.Input;
using Gamelab.Items;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

public class Player
{
    public IInputProvider Input { get; private set; }
    public Body Body { get; private set; }
    public Vector2 Position => Body.Position * PixelsPerMeter;
    public Item HeldItem { get; set; }

    private readonly TrainContext trainContext;
    public Vector2 LookDirection => new((float)Math.Cos(Body.Rotation), (float)Math.Sin(Body.Rotation));

    private float LerpFactor => GamelabGame.Instance.GameplayConfig.PlayerVelocityLerpFactor;
    private float PixelsPerMeter => GamelabGame.Instance.GameplayConfig.PixelsPerMeter;
    private float MaxVelocity => GamelabGame.Instance.GameplayConfig.PlayerMaxVelocity;
    private float InteractDistancePixels => GamelabGame.Instance.GameplayConfig.PlayerInteractDistancePixels;

    private float HeldItemOffsetRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerHeldItemOffsetRadiusMultiplier;

    private float HeldItemSizeRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerHeldItemSizeRadiusMultiplier;

    private float VisionConeLengthRadiusMultiplier =>
        GamelabGame.Instance.GameplayConfig.PlayerVisionConeLengthRadiusMultiplier;

    private float VisionConeAngleRadians =>
        MathHelper.ToRadians(GamelabGame.Instance.GameplayConfig.PlayerVisionConeAngleDegrees);

    private float Radius => GamelabGame.Instance.GameplayConfig.PlayerRadiusPixels;


    public Player(Body body, IInputProvider input, TrainContext context)
    {
        trainContext = context;
        Body = body;
        Input = input;
    }


    public void Update()
    {
        Vector2 movement = Input.GetMovement();

        if (movement != Vector2.Zero)
        {
            Body.Rotation = (float)Math.Atan2(movement.Y, movement.X);
        }

        Vector2 targetVelocity = movement * MaxVelocity;
        Body.LinearVelocity = Vector2.Lerp(Body.LinearVelocity, targetVelocity, LerpFactor);

        if (Input.IsActionJustPressed())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Vector2 pixelPosition = Body.Position * PixelsPerMeter;
        Vector2 targetPoint = pixelPosition + (LookDirection * InteractDistancePixels);
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
            Vector2 itemOffset = LookDirection * (Radius * HeldItemOffsetRadiusMultiplier);
            Vector2 itemPosition = Position + itemOffset;
            int itemSize = (int)(Radius * HeldItemSizeRadiusMultiplier);

            HeldItem.Draw(spriteBatch, itemPosition - new Vector2(itemSize / 2f), itemSize);
        }
    }

    private void DrawVisionCone(SpriteBatch spriteBatch)
    {
        float coneLength = Radius * VisionConeLengthRadiusMultiplier;

        Vector2 leftSide = Vector2.Transform(LookDirection, Matrix.CreateRotationZ(-VisionConeAngleRadians)) *
                           coneLength;
        Vector2 rightSide = Vector2.Transform(LookDirection, Matrix.CreateRotationZ(VisionConeAngleRadians)) *
                            coneLength;

        spriteBatch.DrawLine(Position, Position + leftSide, Color.Red, 2f);
        spriteBatch.DrawLine(Position, Position + rightSide, Color.Red, 2f);
        spriteBatch.DrawLine(Position + leftSide, Position + rightSide, Color.Red, 2f);
    }
}