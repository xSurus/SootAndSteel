using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public abstract class AbstractStation : AbstractGrabbable, IInteractable, IPickable, IItemProvider,
    IItemReceiver, IUpdatable
{
    private static readonly Logger logger = new("Station");
    protected StationConfig StationConfig => GamelabGame.Instance.StationRegistry.Get(StationId);

    public string StationId { get; protected set; }

    protected readonly ISoundService soundService;
    public Item HeldItem { get; set; }
    public virtual Item PeekNextItem() => HeldItem;
    public virtual bool CanReceiveItem(Item item, IItemProvider source) => false;
    protected List<ProviderTicket> ProviderQueue { get; } = [];
    protected List<ConsumerTicket> ConsumerQueue { get; } = [];
    private const float TimeUntilKick = 0.5f;
    public Vector2 DrawPosition => Position - new Vector2(GamelabGame.Instance.GameplayConfig.TrainTileSize / 2f);
    protected override bool AllowPlayerRotation { get; } = false;

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

    protected AbstractStation(string stationId, Vector2 position)
    {
        StationId = stationId;
        float collisionSizePixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.90f;
        float simSize = collisionSizePixels.ToMeters();
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(simSize, simSize, 1f, position.ToMeters());

        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.PickupItem);
        soundService.LoadSound(Sounds.DropItem);
        soundService.LoadSound(Sounds.ShovelUp);
        soundService.LoadSound(Sounds.ShovelDown);
    }

    public virtual void OnPickup(Player interactingPlayer)
    {
        Item playerItem = interactingPlayer.PeekNextItem();
        if (playerItem != null && CanReceiveItem(playerItem, interactingPlayer) &&
            interactingPlayer.TryProvideItem(out Item playerItemTaken))
        {
            ReceiveItem(playerItemTaken, interactingPlayer);
        }
        else if (playerItem == null && TryProvideItem(out Item stationItem))
        {
            interactingPlayer.ReceiveItem(stationItem, this);
        }
    }

    public virtual void OnPickupHeld(Player interactingPlayer, float dt)
    {
    }

    protected override void OnLastRelease(Player interactingPlayer)
    {
        base.OnLastRelease(interactingPlayer);
        gameplayContext.Map?.SnapToNearestValidCell(this);
    }

    public virtual void Update(float dt)
    {
        for (int i = ProviderQueue.Count - 1; i >= 0; i--)
        {
            ProviderQueue[i].TimeSinceLastPing += dt;
            if (ProviderQueue[i].TimeSinceLastPing > TimeUntilKick) ProviderQueue.RemoveAt(i);
        }

        for (int i = ConsumerQueue.Count - 1; i >= 0; i--)
        {
            ConsumerQueue[i].TimeSinceLastPing += dt;
            if (ConsumerQueue[i].TimeSinceLastPing > TimeUntilKick) ConsumerQueue.RemoveAt(i);
        }
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

            spriteBatch.DrawWithHighlight(texture: tex,
                position: bottomCenter,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: depth,
                isHighlighted: IsHighlighted);
        }

        if (HeldItem != null)
        {
            Vector2 itemHoverPosition = bottomCenter + new Vector2(0, -tileSize * 0.8f);
            HeldItem.Draw(spriteBatch, itemHoverPosition, tileSize / 2, depth + RenderUtility.Eps);
        }
    }

    public virtual bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        item = HeldItem;
        if (HeldItem != null && CanProvideItem(consumer))
        {
            HeldItem = null;
            soundService.PlayOnce(ItemIsGranular(item) ? Sounds.ShovelUp : Sounds.PickupItem);
            if (consumer != null) ConsumerQueue.RemoveAll(t => t.Consumer == consumer);
            return true;
        }

        item = null;
        return false;
    }

    public virtual void ReceiveItem(Item item, IItemProvider source)
    {
        soundService.PlayOnce(ItemIsGranular(item) ? Sounds.ShovelDown : Sounds.DropItem);
        HeldItem = item;
    }

    public virtual void PingPushIntent(IItemProvider source, float dt)
    {
        ProviderTicket existingTicket = ProviderQueue.Find(t => t.Provider == source);
        if (existingTicket != null)
        {
            existingTicket.TimeSinceLastPing = 0f;
            return;
        }

        ProviderQueue.Add(new ProviderTicket(source));
    }

    public virtual void PingPullIntent(IItemReceiver consumer, float dt)
    {
        if (consumer == null) return;
        var existingTicket = ConsumerQueue.Find(t => t.Consumer == consumer);
        if (existingTicket != null)
        {
            existingTicket.TimeSinceLastPing = 0f;
            return;
        }

        ConsumerQueue.Add(new ConsumerTicket(consumer));
    }

    protected bool IsConsumerFirstInLine(IItemReceiver consumer)
    {
        if (consumer == null || consumer is Player) return true;
        return ConsumerQueue.Count == 0 || ConsumerQueue[0].Consumer == consumer;
    }

    public virtual bool CanProvideItem(IItemReceiver consumer)
    {
        return HeldItem != null && IsConsumerFirstInLine(consumer);
    }
    
    protected bool ItemIsGranular(Item item)
    {
        return (item is BulletItem && ((BulletItem) item).Type == EComponentType.Propellant) || item.Id == "Coal";
    }
}