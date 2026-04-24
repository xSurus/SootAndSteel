using Gamelab.Data;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Shop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public class BuyableStationWrapper : AbstractPhysicalEntity, IInteractable, IGrabbable, ITooltipable
{
    public string StationKindId { get; }
    private int Cost { get; }
    private AbstractStation WrappedStation { get; set; }
    public bool IsTooltipVisible => IsHighlighted;
    private string TooltipTitle { get; }
    private string TooltipDescription { get; }
    public string GetTooltipTitle() => TooltipTitle;
    public string GetTooltipDescription() => TooltipDescription;

    public Color GetTooltipTextColor() => GamelabGame.Instance.CurrentRun.Credits >= Cost ? Color.White : Color.Red;

    public BuyableStationWrapper(string stationKindId, Vector2 position)
    {
        StationKindId = stationKindId;
        CatalogItem item = GamelabGame.Instance.Services.GetService<IShopService>().GetCatalogItem(stationKindId);
        Cost = item?.Price ?? 0;
        TooltipTitle = item != null ? $"Buy {item.Name}" : $"Buy {StationKindId}";
        TooltipDescription = $"{Cost} Credits\n{(item?.Description ?? "")}";
        WrappedStation = StationFactory.CreateStation(StationKindId, position);
        PhysicsBody = WrappedStation.PhysicsBody;
        PhysicsBody.Tag = this;
    }

    public void OnInteract(Player interactingPlayer)
    {
        if (GamelabGame.Instance.CurrentRun.TrySpendCredits(Cost))
        {
            PhysicsBody.Tag = WrappedStation;
            gameplayContext.Map.MapObjects.Add(WrappedStation);
            gameplayContext.Map.SnapToNearestValidCell(WrappedStation);
            gameplayContext.Map.MapObjects.Remove(this);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        WrappedStation.Draw(spriteBatch);
    }

    public bool OnGrab(Player player, Vector2 grabPointWorldMeters)
    {
        return WrappedStation.OnGrab(player, grabPointWorldMeters);
    }

    public void OnRelease(Player player)
    {
        WrappedStation.OnRelease(player);
    }
}