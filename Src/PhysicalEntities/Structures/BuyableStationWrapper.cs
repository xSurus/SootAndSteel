using System.Linq;
using FontStashSharp;
using Gamelab.Assets;
using Gamelab.Map.Hub;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Structures;

public class BuyableStationWrapper : AbstractPhysicalEntity, IInteractable, IGrabbable, ITooltipable
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    public string StationKindId { get; }
    private int Cost { get; }
    private AbstractStation WrappedStation { get; set; }
    public Vector2 WorldPosition => Position;
    public bool IsActive => true;
    public string GetTitle() => $"Buy {StationKindId}";
    public string GetDescription() => $"{Cost} Gold";
    public Color GetTextColor() => GamelabGame.Instance.Credits >= Cost ? Color.White : Color.Red;

    public BuyableStationWrapper(string stationKindId, int cost, Vector2 position)
    {
        StationKindId = stationKindId;
        Cost = cost;
        WrappedStation = StationYardFactory.CreateYardStation(StationKindId, position);
        PhysicsBody = WrappedStation.PhysicsBody;
        PhysicsBody.Tag = this;
    }

    public void OnInteract(Player interactingPlayer)
    {
        if (GamelabGame.Instance.TrySpendCredits(Cost))
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