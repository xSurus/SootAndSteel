using Gamelab.Interactable.Stations;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map.Train;

public class TileCell(int x, int y, Vector2 worldPosition, World world, int tileSize)
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public Vector2 WorldPosition { get; private set; } = worldPosition;
    public bool IsWalkable { get; private set; } = true;
    public AbstractStation AbstractStation { get; private set; }

    public void SetObject(AbstractStation obj)
    {
        AbstractStation = obj;
        IsWalkable = obj == null || !obj.IsSolid;

        if (AbstractStation != null)
        {
            Vector2 simPos = (WorldPosition + new Vector2(tileSize / 2f)).ToMeters();
            float simSize = tileSize.ToMeters();
            Body stationBody = world.CreateRectangle(simSize, simSize, 1f, simPos);
            AbstractStation.AttachPhysics(stationBody);
        }
    }

    public void ClearObject()
    {
        if (AbstractStation?.PhysicsBody != null)
        {
            world.Remove(AbstractStation.PhysicsBody);
            AbstractStation.DetachPhysics();
        }

        AbstractStation = null;
        IsWalkable = true;
    }

    public void UpdateWorldPosition(Vector2 newPosition)
    {
        WorldPosition = newPosition;
        if (AbstractStation?.PhysicsBody != null)
        {
            AbstractStation.PhysicsBody.Position = (WorldPosition + new Vector2(tileSize / 2f)).ToMeters();
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D tileTexture)
    {
        spriteBatch.Draw(tileTexture, WorldPosition, null, Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        AbstractStation?.Draw(spriteBatch, WorldPosition, tileSize);
    }
}