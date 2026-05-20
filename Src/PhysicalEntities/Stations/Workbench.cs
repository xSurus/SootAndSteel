using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EventInstance = FmodForFoxes.Studio.EventInstance;

namespace Gamelab.PhysicalEntities.Stations;

public class Workbench : AbstractStation, IInteractable, IDisposable
{
    private static readonly Logger Logger = new(StationIds.Workbench);
    protected List<BulletItem> PlacedItems { get; } = new();

    private float craftProgress = 0f;
    private bool isCrafting = false;
    private float sparkCooldown = 0f;

    private EventInstance craftSound;

    private Vector2 TableTopCenter
    {
        get
        {
            int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
            return Position + new Vector2(0, tileSize / 2f - tileSize * 0.8f);
        }
    }

    public Workbench(Vector2 position, String stationId = StationIds.Workbench) : base(stationId, position)
    {
        soundService.LoadSound(Sounds.Craft);
        craftSound = soundService.GetSoundInstance(Sounds.Craft);
        soundService.RegisterParameter(craftSound, "Is Crafting", () => isCrafting ? 1.0f : 0.0f);
        craftSound?.Start();
    }

    public override bool CanReceiveItem(Item item, IItemProvider source)
    {
        if (craftProgress > 0f) return false;

        return ValidatePlace(item);
    }

    public override void ReceiveItem(Item item, IItemProvider source)
    {
        soundService.PlayOnce(ItemIsGranular(item) ? Sounds.ShovelDown : Sounds.DropItem);
        PlacedItems.Add((BulletItem)item);
    }

    public override Item PeekNextItem()
    {
        if (craftProgress > 0f) return null;
        return PlacedItems.LastOrDefault();
    }

    public override bool TryProvideItem(out Item item, IItemReceiver consumer = null)
    {
        item = null;
        if (!CanProvideItem(consumer)) return false;

        item = PlacedItems.Last();
        PlacedItems.RemoveAt(PlacedItems.Count - 1);

        soundService.PlayOnce(ItemIsGranular(item) ? Sounds.ShovelUp : Sounds.PickupItem);
        if (consumer != null) ConsumerQueue.RemoveAll(t => t.Consumer == consumer);
        return true;
    }

    public override bool CanProvideItem(IItemReceiver consumer)
    {
        return craftProgress == 0f && PlacedItems.Count > 0 && IsConsumerFirstInLine(consumer);
    }

    public void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (craftProgress > 0f || ValidateCraft())
        {
            craftProgress += dt;
            isCrafting = true;
        }

        if (isCrafting)
        {
            sparkCooldown -= dt;
            if (sparkCooldown <= 0f)
            {
                GamelabGame.Instance.Services.GetService<IVfxService>()
                    .EmitBurst(ParticleFactory.CreateWorkbenchSpark(TableTopCenter));
                sparkCooldown = 0.25f;
            }
        }

        if (craftProgress >= 2f) // TODO add dynamic craft time
        {
            var vfx = GamelabGame.Instance.Services.GetService<IVfxService>();
            vfx.EmitBurst(ParticleFactory.CreateWorkbenchSpark(TableTopCenter));
            vfx.EmitBurst(ParticleFactory.CreateWorkbenchSpark(TableTopCenter));
            vfx.EmitBurst(ParticleFactory.CreateWorkbenchSpark(TableTopCenter));

            BulletItem craftedItem = new BulletItem(PlacedItems.ToArray());
            PlacedItems.Clear();
            PlacedItems.Add(craftedItem);
            craftProgress = 0f;
            isCrafting = false;
            sparkCooldown = 0f;
        }
    }

    public void OnInteractReleased(Player interactingPlayer)
    {
        isCrafting = false;
        sparkCooldown = 0f;
    }

    public override void OnHighlightRemoved(Player player)
    {
        base.OnHighlightRemoved(player);
        isCrafting = IsHighlighted;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        Vector2 feetPosition = Position + new Vector2(0, tileSize / 2f);
        float depth = RenderUtility.CalculateDepth(feetPosition.Y);

        if (PlacedItems.Count > 0)
        {
            int drawItemSize = (int)(tileSize * 0.4f);
            float yOffset = tileSize * 0.1f;
            float spacing = tileSize * 0.25f;
            float totalWidth = (PlacedItems.Count - 1) * spacing;
            float startX = -totalWidth / 2f;
            Vector2 tableTopCenter = feetPosition + new Vector2(0, -tileSize * 0.8f);
            for (int i = 0; i < PlacedItems.Count; i++)
            {
                Vector2 itemPos = tableTopCenter + new Vector2(startX + i * spacing, -yOffset);
                PlacedItems[i].Draw(spriteBatch, itemPos, drawItemSize, depth + RenderUtility.Eps);
            }
        }
    }

    private List<EComponentType> GetContainedComponents()
    {
        return PlacedItems.Select(x => x.Type).Distinct().ToList();
    }

    private bool ValidatePlace(Item item)
    {
        if (item == null) return false;

        if (PlacedItems.Count >= 3) return false;

        if (item.Id != "Bullet") return false;
        BulletItem bulletItem = (BulletItem)item;

        // ComponentId may not be on table already
        if (PlacedItems.Any(placedItem =>
                placedItem.ComponentIds.Any(compId => bulletItem.ComponentIds.Contains(compId)))) return false;

        List<EComponentType> components = GetContainedComponents();

        // Check if item can be placed as upgrade
        if (!components.Any() ||
            (components.Count() == 1 && components.Contains(bulletItem.Type)))
            return true;

        // Check if item can be placed for final bullet
        if (!components.Contains(bulletItem.Type) &&
            PlacedItems.All(x => x.HasBasic) &&
            bulletItem.HasBasic)
            return true;

        return false;
    }

    private bool ValidateCraft()
    {
        List<EComponentType> components = GetContainedComponents();

        // check if valid upgrade
        if (PlacedItems.Count() == 2 &&
            components.Count() == 1 &&
            components.First() != EComponentType.Bullet)
            return true;

        // check if valid bullet
        if (PlacedItems.Count() == 3 &&
            components.Count() == 3 &&
            components.All(x => x != EComponentType.Bullet) &&
            PlacedItems.All(x => x.HasBasic))
            return true;

        return false;
    }

    public void Dispose()
    {
        craftSound?.Stop();
        craftSound?.Dispose();
    }
}