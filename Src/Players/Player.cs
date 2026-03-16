using System;
using Gamelab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Players;

// REMOVED: controllerIndex from constructor as it's now inside the InputProvider
public class Player(int playerIndex, Body body, IInputProvider input)
{
    public int PlayerIndex { get; set; } = playerIndex;
    public IInputProvider Input { get; private set; } = input;
    public Body Body { get; private set; } = body;
    public const float PixelsPerMeter = 100f;
    private const float MaxVelocity = 5f;
    public float Radius { get; private set; } = 24f;
    public Vector2 Position => Body.Position * PixelsPerMeter;
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
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        float scale = (Radius * 2) / texture.Width;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        spriteBatch.Draw(texture, Position, null, Color.White, Body.Rotation, origin, scale, SpriteEffects.None, 0f);
        DrawVisionCone(spriteBatch);
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