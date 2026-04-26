using Gamelab.Assets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Structures;

public class CoalWagon : AbstractPhysicalEntity
{
    private readonly float heightPixels;
    private readonly float widthPixels;

    private static readonly Logger logger = new("CoalWagon");

    public CoalWagon(Vector2 position)
    {
        heightPixels = GamelabGame.Instance.GameplayConfig.TrainTileSize *
                       GamelabGame.Instance.GameplayConfig.TrainHeight;
        widthPixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 6f;

        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(widthPixels.ToMeters(), heightPixels.ToMeters(), 1f,
            position.ToMeters());
        PhysicsBody.Tag = this;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = AssetManager.GetStructureTexture("CoalWagon2");
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
        Vector2 bottomCenter = Position + new Vector2(0, heightPixels / 2f);
        float depth = RenderUtility.CalculateDepth(bottomCenter.Y);

        spriteBatch.Draw(
            texture: tex,
            position: bottomCenter,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: origin,
            scale: 0.85f,
            effects: SpriteEffects.None,
            layerDepth: depth
        );
    }
}