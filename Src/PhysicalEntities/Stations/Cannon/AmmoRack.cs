using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Players;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Services.Sound;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Cannon;

public class BulletRack(Vector2 position)
    : AbstractStation(StationIds.BulletRack, position)
{
    private readonly List<Item> storedBullets = [];
    private const int MaxCapacity = 5;
    public override Item PeekNextItem() => storedBullets.Count > 0 ? storedBullets[0] : null;

    public override bool OnGrab(Player interactingPlayer, Vector2 grabPointWorldMeters) => false;

    public override bool CanProvideItem(IItemReceiver consumer)
    {
        return storedBullets.Count > 0 && IsConsumerFirstInLine(consumer);
    }

    public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        item = null;
        if (!CanProvideItem(consumer)) return false;
        item = storedBullets[0];
        storedBullets.RemoveAt(0);
        soundService.PlayOnce(Sounds.PickupItem);
        if (consumer != null)
        {
            ConsumerQueue.RemoveAll(t => t.Consumer == consumer);
        }

        return true;
    }

    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        return storedBullets.Count < MaxCapacity && item is BulletItem { Type: EComponentType.Bullet };
    }

    public override void ReceiveItem(Item item, IItemProvider source)
    {
        storedBullets.Add(item);
        soundService.PlayOnce(Sounds.DropItem);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D bgTex = AssetManager.GetStructureTexture("BulletRack");
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 feetPosition = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(feetPosition.Y);
        float bgScale = tileSize / (float)bgTex.Width * 1.2f;
        Color idleLight = new(255, 255, 255, 20);
        Color highlightLight = new(255, 255, 255, 44);
        spriteBatch.DrawWithLightBoost(bgTex, feetPosition, null, Color.White, idleLight, highlightLight, 0f,
            new Vector2(bgTex.Width / 2f, bgTex.Height), bgScale, SpriteEffects.None, depth, IsHighlighted);

        if (storedBullets.Count > 0)
        {
            int drawItemSize = (int)(tileSize * 0.3f);
            float slotSpacing = tileSize * 0.2f;
            Vector2 tableTopCenter = feetPosition + new Vector2(-tileSize * 0.1f, -tileSize * 1.2f);

            for (int i = 0; i < storedBullets.Count; i++)
            {
                float y = (i - (MaxCapacity - 1) / 2f) * slotSpacing;
                storedBullets[i].Draw(spriteBatch, tableTopCenter + new Vector2(0, y), drawItemSize, depth + RenderUtility.Eps, MathF.PI / 2f);
            }
        }
    }
}