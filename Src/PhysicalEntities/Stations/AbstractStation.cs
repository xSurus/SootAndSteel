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
    public Vector2 DrawPosition => Position - new Vector2(GamelabGame.Instance.GameplayConfig.TrainTileSize / 2f);
    protected override bool AllowPlayerRotation { get; } = false;

    protected AbstractStation(string type, Color displayColor, Vector2 position, GameplayContext gameplayContext)
    {
        Type = type;
        DisplayColor = displayColor;
        float collisionSizePixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.90f;
        float simSize = collisionSizePixels.ToMeters();
        PhysicsBody = gameplayContext.Map.PhysicsWorld.CreateRectangle(simSize, simSize, 1f, position.ToMeters());
    }

    public virtual void Update(float dt, GameplayContext gameplayContext)
    {
    }

    public virtual void OnInteract(Player interactingPlayer, GameplayContext context)
    {
    }

    public virtual void OnInteractHeld(Player interactingPlayer, GameplayContext context, float dt)
    {
    }

    public virtual void OnPickup(Player interactingPlayer, GameplayContext gameplayContext)
    {
    }

    public virtual void OnPickupHeld(Player interactingPlayer, GameplayContext gameplayContext, float dt)
    {
    }

    protected override void OnLastRelease(Player interactingPlayer, GameplayContext gameplayContext)
    {
        base.OnLastRelease(interactingPlayer, gameplayContext);
        gameplayContext.Map.SnapToNearestValidCell(this);
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