using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EventInstance = FmodForFoxes.Studio.EventInstance;

namespace Gamelab.PhysicalEntities.Stations;

public class Workbench : AbstractStation
{
    private static readonly Logger Logger = new(StationIds.Workbench);
    protected List<BulletItem> PlacedItems { get; } = new();

    private float craftProgress = 0f;
    private bool isCrafting = false;

    private readonly ISoundService soundService;
    private EventInstance craftSound;

    public Workbench(Vector2 position) : base(StationIds.Workbench, position)
    {
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.Craft);
        craftSound = soundService.GetSoundInstance(Sounds.Craft);
        soundService.RegisterParameter(craftSound, "Is Crafting", () => isCrafting ? 1.0f : 0.0f);
        craftSound?.Start();
    }

    public override void OnPickup(Player interactingPlayer)
    {
        if (craftProgress > 0f) return;

        // player picks up item from workbench
        if (interactingPlayer.HeldItem == null && PlacedItems.Any())
        {
            interactingPlayer.HeldItem = PlacedItems.Last();
            PlacedItems.RemoveAt(PlacedItems.Count - 1);
            soundService.PlayOnce(Sounds.PickupItem);
            return;
        }

        // player places item onto workbench
        if (ValidatePlace(interactingPlayer.HeldItem))
        {
            PlacedItems.Add((BulletItem)interactingPlayer.HeldItem);
            interactingPlayer.HeldItem = null;
            soundService.PlayOnce(Sounds.DropItem);
        }
    }

    public override void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (craftProgress > 0f || ValidateCraft())
        {
            craftProgress += dt;
            isCrafting = true;
        }

        if (craftProgress >= 2f) // TODO add dynamic craft time
        {
            BulletItem craftedItem = new BulletItem(PlacedItems.ToArray());
            PlacedItems.Clear();
            PlacedItems.Add(craftedItem);
            craftProgress = 0f;
            isCrafting = false;
        }
    }

    public override void OnInteractReleased(Player interactingPlayer)
    {
        isCrafting = false;
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
            float quadOffset = tileSize * 0.2f;
            Vector2 tableTopCenter = feetPosition + new Vector2(0, -tileSize * 0.8f);
            Vector2[] gridOffsets =
            {
                new Vector2(-quadOffset, -quadOffset),
                new Vector2(quadOffset, -quadOffset),
                new Vector2(-quadOffset, quadOffset),
                new Vector2(quadOffset, quadOffset)
            };

            for (int i = 0; i < PlacedItems.Count; i++)
            {
                Vector2 itemPos = tableTopCenter + gridOffsets[i];
                PlacedItems[i].Draw(spriteBatch, itemPos, drawItemSize, depth + RenderUtility.Eps);
            }
        }

        if (craftProgress > 0f)
        {
            int barWidth = tileSize - 4;
            int barHeight = 6;
            float progressPercentage = craftProgress / 2f; // TODO add dynamic craft time

            Vector2 barPos = feetPosition + new Vector2(-barWidth / 2f, -tileSize - 10f);

            Rectangle bgBar = new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight);
            Rectangle fillBar = new Rectangle(bgBar.X, bgBar.Y, (int)(barWidth * progressPercentage), barHeight);

            spriteBatch.Draw(AssetManager.BlankTexture, bgBar, null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None,
                depth + 2 * RenderUtility.Eps);
            spriteBatch.Draw(AssetManager.BlankTexture, fillBar, null, Color.Yellow, 0f, Vector2.Zero,
                SpriteEffects.None, depth + 3 * RenderUtility.Eps);
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
}