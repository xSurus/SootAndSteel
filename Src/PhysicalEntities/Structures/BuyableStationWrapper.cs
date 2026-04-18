using Gamelab.Assets;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Structures;

public class BuyableStationWrapper : AbstractPhysicalEntity, IInteractable, IGrabbable, ITooltipable
{
    protected StationConfig WrappedStationConfig => GamelabGame.Instance.ConfigurableStationRegistry.Get(StationKindId);
    public string StationKindId { get; }
    private int Cost { get; }
    private AbstractStation WrappedStation { get; set; }
    public bool IsTooltipVisible => IsHighlighted;

    public string GetTooltipTitle()
    {
        return WrappedStationConfig != null ? $"Buy {WrappedStationConfig.Title}" : $"Buy {StationKindId}";
    }

    public string GetTooltipDescription()
    {
        return $"{Cost} Credits\n{(WrappedStationConfig?.Description ?? "")}";
    }

    public Color GetTooltipTextColor() => GamelabGame.Instance.CurrentRun.Credits >= Cost ? Color.White : Color.Red;

    public BuyableStationWrapper(string stationKindId, Vector2 position)
    {
        StationKindId = stationKindId;
        var config = GamelabGame.Instance.ConfigurableStationRegistry.Get(stationKindId);
        Cost = config?.ShopPrice ?? 0;
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
        Texture2D tex = AssetManager.GetStationTexture(StationKindId);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float originalSize = tex.Width;
        float scale = tileSize / originalSize;

        Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.5f);
        spriteBatch.Draw(tex, drawingPos, null, Color.White,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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