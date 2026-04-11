using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Items.Crafting;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Workbenches;

public abstract class AbstractWorkbench(string type, Color displayColor, Vector2 position)
    : AbstractStation(type, displayColor, position), IRepairable
{
    protected List<Recipe> ValidRecipes { get; } = new();
    protected List<Item> PlacedItems { get; } = new();

    private Recipe currentValidCompleteRecipe = null;
    private float craftProgress;
    private readonly RepairState repairState = new(type == "Anvil"
        ? GamelabGame.Instance.GameplayConfig.RepairableAnvilMaxHealth
        : GamelabGame.Instance.GameplayConfig.RepairableAnvilMaxHealth);

    private float RepairPerSecond => GamelabGame.Instance.GameplayConfig.RepairableAnvilRepairPerSecond;
    public bool IsBroken => repairState.IsBroken;
    public float CurrentHealth => repairState.CurrentHealth;
    public float MaxHealth => repairState.MaxHealth;

    public override void OnPickup(Player interactingPlayer)
    {
        if (IsBroken)
        {
            return;
        }

        if (HeldItem != null && interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = HeldItem;
            HeldItem = null;
            return;
        }

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
        if (IsBroken)
        {
            Repair(RepairPerSecond * dt);
            return;
        }

        if (currentValidCompleteRecipe == null) return;
        craftProgress += dt;
        if (craftProgress >= currentValidCompleteRecipe.CraftingTime)
        {
            PlacedItems.Clear();
            Item craftedItem = new(currentValidCompleteRecipe.OutputItemId);
            PlacedItems.Add(craftedItem);
            currentValidCompleteRecipe = null;
            craftProgress = 0f;
            CheckForCompleteRecipe();
        }
    }

    public void Repair(float amount)
    {
        repairState.Repair(amount);
    }

    public void TakeDamage(float damageAmount)
    {
        repairState.ApplyDamage(damageAmount);
        if (repairState.IsBroken)
        {
            craftProgress = 0f;
        }
    }

    public void OnHit(AbstractProjectile projectile)
    {
        if (projectile is not EnemyProjectile || IsBroken)
        {
            return;
        }

        TakeDamage(projectile.Damage);
        projectile.Deactivate();
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
        Color previousColor = DisplayColor;
        if (IsBroken)
        {
            DisplayColor = Color.DimGray;
        }

        base.Draw(spriteBatch);
        DisplayColor = previousColor;

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float itemSizeFloat = tileSize * 0.4f;
        int drawItemSize = (int)itemSizeFloat;
        float quadOffset = tileSize * 0.25f;
        Vector2[] gridOffsets =
        {
            new(-quadOffset, -quadOffset),
            new(quadOffset, -quadOffset),
            new(-quadOffset, quadOffset),
            new(quadOffset, quadOffset)
        };

        for (int i = 0; i < PlacedItems.Count; i++)
        {
            Vector2 itemPos = Position + gridOffsets[i];
            PlacedItems[i].Draw(spriteBatch, itemPos, drawItemSize);
        }

        if (!IsBroken && currentValidCompleteRecipe != null && craftProgress > 0f)
        {
            int barWidth = tileSize - 4;
            int barHeight = 6;
            float progressPercentage = craftProgress / currentValidCompleteRecipe.CraftingTime;

            Rectangle bgBar = new(
                (int)(Position.X - barWidth / 2f),
                (int)(Position.Y + (tileSize / 2f) - barHeight - 2),
                barWidth,
                barHeight
            );

            Rectangle fillBar = new(
                bgBar.X,
                bgBar.Y,
                (int)(barWidth * progressPercentage),
                barHeight
            );

            spriteBatch.Draw(AssetManager.BlankTexture, bgBar, Color.Black);
            spriteBatch.Draw(AssetManager.BlankTexture, fillBar, Color.Yellow);
        }

        DrawHealthBar(spriteBatch, tileSize);
    }

    private void DrawHealthBar(SpriteBatch spriteBatch, int tileSize)
    {
        if (CurrentHealth >= MaxHealth)
        {
            return;
        }

        int barWidth = tileSize - 10;
        int barHeight = 6;
        Rectangle bgBar = new(
            (int)(Position.X - barWidth / 2f),
            (int)(Position.Y + tileSize / 2f - 8),
            barWidth,
            barHeight);
        Rectangle fillBar = new(bgBar.X, bgBar.Y, (int)(barWidth * (CurrentHealth / MaxHealth)), barHeight);
        spriteBatch.Draw(AssetManager.BlankTexture, bgBar, Color.Black);
        spriteBatch.Draw(AssetManager.BlankTexture, fillBar, IsBroken ? Color.OrangeRed : Color.LimeGreen);
    }
}
