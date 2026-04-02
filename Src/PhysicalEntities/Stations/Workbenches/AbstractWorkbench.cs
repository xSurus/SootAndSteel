using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Items.Crafting;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Workbenches;

public abstract class AbstractWorkbench(string type, Color displayColor, Vector2 position)
    : AbstractStation(type, displayColor, position)
{
    protected List<Recipe> ValidRecipes { get; } = new();
    protected List<Item> PlacedItems { get; } = new();

    private Recipe currentValidCompleteRecipe = null;
    private float craftProgress = 0f;

    public override void OnPickup(Player interactingPlayer)
    {
        // playerp picks up held item
        if (HeldItem != null && interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = HeldItem;
            HeldItem = null;
            return;
        }

        // player places an item onto the workbench
        if (interactingPlayer.HeldItem != null && HeldItem == null)
        {
            if (CanAcceptItem(interactingPlayer.HeldItem))
            {
                PlacedItems.Add(interactingPlayer.HeldItem);
                interactingPlayer.HeldItem = null;
                CheckForCompleteRecipe();
                craftProgress = 0f;
            }
        }
        // player takes back item from recipe collection
        else if (interactingPlayer.HeldItem == null && PlacedItems.Count > 0 && HeldItem == null)
        {
            interactingPlayer.HeldItem = PlacedItems.Last();
            PlacedItems.RemoveAt(PlacedItems.Count - 1);
            CheckForCompleteRecipe();
            craftProgress = 0f;
        }
    }

    public override void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (currentValidCompleteRecipe == null) return;
        craftProgress += dt;
        if (craftProgress >= currentValidCompleteRecipe.CraftingTime)
        {
            PlacedItems.Clear();
            Item craftedItem = new Item(currentValidCompleteRecipe.OutputItemId);
            PlacedItems.Add(craftedItem);
            currentValidCompleteRecipe = null;
            craftProgress = 0f;
            CheckForCompleteRecipe();
        }
    }

    private bool CanAcceptItem(Item newItem)
    {
        if (PlacedItems.Count >= 4) return false;
        if (PlacedItems.Count == 0) return true;
        List<string> testList = PlacedItems.Select(i => i.Id).ToList();
        testList.Add(newItem.Id);

        foreach (var recipe in ValidRecipes)
        {
            if (CraftingUtility.IsValidPartialRecipe(testList, recipe.RequiredItemIds))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckForCompleteRecipe()
    {
        List<string> currentIds = PlacedItems.Select(i => i.Id).ToList();

        foreach (var recipe in ValidRecipes)
        {
            if (CraftingUtility.IsCompleteMatch(currentIds, recipe.RequiredItemIds))
            {
                currentValidCompleteRecipe = recipe;
                return;
            }
        }

        currentValidCompleteRecipe = null;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        float itemSizeFloat = tileSize * 0.4f;
        int drawItemSize = (int)itemSizeFloat;

        Vector2 dynamicPos = DrawPosition;

        float cellSize = tileSize * 0.5f;

        float centerOffset = (cellSize - itemSizeFloat) * 0.5f;

        for (int i = 0; i < PlacedItems.Count; i++)
        {
            int row = i / 2;
            int col = i % 2;

            Vector2 itemPos = new Vector2(
                dynamicPos.X + (col * cellSize) + centerOffset,
                dynamicPos.Y + (row * cellSize) + centerOffset
            );

            PlacedItems[i].Draw(spriteBatch, itemPos, drawItemSize);
        }

        if (currentValidCompleteRecipe != null && craftProgress > 0f)
        {
            int barWidth = tileSize - 4;
            int barHeight = 6;

            float progressPercentage = craftProgress / currentValidCompleteRecipe.CraftingTime;

            Rectangle bgBar = new Rectangle(
                (int)dynamicPos.X + 2,
                (int)dynamicPos.Y + tileSize - barHeight - 2,
                barWidth,
                barHeight
            );

            Rectangle fillBar = new Rectangle(
                bgBar.X,
                bgBar.Y,
                (int)(barWidth * progressPercentage),
                barHeight
            );

            spriteBatch.Draw(AssetManager.BlankTexture, bgBar, Color.Black);
            spriteBatch.Draw(AssetManager.BlankTexture, fillBar, Color.Yellow);
        }
    }
}