using System.Collections.Generic;
using Gamelab.Items;
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
    private const int MaxCapacity = 4;
    public override Item PeekNextItem() => storedBullets.Count > 0 ? storedBullets[0] : null;

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
        base.Draw(spriteBatch);

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 feetPosition = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(feetPosition.Y);

        if (storedBullets.Count > 0)
        {
            int drawItemSize = (int)(tileSize * 0.4f);
            float quadOffset = tileSize * 0.2f;
            Vector2 tableTopCenter = feetPosition + new Vector2(0, -tileSize * 0.8f);

            Vector2[] gridOffsets =
            {
                new Vector2(-quadOffset, -quadOffset),
                new Vector2(quadOffset, -quadOffset),
                new Vector2(-quadOffset, quadOffset),
                new Vector2(quadOffset, quadOffset)
            };

            for (int i = 0; i < storedBullets.Count; i++)
            {
                Vector2 itemPos = tableTopCenter + gridOffsets[i];
                storedBullets[i].Draw(spriteBatch, itemPos, drawItemSize, depth + RenderUtility.Eps);
            }
        }
    }
}