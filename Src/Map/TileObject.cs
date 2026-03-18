using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Map;

public abstract class TileObject(string type, Color displayColor)
{
    public string Type { get; protected set; } = type;

    // TODO swap to a texture instead of display color at some point
    public Color DisplayColor { get; protected set; } = displayColor;
    public bool IsSolid { get; protected set; } = true;
    public Item HeldItem { get; set; }

    public virtual void Update(float deltaTime)
    {
        // by default, does nothing. Stations like coal oven need to continously update
    }

    public abstract void Interact(Player interactingPlayer);

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, int tileSize)
    {
        Rectangle rect = new Rectangle((int)position.X + 5, (int)position.Y + 5, tileSize - 10, tileSize - 10);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, DisplayColor);
        HeldItem?.Draw(spriteBatch, position + new Vector2(tileSize / 4), tileSize / 2);
    }
}