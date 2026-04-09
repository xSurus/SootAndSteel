using Gamelab.Assets;
using Gamelab.Items;
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
    public Vector2 DrawPosition => Position - new Vector2(GamelabGame.Instance.GameplayConfig.TrainTileSize / 2f);
    protected override bool AllowPlayerRotation { get; } = false;

    protected AbstractStation(string type, Color displayColor, Vector2 position)
    {
        Type = type;
        DisplayColor = displayColor;
        float collisionSizePixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.90f;
        float simSize = collisionSizePixels.ToMeters();
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(simSize, simSize, 1f, position.ToMeters());
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void OnInteract(Player interactingPlayer)
    {
    }

    public virtual void OnInteractHeld(Player interactingPlayer, float dt)
    {
    }

    public virtual void OnPickup(Player interactingPlayer)
    {
    }

    public virtual void OnPickupHeld(Player interactingPlayer, float dt)
    {
    }

    protected override void OnLastRelease(Player interactingPlayer)
    {
        base.OnLastRelease(interactingPlayer);
        gameplayContext.Map?.SnapToNearestValidCell(this);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 position = DrawPosition;

        Rectangle rect = new Rectangle((int)position.X + 5, (int)position.Y + 5, tileSize - 10, tileSize - 10);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, DisplayColor);
        HeldItem?.Draw(spriteBatch, position + new Vector2(tileSize / 4f), tileSize / 2);
    }
}