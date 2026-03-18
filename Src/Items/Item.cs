using Gamelab.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Items;

public class Item(string id, Color debugColor)
{
    public string Id { get; private set; } = id;
    public Color DebugColor { get; private set; } = debugColor;

    // TODO Alle basic Items in einem JSON definieren? Items mit speziellen Features als eigene Klasse

    public void Draw(SpriteBatch spriteBatch, Vector2 position, int size)
    {
        // als test sind items bisher nur eine pixelbox, später ersetzen mit texture
        Rectangle rect = new Rectangle((int)position.X, (int)position.Y, size, size);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, DebugColor);
    }
}