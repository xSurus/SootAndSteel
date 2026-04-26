using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public abstract class AbstractStation : AbstractGrabbable, IInteractable, IPickable, IUpdatable, ITooltipable
{
    private static readonly Logger logger = new("Station");
    protected StationConfig StationConfig => GamelabGame.Instance.StationRegistry.Get(StationId);

    public string StationId { get; protected set; }

    // TODO swap to a texture instead of display color at some point
    public Color DisplayColor { get; protected set; }
    public Item HeldItem { get; set; }
    public Vector2 DrawPosition => Position - new Vector2(GamelabGame.Instance.GameplayConfig.TrainTileSize / 2f);
    protected override bool AllowPlayerRotation { get; } = false;
    public virtual bool IsVisible => IsHighlighted && StationConfig != null;

    public virtual string GetTitle()
    {
        return StationConfig?.Name ?? StationId;
    }

    public virtual string GetDescription()
    {
        return StationConfig?.Description ?? "";
    }

    public virtual string CategoryName =>
        StationConfig?.ItemType is null ? null : ShopItemIconAtlas.StationCategoryName;

    public virtual string FunctionalityName =>
        StationConfig?.ItemType is { } itemType ? ShopItemIconAtlas.GetItemTypeName(itemType) : null;

    public virtual Rectangle? IconSourceRect => null;

    public virtual int? Cost => null;

    protected AbstractStation(string stationId, Vector2 position)
    {
        StationId = stationId;
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
        Texture2D tex = AssetManager.GetStationTexture(StationId);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 bottomCenter = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(bottomCenter.Y);
        if (tex == AssetManager.BlankTexture)
        {
            int drawSize = tileSize - 10;
            Vector2 origin = new Vector2(drawSize / 2f, drawSize);
            Rectangle sourceRect = new Rectangle(0, 0, drawSize, drawSize);

            spriteBatch.Draw(
                texture: AssetManager.BlankTexture,
                position: bottomCenter,
                sourceRectangle: sourceRect,
                color: Color.White,
                rotation: PhysicsBody.Rotation,
                origin: origin,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: depth
            );
        }
        else
        {
            float scale = tileSize / (float)tex.Width;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(
                texture: tex,
                position: bottomCenter,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: depth
            );
        }

        if (HeldItem != null)
        {
            Vector2 itemHoverPosition = bottomCenter + new Vector2(0, -tileSize * 0.8f);
            HeldItem.Draw(spriteBatch, itemHoverPosition, tileSize / 2, depth + RenderUtility.Eps);
        }
    }
}