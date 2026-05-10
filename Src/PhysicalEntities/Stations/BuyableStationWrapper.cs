using Gamelab.Data;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Shop;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public class BuyableStationWrapper : AbstractPhysicalEntity, IInteractable, IGrabbable, ITooltipable
{
    public string StationKindId { get; }
    public int Cost { get; }

    private AbstractStation WrappedStation { get; set; }
    public bool IsVisible => IsHighlighted;
    private string Title { get; }
    private string Description { get; }
    public string GetTitle() => Title;
    public string GetDescription() => Description;

    /// <summary>
    /// Category and icon mirror whatever the wrapped station would show post-purchase, so a
    /// dispenser in the shop has the same identity as one already on the train.
    /// </summary>
    public string CategoryName => WrappedStation?.CategoryName;

    public string FunctionalityName => WrappedStation?.FunctionalityName;

    public Rectangle? IconSourceRect => WrappedStation?.IconSourceRect;

    // Wrapper exposes the cost as a non-nullable int (callers always need it); the
    // ITooltipable contract is the optional view of the same value.
    int? ITooltipable.Cost => Cost;

    private readonly ISoundService soundService;

    public BuyableStationWrapper(string stationKindId, Vector2 position)
    {
        StationKindId = stationKindId;
        CatalogItem item = GamelabGame.Instance.Services.GetService<IShopService>().GetCatalogItem(stationKindId);
        Cost = item?.Price ?? 0;
        Title = item != null ? $"{item.Name}" : $"{StationKindId}";
        Description = item?.Description ?? string.Empty;
        WrappedStation = StationFactory.CreateStation(StationKindId, position);
        PhysicsBody = WrappedStation.PhysicsBody;
        PhysicsBody.Tag = this;
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.Purchase);
    }

    public void OnInteract(Player interactingPlayer)
    {
        if (GamelabGame.Instance.CurrentRun.TrySpendCredits(Cost))
        {
            PhysicsBody.Tag = WrappedStation;
            gameplayContext.Map.MapObjects.Add(WrappedStation);
            gameplayContext.Map.SnapToNearestValidCell(WrappedStation);
            gameplayContext.Map.MapObjects.Remove(this);
            soundService.PlayOnce(Sounds.Purchase);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        WrappedStation.Draw(spriteBatch);
    }

    public override void OnHighlight(Player player)
    {
        base.OnHighlight(player);
        WrappedStation.OnHighlight(player);
    }

    public override void OnHighlightRemoved(Player player)
    {
        base.OnHighlightRemoved(player);
        WrappedStation.OnHighlightRemoved(player);
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