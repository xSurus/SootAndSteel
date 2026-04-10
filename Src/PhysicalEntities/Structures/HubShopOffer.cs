using System;
using Gamelab.Assets;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Structures;

public sealed class HubShopOffer : AbstractGrabbable, IInteractable, IPickable, IDisposable
{
    private readonly Vector2 spawnCenterPixels;
    private readonly TrainMap prepTrainMap;
    private readonly Func<Point, bool> isTrainTileOccupied;
    private readonly Action<HubShopOffer, Vector2> onPlacedOnTrain;

    public string StationKindId { get; }
    public int Cost { get; }
    public string DisplayName { get; }
    public Color DisplayColor { get; }

    protected override bool AllowPlayerRotation => false;

    public HubShopOffer(
        Vector2 centerPixels,
        int cost,
        string stationKindId,
        string displayName,
        Color displayColor,
        TrainMap prepTrainMap,
        Func<Point, bool> isTrainTileOccupied,
        Action<HubShopOffer, Vector2> onPlacedOnTrain)
    {
        spawnCenterPixels = centerPixels;
        this.prepTrainMap = prepTrainMap;
        this.isTrainTileOccupied = isTrainTileOccupied;
        this.onPlacedOnTrain = onPlacedOnTrain;
        StationKindId = stationKindId;
        Cost = cost;
        DisplayName = displayName;
        DisplayColor = displayColor;

        float collisionSizePixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.9f;
        float simSize = collisionSizePixels.ToMeters();
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(
            simSize,
            simSize,
            1f,
            centerPixels.ToMeters(),
            0f,
            BodyType.Static);
    }

    public string BuildTooltipText()
    {
        return $"{DisplayName} — {Cost} credits\nDrag onto the train; pay when you depart.";
    }

    public void OnInteract(Player interactingPlayer)
    {
    }

    public void OnInteractHeld(Player interactingPlayer, float dt)
    {
    }

    public void OnPickup(Player interactingPlayer)
    {
    }

    public void OnPickupHeld(Player interactingPlayer, float dt)
    {
    }

    private void SnapBackToVendor()
    {
        PhysicsBody.BodyType = BodyType.Static;
        PhysicsBody.Position = spawnCenterPixels.ToMeters();
        PhysicsBody.Rotation = 0f;
    }

    protected override void OnLastRelease(Player interactingPlayer)
    {
        GameplayContext ctx = GamelabGame.Instance.Services.GetService<GameplayContext>();
        if (ctx.Map != prepTrainMap)
        {
            SnapBackToVendor();
            return;
        }

        Point g = prepTrainMap.GetTileIndexFromPixels(Position);
        if (g.X < 0 || g.X >= prepTrainMap.Width || g.Y < 0 || g.Y >= prepTrainMap.Height)
        {
            SnapBackToVendor();
            return;
        }

        if (isTrainTileOccupied(g))
        {
            SnapBackToVendor();
            return;
        }

        Vector2 center = prepTrainMap.GetTileCenterPixels(g.X, g.Y);
        onPlacedOnTrain(this, center);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 topLeft = Position - new Vector2(tileSize / 2f);
        Rectangle rect = new Rectangle((int)topLeft.X + 5, (int)topLeft.Y + 5, tileSize - 10, tileSize - 10);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, DisplayColor);
    }

    public void Dispose()
    {
        if (PhysicsBody?.World == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
    }
}
