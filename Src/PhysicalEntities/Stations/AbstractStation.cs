using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public abstract class AbstractStation : AbstractGrabbable, IInteractable, IPickable
{
    public string Type { get; protected set; }

    // TODO swap to a texture instead of display color at some point
    public Color DisplayColor { get; protected set; }
    public Item HeldItem { get; set; }

    protected override bool AllowPlayerRotation { get; } = false;

    protected AbstractStation(string type, Color displayColor, Vector2 position, TrainContext trainContext)
    {
        Type = type;
        DisplayColor = displayColor;
        float collisionSizePixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.90f;
        float simSize = collisionSizePixels.ToMeters();
        PhysicsBody = trainContext.Map.PhysicsWorld.CreateRectangle(simSize, simSize, 1f, position.ToMeters());
    }

    public virtual void Update(float dt, TrainContext trainContext)
    {
    }

    public virtual void OnInteract(Player interactingPlayer, TrainContext context)
    {
    }

    public virtual void OnInteractHeld(Player interactingPlayer, TrainContext context, float dt)
    {
    }

    public virtual void OnPickup(Player interactingPlayer, TrainContext trainContext)
    {
    }

    public virtual void OnPickupHeld(Player interactingPlayer, TrainContext trainContext, float dt)
    {
    }

    protected override void OnLastRelease(Player interactingPlayer, TrainContext trainContext)
    {
        base.OnLastRelease(interactingPlayer, trainContext);
        trainContext.Map.SnapToNearestValidCell(this);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        var position = PhysicsBody.Position.ToPixels() - new Vector2(tileSize / 2f);
        Rectangle rect = new Rectangle((int)position.X + 5, (int)position.Y + 5, tileSize - 10, tileSize - 10);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, DisplayColor);
        HeldItem?.Draw(spriteBatch, position + new Vector2(tileSize / 4), tileSize / 2);
    }
}