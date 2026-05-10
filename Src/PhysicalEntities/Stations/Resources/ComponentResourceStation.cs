using System.Linq;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Sound;
using Gamelab.UI;
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

    private AbstractComponent dispensedComponentCache;

    private AbstractComponent DispensedComponent =>
        dispensedComponentCache ??= ComponentFactory.CreateDefinition(ComponentId);

    public override string GetTitle()
    {
        return DispensedComponentConfig?.Name ?? "Unknown Component";
    }

    public override string GetDescription()
    {
        return DispensedComponentConfig?.Description ?? "No description";
    }

    public EItemType DispensedItemType => DispensedComponent.IsBasic ? EItemType.Basic : EItemType.Upgrading;

    public override string CategoryName => ShopItemIconAtlas.GetComponentTypeName(DispensedComponent.Type);

    public override string FunctionalityName => ShopItemIconAtlas.GetItemTypeName(DispensedItemType);

    public override Rectangle? IconSourceRect =>
        ShopItemIconAtlas.TryGetRect(DispensedComponent.Type, DispensedItemType);

    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        return item is { Id: "Bullet" } && ((BulletItem)item).ComponentIds.SequenceEqual([ComponentId]);
    }

    public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        if (!CanProvideItem(consumer))
        {
            item = null;
            return false;
        }

        item = new BulletItem(ComponentId);
        soundService.PlayOnce(DispensedComponent.Type == EComponentType.Propellant ? Sounds.ShovelUp : Sounds.PickupItem);
        if (consumer != null) ConsumerQueue.RemoveAll(t => t.Consumer == consumer);

        return true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = AssetManager.GetStationTexture(GamelabGame.Instance.ComponentRegistry.Get(ComponentId).Sprite);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float scale = tileSize / (float)tex.Width;
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
        Vector2 bottomCenter = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(bottomCenter.Y);

        spriteBatch.DrawWithHighlight(texture: tex,
            position: bottomCenter,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: depth,
            IsHighlighted
        );
    }
}