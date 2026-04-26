using System.Linq;
using Gamelab.Assets;
using Gamelab.Items.Bullets;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class ComponentResourceStation(Vector2 position, string componentId)
    : ResourceStation(
        position,
        StationIds.GetComponentResourceId(componentId))
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
        float scale = tileSize / (float)tex.Width;
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
        Vector2 bottomCenter = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(bottomCenter.Y);

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
}