using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Interactable.Stations;

public abstract class AbstractStation(string type, Color displayColor) : IInteractable
{
    public string Type { get; protected set; } = type;

    // TODO swap to a texture instead of display color at some point
    public Color DisplayColor { get; protected set; } = displayColor;
    public bool IsSolid { get; protected set; } = true;
    public Item HeldItem { get; set; }

    public Body PhysicsBody { get; protected set; }

    public void AttachPhysics(Body body)
    {
        PhysicsBody = body;
        PhysicsBody.Tag = this;

        if (!IsSolid)
        {
            foreach (var fixture in PhysicsBody.FixtureList)
            {
                fixture.IsSensor = true;
            }
        }
    }

    public void DetachPhysics()
    {
        PhysicsBody = null;
    }

    public virtual void Update(float dt, TrainContext trainContext)
    {
    }

    public virtual void OnInteractAction(Player player, TrainContext context)
    {
    }

    public virtual void OnInteractActionHeld(Player player, TrainContext context, float dt)
    {
    }

    public virtual void OnGrabAction(Player interactingPlayer, TrainContext trainContext)
    {
    }

    public virtual void OnGrabActionHeld(Player interactingPlayer, TrainContext trainContext, float dt)
    {
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, int tileSize)
    {
        Rectangle rect = new Rectangle((int)position.X + 5, (int)position.Y + 5, tileSize - 10, tileSize - 10);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, DisplayColor);
        HeldItem?.Draw(spriteBatch, position + new Vector2(tileSize / 4), tileSize / 2);
    }
}