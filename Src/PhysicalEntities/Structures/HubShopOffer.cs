using System.Collections.Generic;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Assets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

/// <summary>
/// A shop upgrade tile that lives in the hub vendor row until dragged onto the prep train.
/// Once on the train it stays as an <see cref="AbstractStation"/> placeholder until the player
/// interacts and pays. On depart, <see cref="PrepTrainLayout"/> captures it by <see cref="AbstractStation.Type"/>
/// and <see cref="Hub.StationYardFactory"/> converts it to the real station in gameplay.
/// </summary>
public sealed class HubShopOffer : AbstractStation
{
    private readonly Vector2 spawnCenterPixels;
    private readonly TrainMap prepTrainMap;
    private readonly List<HubShopOffer> hubDragOffers;
    private bool isOnTrain;

    public int Cost { get; }
    public string DisplayName { get; }
    public bool IsPurchased { get; private set; }

    public HubShopOffer(
        Vector2 centerPixels,
        int cost,
        string stationKindId,
        string displayName,
        Color displayColor,
        TrainMap prepTrainMap,
        List<HubShopOffer> hubDragOffers)
        : base(stationKindId, displayColor, centerPixels)
    {
        spawnCenterPixels = centerPixels;
        this.prepTrainMap = prepTrainMap;
        this.hubDragOffers = hubDragOffers;
        Cost = cost;
        DisplayName = displayName;
    }

    public string BuildTooltipText() => isOnTrain
        ? $"{DisplayName} — {Cost}c\nInteract to purchase."
        : $"{DisplayName} — {Cost} credits\nDrag onto the train, then Interact to purchase.";

    public override void OnInteract(Player interactingPlayer)
    {
        if (IsPurchased || !isOnTrain) return;
        if (!GamelabGame.Instance.TrySpendCredits(Cost)) return;
        IsPurchased = true;
    }

    protected override void OnLastRelease(Player interactingPlayer)
    {
        Point tile = prepTrainMap.GetTileIndexFromPixels(Position);
        bool validDrop = gameplayContext.Map == prepTrainMap
            && tile.X >= 0 && tile.X < prepTrainMap.Width
            && tile.Y >= 0 && tile.Y < prepTrainMap.Height
            && !IsTileOccupied(tile);

        if (!validDrop)
        {
            if (isOnTrain)
            {
                isOnTrain = false;
                prepTrainMap.MapObjects.Remove(this);
                hubDragOffers.Add(this);
            }
            PhysicsBody.BodyType = BodyType.Static;
            PhysicsBody.Position = spawnCenterPixels.ToMeters();
            PhysicsBody.Rotation = 0f;
            return;
        }

        Position = prepTrainMap.GetTileCenterPixels(tile.X, tile.Y);
        PhysicsBody.Rotation = 0f;
        PhysicsBody.BodyType = BodyType.Static;

        if (!isOnTrain)
        {
            isOnTrain = true;
            prepTrainMap.MapObjects.Add(this);
            hubDragOffers.Remove(this);
        }
    }

    private bool IsTileOccupied(Point tile)
    {
        foreach (IPhysicalEntity entity in prepTrainMap.MapObjects)
            if (entity is AbstractStation station && !ReferenceEquals(station, this)
                && prepTrainMap.GetTileIndexFromPixels(station.Position) == tile)
                return true;
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {   
        Texture2D tex = AssetManager.GetStationTexture(Type);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float originalSize = tex.Width;
        float scale = tileSize / originalSize;

        Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.5f);
        spriteBatch.Draw(tex, drawingPos, null, Color.White,
                                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
