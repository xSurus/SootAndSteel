using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface IPhysicalEntity
{
    Body PhysicsBody { get; }
    Vector2 Position { get; set; }
    void Draw(SpriteBatch spriteBatch);
}