using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities;

public interface IPhysicalEntity
{
    Body PhysicsBody { get; }
    void Draw(SpriteBatch spriteBatch);
}