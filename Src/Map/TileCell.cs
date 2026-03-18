using Gamelab.Stations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map;

public class TileCell(int x, int y, Vector2 worldPosition, World world, float pixelsPerMeter, int tileSize)
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public Vector2 WorldPosition { get; private set; } = worldPosition;
    public bool IsWalkable { get; private set; } = true;
    public AbstractStation AbstractStation { get; private set; }

    private Body collisionBody;

    public void SetObject(AbstractStation obj)
    {
        AbstractStation = obj;
        IsWalkable = obj == null || !obj.IsSolid;
        UpdatePhysicsBody();
    }

    public void ClearObject()
    {
        AbstractStation = null;
        IsWalkable = true;
        UpdatePhysicsBody();
    }

    public void UpdateWorldPosition(Vector2 newPosition)
    {
        WorldPosition = newPosition;
        if (collisionBody != null)
        {
            Vector2 simPos = (WorldPosition + new Vector2(tileSize / 2f)) / pixelsPerMeter;
            collisionBody.Position = simPos;
        }
    }

    private void UpdatePhysicsBody()
    {
        if (!IsWalkable && collisionBody == null)
        {
            Vector2 simPos = (WorldPosition + new Vector2(tileSize / 2f)) / pixelsPerMeter;
            float simSize = tileSize / pixelsPerMeter;

            collisionBody = world.CreateRectangle(simSize, simSize, 1f, simPos);
        }
        else if (IsWalkable && collisionBody != null)
        {
            world.Remove(collisionBody);
            collisionBody = null;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D tileTexture)
    {
        // Background
        spriteBatch.Draw(tileTexture, WorldPosition, null, Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        // Tile
        AbstractStation?.Draw(spriteBatch, WorldPosition, tileSize);
    }
}