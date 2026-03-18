using System;
using Gamelab.Assets;
using Gamelab.Input;
using Gamelab.Items;
using Gamelab.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

public class Player(int playerIndex, Body body, IInputProvider input, TrainMap map)
{
    public int PlayerIndex { get; set; } = playerIndex;
    public IInputProvider Input { get; private set; } = input;
    public Body Body { get; private set; } = body;
    public const float PixelsPerMeter = 100f;
    private const float MaxVelocity = 5f;
    public float Radius { get; private set; } = 24f;
    public Vector2 Position => Body.Position * PixelsPerMeter;
    public Item HeldItem { get; set; }
    private TrainMap trainMap = map;
    public Vector2 LookDirection => new Vector2((float)Math.Cos(Body.Rotation), (float)Math.Sin(Body.Rotation));

    private const float LerpFactor = 0.8f;

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
        Vector2 targetPoint = pixelPosition + (LookDirection * 40f);
        Rectangle trainBounds = trainMap.GetBounds();
        if (trainBounds.Contains(targetPoint))
        {
            int gridX = (int)((targetPoint.X - trainBounds.X) / trainMap.TileSize);
            int gridY = (int)((targetPoint.Y - trainBounds.Y) / trainMap.TileSize);
            TileCell targetCell = trainMap.GetTile(gridX, gridY);
            targetCell?.AbstractStation?.Interact(this);
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
            Vector2 itemOffset = LookDirection * (Radius * 1.5f);
            Vector2 itemPosition = Position + itemOffset;
            int itemSize = (int)(Radius * 0.8f);

            HeldItem.Draw(spriteBatch, itemPosition - new Vector2(itemSize / 2f), itemSize);
        }
    }

    private void DrawVisionCone(SpriteBatch spriteBatch)
    {
        float coneLength = Radius * 2.5f;
        float coneAngle = MathHelper.ToRadians(30);

        Vector2 leftSide = Vector2.Transform(LookDirection, Matrix.CreateRotationZ(-coneAngle)) * coneLength;
        Vector2 rightSide = Vector2.Transform(LookDirection, Matrix.CreateRotationZ(coneAngle)) * coneLength;

        spriteBatch.DrawLine(Position, Position + leftSide, Color.Red, 2f);
        spriteBatch.DrawLine(Position, Position + rightSide, Color.Red, 2f);
        spriteBatch.DrawLine(Position + leftSide, Position + rightSide, Color.Red, 2f);
    }
}