using Gamelab.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Items;

public class Item(string id)
{
    public string Id { get; } = id;
    public ItemDefinition Definition => ItemRegistry.Get(Id);

    public void Draw(SpriteBatch spriteBatch, Vector2 position, int size)
    {
        Rectangle rect = new Rectangle((int)position.X, (int)position.Y, size, size);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, Definition.Color);
    }
}