using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities;

public abstract class AbstractPhysicalEntity : IPhysicalEntity
{
    private Body physicsBody;

    public Body PhysicsBody
    {
        get => physicsBody;
        protected set
        {
            physicsBody = value;
            if (physicsBody != null)
            {
                physicsBody.Tag = this;
            }
        }
    }

    public abstract void Draw(SpriteBatch spriteBatch);
}