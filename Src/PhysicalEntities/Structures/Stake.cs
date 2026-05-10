using System;
using Gamelab.Assets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Structures;

public class Stake : AbstractPhysicalEntity
{
    public override bool CanHighlight => false;

    private const float CollisionWidthPixels = 45f;
    private const float CollisionHeightPixels = 80f;
    private const float VisualWidthPixels = 50f;
    private const float BuriedTipFraction = 0.13f;

    private readonly float visualScale;
    private readonly Rectangle visibleSource;

    public Stake(Vector2 feetPositionPixels)
    {
        Texture2D tex = AssetManager.GetHubDecorationTexture("Stake");
        visualScale = VisualWidthPixels / tex.Width;
        int visibleHeight = (int)MathF.Round(tex.Height * (1f - BuriedTipFraction));
        visibleSource = new Rectangle(0, 0, tex.Width, visibleHeight);

        Vector2 bodyCenterPixels = feetPositionPixels - new Vector2(0, CollisionHeightPixels / 2f);
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(
            CollisionWidthPixels.ToMeters(),
            CollisionHeightPixels.ToMeters(),
            1f,
            bodyCenterPixels.ToMeters(),
            0f,
            BodyType.Static);
    }

    public void RemovePhysicsBody()
    {
        if (PhysicsBody == null) return;
        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = AssetManager.GetHubDecorationTexture("Stake");
        Vector2 origin = new Vector2(visibleSource.Width / 2f, visibleSource.Height);
        Vector2 feet = Position + new Vector2(0, CollisionHeightPixels / 2f);
        float depth = RenderUtility.CalculateWorldObjectDepth(feet.Y);

        spriteBatch.Draw(
            texture: tex,
            position: feet,
            sourceRectangle: visibleSource,
            color: Color.White,
            rotation: 0f,
            origin: origin,
            scale: visualScale,
            effects: SpriteEffects.None,
            layerDepth: depth);
    }
}
