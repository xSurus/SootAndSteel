using System;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Animation;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Conveyors;

public class Conveyor : AbstractStation, IUpdatable, IInteractable
{
    public GridDirection FacingDirection { get; protected set; }
    private float transportTimer = 0f;
    private const float TransportDuration = 3.0f;
    private readonly AnimatedSprite topSprite;
    private readonly IAnimationService animationService = GamelabGame.Instance.Services.GetService<IAnimationService>();

    public Conveyor(Vector2 position, GridDirection facingDirection, String stationId = StationIds.Conveyor) : base(
        stationId, position)
    {
        FacingDirection = facingDirection;
        topSprite = new AnimatedSprite(AssetManager.ConveyorSpriteSheet);
        topSprite.SetAnimation("Run");
        topSprite.Origin = new Vector2(50f, 50f);
        animationService.Register(topSprite, false);
    }

    protected virtual bool AcceptsItem(Item item)
    {
        return true;
    }

    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        if (HeldItem != null || item == null || !AcceptsItem(item)) return false;
        if (source is Player) return true;

        if (ProviderQueue.Count == 0 || ProviderQueue[0].Provider == source)
        {
            return true;
        }

        return false;
    }

    public void OnInteract(Player interactingPlayer)
    {
        FacingDirection = FacingDirection.Clockwise();
    }

    public override Item PeekNextItem()
    {
        if (transportTimer >= TransportDuration / 2f)
        {
            return HeldItem;
        }

        return null;
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        if (IsBeingHeld) return;

        float previousTimer = transportTimer;

        AbstractStation sourceStation = GetSourceStation();
        IItemProvider passiveProvider = sourceStation as IItemProvider;
        bool shouldPull = passiveProvider != null &&
                          (sourceStation is not Conveyor ||
                           ((Conveyor)sourceStation).FacingDirection != FacingDirection);

        IItemReceiver sinkStation = GetSinkStation();
        if (sinkStation != null)
        {
            PingPullIntent(sinkStation, dt);
        }

        if (shouldPull)
        {
            Item nextItem = passiveProvider.PeekNextItem();
            if (nextItem != null && AcceptsItem(nextItem))
            {
                PingPushIntent(passiveProvider, dt);
            }
        }

        if (HeldItem == null)
        {
            if (shouldPull)
            {
                Item nextItem = passiveProvider.PeekNextItem();

                if (nextItem == null || AcceptsItem(nextItem))
                {
                    passiveProvider.PingPullIntent(this, dt);
                    if (nextItem != null && passiveProvider.CanProvideItem(this) &&
                        CanReceiveItem(nextItem, passiveProvider))
                    {
                        if (passiveProvider.TryProvideItem(out Item grabbedItem, this))
                        {
                            ReceiveItem(grabbedItem, passiveProvider);
                        }
                    }
                }
            }
        }
        else
        {
            float halfDuration = TransportDuration / 2f;
            bool sinkReady = false;

            if (sinkStation != null)
            {
                sinkStation.PingPushIntent(this, dt);
                bool canReceive = sinkStation.CanReceiveItem(HeldItem, this);
                sinkReady = canReceive && IsConsumerFirstInLine(sinkStation);
            }

            if (transportTimer < halfDuration)
            {
                transportTimer += dt;

                if (transportTimer >= halfDuration && !sinkReady)
                {
                    transportTimer = halfDuration;
                }
            }
            else if (transportTimer < TransportDuration)
            {
                if (sinkReady)
                {
                    transportTimer += dt;
                }
            }
            else
            {
                if (sinkReady)
                {
                    sinkStation.ReceiveItem(HeldItem, this);
                    ConsumerQueue.RemoveAll(t => t.Consumer == sinkStation);
                    HeldItem = null;
                    transportTimer = 0f;
                }
            }
        }

        animationService.SetActive(topSprite, transportTimer > previousTimer);
    }

    public override void ReceiveItem(Item item, IItemProvider source)
    {
        HeldItem = item;
        ProviderQueue.RemoveAll(t => t.Provider == source);
        if (source is Player || source != GetSourceStation())
        {
            transportTimer = TransportDuration / 2f;
        }
        else
        {
            transportTimer = 0f;
        }
    }


    private AbstractStation GetSourceStation()
        => gameplayContext.Map.GetAdjacentStation(Position, FacingDirection.Opposite());

    private AbstractStation GetSinkStation()
        => gameplayContext.Map.GetAdjacentStation(Position, FacingDirection);

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 bottomCenter = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(bottomCenter.Y);

        // draw base
        Texture2D tex = AssetManager.GetStationTexture(StationId);
        float scale = tileSize / (float)tex.Width;
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

        spriteBatch.DrawWithHighlight(
            texture: tex,
            position: bottomCenter,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: depth,
            isHighlighted: IsHighlighted
        );

        float topDepth = depth + RenderUtility.Eps;
        topSprite.Depth = topDepth;
        float topScale = tileSize / 100f;
        float rotation = FacingDirection switch
        {
            GridDirection.Right => 0f,
            GridDirection.Up => -MathHelper.PiOver2,
            GridDirection.Left => MathHelper.Pi,
            GridDirection.Down => MathHelper.PiOver2,
            _ => 0f
        };

        Vector2 tableTopCenter = bottomCenter + new Vector2(0, -tileSize * 0.9f);
        spriteBatch.DrawWithHighlight(topSprite, tableTopCenter, rotation, new Vector2(topScale), IsHighlighted);

        // draw item
        if (HeldItem != null)
        {
            float progress = Math.Clamp(transportTimer / TransportDuration, 0f, 1f);
            float slideFactor = progress - 0.5f;
            Vector2 offset = FacingDirection.ToVector2() * (tileSize * slideFactor);

            Vector2 animatedItemPos = tableTopCenter + offset;

            int itemDrawSize = tileSize / 2;
            Vector2 bottomCenteredPos = animatedItemPos + new Vector2(0, itemDrawSize / 2f);

            float itemDepthY = bottomCenter.Y + offset.Y + (tileSize * 0.8f);
            float itemDepth = RenderUtility.CalculateDepth(itemDepthY);

            HeldItem.Draw(spriteBatch, bottomCenteredPos, itemDrawSize, itemDepth);
        }
    }
}