using System.Linq;
using Gamelab.Assets;
using Gamelab.Items.Bullets;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class ComponentResource(Vector2 position, string componentId)
    : AbstractResource(StationIds.GetComponentStationId(componentId),
        GamelabGame.Instance.ComponentRegistry.Get(componentId).Color,
        "Bullet",
        position)
{
    public string ComponentId { get; } = componentId;
    protected ComponentConfig DispensedComponentConfig => GamelabGame.Instance.ComponentRegistry.Get(ComponentId);

    public override string GetTooltipTitle()
    {
        return DispensedComponentConfig?.Name ?? "Unknown Component";
    }

    public override string GetTooltipDescription()
    {
        return DispensedComponentConfig?.Description ?? "No description";
    }

    public override bool IsTooltipVisible => IsHighlighted && DispensedComponentConfig != null;

    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new BulletItem(ComponentId);
        }
        else if (interactingPlayer.HeldItem.Id == ResourceId
                 && ((BulletItem)interactingPlayer.HeldItem).ComponentIds.SequenceEqual([ComponentId]))
        {
            interactingPlayer.HeldItem = null;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = AssetManager.GetStationTexture(GamelabGame.Instance.ComponentRegistry.Get(ComponentId).Sprite);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float originalSize = tex.Width;
        float scale = tileSize / originalSize;

        Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.5f);
        spriteBatch.Draw(tex, drawingPos, null, Color.White,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}