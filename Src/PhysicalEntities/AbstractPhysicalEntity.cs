using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities;

public abstract class AbstractPhysicalEntity : IPhysicalEntity, IHighlightable
{
    protected readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private Body physicsBody;

    public Body PhysicsBody
    {
        get => physicsBody;
        set
        {
            physicsBody = value;
            if (physicsBody != null)
            {
                physicsBody.Tag = this;
            }
        }
    }

    public bool IsHighlighted => highlighterCount > 0;
    private int highlighterCount;

    public Vector2 Position
    {
        get => PhysicsBody.Position.ToPixels();
        set => PhysicsBody.Position = value.ToMeters();
    }

    public abstract void Draw(SpriteBatch spriteBatch);

    public virtual void OnHighlight(Player player)
    {
        highlighterCount++;
    }

    public virtual void OnHighlightRemoved(Player player)
    {
        highlighterCount--;
        if (highlighterCount < 0) highlighterCount = 0;
    }
}