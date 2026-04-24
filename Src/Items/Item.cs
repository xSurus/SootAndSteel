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