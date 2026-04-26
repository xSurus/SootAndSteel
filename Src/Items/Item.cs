using Gamelab.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Items;

public class Item(string id)
{
    public string Id { get; } = id;
    public ItemDefinition Definition => ItemRegistry.Get(Id);

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, int size, float depth)
    {
        Texture2D tex = AssetManager.GetItemTexture(Id);

        if (tex != null)
        {
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            float scale = size / (float)tex.Width;

            spriteBatch.Draw(
                texture: tex,
                position: position,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: depth
            );
        }
        else
        {
            Vector2 origin = new Vector2(size / 2f, size / 2f);
            Rectangle sourceRect = new Rectangle(0, 0, size, size);

            spriteBatch.Draw(
                texture: AssetManager.BlankTexture,
                position: position,
                sourceRectangle: sourceRect,
                color: Definition.Color,
                rotation: 0f,
                origin: origin,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: depth
            );
        }
    }
}