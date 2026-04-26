using System.Collections.Generic;
using System.Linq;
using Gamelab.Components;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Microsoft.Xna.Framework;
using MonoGameGum;
using Gamelab;

namespace Gamelab.UI;

/// <summary>
/// Owns the in-world Gum tooltips that float above hovered <see cref="ITooltipable"/>s
/// (stations, buyable shop wrappers, ...). One <see cref="ToolTip"/> is created per item
/// the first time it appears, kept hidden until <see cref="ITooltipable.IsVisible"/>
/// is true, and recycled when the item leaves the map.
/// Overlap separation assumes at most four simultaneous highlights (one per local player).
/// </summary>
public class WorldUiManager(GamelabGame game)
{
    /// <summary>Matches max local players; visible world tooltips are typically ≤ this in co-op.</summary>
    private const int MaxExpectedConcurrentTooltips = 4;

    private readonly Dictionary<ITooltipable, ToolTip> activeTooltips = new();
    private readonly List<LivePlacement> placementBuffer = new(MaxExpectedConcurrentTooltips);

    public void Update(IEnumerable<IPhysicalEntity> mapObjects, Matrix cameraMatrix)
    {
        var tooltipables = mapObjects.OfType<ITooltipable>().ToHashSet();
        var toRemove = activeTooltips.Keys.Where(k => !tooltipables.Contains(k)).ToList();
        foreach (var item in toRemove) RemoveTooltip(item);

        placementBuffer.Clear();
        foreach (var item in tooltipables)
        {
            if (!activeTooltips.TryGetValue(item, out var tooltip))
            {
                tooltip = CreateTooltip(item);
            }

            tooltip.Visual.Visible = item.IsVisible;
            if (item.IsVisible)
            {
                UpdateAffordabilityState(item, tooltip);
                ComputeIdealTooltipPlacement(item, tooltip, cameraMatrix, out float x, out float y, out float w, out float h);
                placementBuffer.Add(new LivePlacement(tooltip, x, y, w, h));
            }
        }

        ResolveTooltipOverlaps(placementBuffer);
        foreach (var p in placementBuffer)
        {
            p.Tooltip.Visual.X = p.X;
            p.Tooltip.Visual.Y = p.Y;
        }
    }

    private ToolTip CreateTooltip(ITooltipable item)
    {
        var tooltip = new ToolTip();
        tooltip.AddToRoot();
        ApplyStaticContent(item, tooltip);

        // Park offscreen until the first position update so the first frame doesn't flicker at (0,0).
        tooltip.Visual.X = -10000;
        tooltip.Visual.Y = -10000;
        tooltip.Visual.Visible = false;

        activeTooltips[item] = tooltip;
        return tooltip;
    }

    /// <summary>
    /// Writes the per-item fields that don't change after construction (title, description,
    /// category, icon, button text/visibility). Called once from <see cref="CreateTooltip"/>.
    /// </summary>
    private static void ApplyStaticContent(ITooltipable item, ToolTip tooltip)
    {
        tooltip.Name = item.GetTitle();
        tooltip.ItemDescription = item.GetDescription();
        tooltip.Functionality = FormatFunctionality(item.FunctionalityName);
        ApplyCategoryRow(tooltip, item);
        ConfigureInteractionButtons(tooltip, item);
    }

    private static string FormatFunctionality(string functionalityName) =>
        string.IsNullOrEmpty(functionalityName) ? "" : $"- {functionalityName} -";

    private void ComputeIdealTooltipPlacement(
        ITooltipable item,
        ToolTip tooltip,
        Matrix cameraMatrix,
        out float x,
        out float y,
        out float width,
        out float height)
    {
        Vector2 screenPos = Vector2.Transform(item.Position, cameraMatrix);
        Vector2 gumCanvasPos = ConvertScreenToGumCanvas(screenPos);

        int tileSize = game.GameplayConfig.TrainTileSize;
        const float liftAmount = 25f;

        // Centre using the nine-slice panel, not tooltip.Visual: the root container is parented to
        // Gum Root and often reports the full canvas width (~1920), which pulls the box to screen center.
        GetTooltipPanelSize(tooltip, out width, out height);

        // Visual.X/Y on the Gum root is the top-left corner; centre the tooltip horizontally
        // above the item, lifting it above the tile.
        x = gumCanvasPos.X - width / 2f;
        y = gumCanvasPos.Y - tileSize / 2f - height - liftAmount;
    }

    /// <summary>
    /// Pushes overlapping tooltip panels apart in Gum space so multiple local players can read nearby stations.
    /// Prefers vertical separation when the intersection is taller than it is wide, otherwise horizontal.
    /// </summary>
    private static void ResolveTooltipOverlaps(List<LivePlacement> placements)
    {
        if (placements.Count <= 1)
        {
            return;
        }

        const float gap = 10f;
        // Enough for four panels to settle; pairwise count is tiny.
        const int iterations = 8;
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int i = 0; i < placements.Count; i++)
            {
                for (int j = i + 1; j < placements.Count; j++)
                {
                    LivePlacement a = placements[i];
                    LivePlacement b = placements[j];
                    if (!TryGetOverlap(a, b, out float overlapW, out float overlapH))
                    {
                        continue;
                    }

                    float push;
                    if (overlapH >= overlapW)
                    {
                        push = overlapH * 0.5f + gap * 0.5f;
                        float cyA = a.Y + a.H * 0.5f;
                        float cyB = b.Y + b.H * 0.5f;
                        if (cyA < cyB)
                        {
                            a.Y -= push;
                            b.Y += push;
                        }
                        else
                        {
                            a.Y += push;
                            b.Y -= push;
                        }
                    }
                    else
                    {
                        push = overlapW * 0.5f + gap * 0.5f;
                        float cxA = a.X + a.W * 0.5f;
                        float cxB = b.X + b.W * 0.5f;
                        if (cxA < cxB)
                        {
                            a.X -= push;
                            b.X += push;
                        }
                        else
                        {
                            a.X += push;
                            b.X -= push;
                        }
                    }
                }
            }
        }

        float canvasW = GumService.Default.CanvasWidth;
        float canvasH = GumService.Default.CanvasHeight;
        if (canvasW <= 0f || canvasH <= 0f)
        {
            return;
        }

        foreach (LivePlacement p in placements)
        {
            p.X = MathHelper.Clamp(p.X, 4f, MathHelper.Max(4f, canvasW - p.W - 4f));
            p.Y = MathHelper.Clamp(p.Y, -400f, MathHelper.Max(-400f, canvasH - p.H - 4f));
        }
    }

    private static bool TryGetOverlap(LivePlacement a, LivePlacement b, out float overlapW, out float overlapH)
    {
        float interLeft = MathHelper.Max(a.X, b.X);
        float interTop = MathHelper.Max(a.Y, b.Y);
        float interRight = MathHelper.Min(a.X + a.W, b.X + b.W);
        float interBottom = MathHelper.Min(a.Y + a.H, b.Y + b.H);
        overlapW = interRight - interLeft;
        overlapH = interBottom - interTop;
        return overlapW > 0.5f && overlapH > 0.5f;
    }

    private sealed class LivePlacement(ToolTip tooltip, float x, float y, float w, float h)
    {
        public readonly ToolTip Tooltip = tooltip;
        public float X = x;
        public float Y = y;
        public readonly float W = w;
        public readonly float H = h;
    }

    /// <summary>Visible panel size from <see cref="ToolTip.Background"/>; matches authored nine-slice (~370×266).</summary>
    private static void GetTooltipPanelSize(ToolTip tooltip, out float width, out float height)
    {
        const float fallbackW = 370f;
        const float fallbackH = 266f;
        if (tooltip.Background != null)
        {
            width = tooltip.Background.GetAbsoluteWidth();
            height = tooltip.Background.GetAbsoluteHeight();
            if (width >= 1f && height >= 1f)
            {
                return;
            }
        }

        width = fallbackW;
        height = fallbackH;
    }

    private Vector2 ConvertScreenToGumCanvas(Vector2 screenPos)
    {
        float scale = game.GumViewportScale;
        if (scale <= 0f)
        {
            return screenPos;
        }

        var viewport = game.GraphicsDevice.Viewport;
        float gumCanvasWidth = GumService.Default.CanvasWidth;
        float gumCanvasHeight = GumService.Default.CanvasHeight;
        if (gumCanvasWidth <= 0f || gumCanvasHeight <= 0f)
        {
            return screenPos / scale;
        }

        float viewportOffsetX = (viewport.Width - gumCanvasWidth * scale) * 0.5f;
        float viewportOffsetY = (viewport.Height - gumCanvasHeight * scale) * 0.5f;

        return new Vector2(
            (screenPos.X - viewportOffsetX) / scale,
            (screenPos.Y - viewportOffsetY) / scale);
    }

    private void RemoveTooltip(ITooltipable item)
    {
        if (!activeTooltips.TryGetValue(item, out var tooltip)) return;

        GumService.Default.Root.Children.Remove(tooltip.Visual);
        activeTooltips.Remove(item);
    }

    /// <summary>
    /// Hides the category strip entirely when the item exposes no <see cref="ITooltipable.CategoryName"/>;
    /// otherwise sets the label and crops the icon out of the shared sprite sheet.
    /// </summary>
    private static void ApplyCategoryRow(ToolTip tooltip, ITooltipable item)
    {
        if (tooltip.ShopItemTypeInstance is not { } shopItemType) return;

        string categoryName = item.CategoryName;
        if (categoryName == null)
        {
            shopItemType.Visual.Visible = false;
            return;
        }

        shopItemType.Visual.Visible = true;
        shopItemType.ShopItemTypeName = categoryName;
        ApplyShopItemIcon(shopItemType, item.IconSourceRect);
    }

    /// <summary>
    /// Points the tooltip's category icon at <see cref="ShopItemIconAtlas.SheetFile"/>
    /// and crops to <paramref name="sourceRect"/>. When <paramref name="sourceRect"/> is null
    /// the icon sprite is hidden but the label stays visible.
    /// </summary>
    private static void ApplyShopItemIcon(ShopItemType shopItemType, Rectangle? sourceRect)
    {
        var sprite = shopItemType.SpriteInstance;
        if (sprite == null) return;

        if (sourceRect is not { } rect)
        {
            sprite.Visible = false;
            return;
        }

        sprite.Visible = true;
        sprite.SourceFileName = ShopItemIconAtlas.SheetFile;
        sprite.TextureAddress = Gum.Managers.TextureAddress.Custom;
        sprite.TextureLeft = rect.X;
        sprite.TextureTop = rect.Y;
        sprite.TextureWidth = rect.Width;
        sprite.TextureHeight = rect.Height;
    }

    /// <summary>
    /// One-time configuration of the interaction row: Xbox face icons from
    /// <see cref="XboxButtonAtlas"/> plus short labels sized to fit the buttons.
    /// </summary>
    private static void ConfigureInteractionButtons(ToolTip tooltip, ITooltipable item)
    {
        if (tooltip.ButtonWithIconInstance is not { } left ||
            tooltip.ButtonWithIconInstance1 is not { } right)
        {
            return;
        }

        const int compactFont = 17;
        left.TextInstance.FontSize = compactFont;
        right.TextInstance.FontSize = compactFont;

        // Default Gum state tints "cantAfford" red; only the credit cost uses that category.
        left.AffordabilityState = ButtonWithIcon.Affordability.canAfford;
        right.AffordabilityState = ButtonWithIcon.Affordability.canAfford;

        switch (item)
        {
            case BuyableStationWrapper buy:
                left.Visual.Visible = true;
                ApplyCoinCostIcon(left);
                left.ButtonText = $"{buy.Cost}";
                right.Visual.Visible = true;
                ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.X);
                right.ButtonText = "Buy";
                break;

            case SpeedLever:
                left.Visual.Visible = false;
                right.Visual.Visible = true;
                ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.X);
                right.ButtonText = "Speed";
                break;

            case Counter:
                left.Visual.Visible = false;
                right.Visual.Visible = true;
                ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.A);
                right.ButtonText = "Swap";
                break;

            case ResourceStation:
                left.Visual.Visible = false;
                right.Visual.Visible = true;
                ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.A);
                right.ButtonText = "Take";
                break;

            case CannonStation:
                left.Visual.Visible = true;
                right.Visual.Visible = true;
                ApplyFaceButtonIcon(left, XboxButtonAtlas.Face.Y);
                left.ButtonText = "Sit";
                ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.X);
                right.ButtonText = "Fire";
                break;

            case Workbench:
                ConfigureCraftingWorkbenchButtons(left, right);
                break;

            case AbstractStation station
                when GamelabGame.Instance?.StationRegistry.Get(station.StationId)?.ItemType == EItemType.Crafting:
                ConfigureCraftingWorkbenchButtons(left, right);
                break;

            default:
                if (item is AbstractStation)
                {
                    left.Visual.Visible = true;
                    right.Visual.Visible = true;
                    ApplyFaceButtonIcon(left, XboxButtonAtlas.Face.X);
                    ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.A);
                    left.ButtonText = "Use";
                    right.ButtonText = "Grab";
                }

                break;
        }
    }

    private static void ConfigureCraftingWorkbenchButtons(ButtonWithIcon left, ButtonWithIcon right)
    {
        left.Visual.Visible = true;
        right.Visual.Visible = true;
        ApplyFaceButtonIcon(left, XboxButtonAtlas.Face.X);
        left.ButtonText = "Interact";
        ApplyFaceButtonIcon(right, XboxButtonAtlas.Face.A);
        right.ButtonText = "Place";
    }

    private static void ApplyCoinCostIcon(ButtonWithIcon button) =>
        button.ButtonIcon = "spr_coin_32.png";

    private static void ApplyFaceButtonIcon(ButtonWithIcon button, XboxButtonAtlas.Face face)
    {
        Rectangle rect = XboxButtonAtlas.GetSourceRect(face);
        var sprite = button.SpriteInstance;
        sprite.SourceFileName = XboxButtonAtlas.SheetFile;
        sprite.TextureAddress = Gum.Managers.TextureAddress.Custom;
        sprite.TextureLeft = rect.X;
        sprite.TextureTop = rect.Y;
        sprite.TextureWidth = rect.Width;
        sprite.TextureHeight = rect.Height;
    }
    private void UpdateAffordabilityState(ITooltipable item, ToolTip tooltip)
    {
        if (tooltip.ButtonWithIconInstance is not { } costButton) return;
        if (item.Cost is not { } cost) return;

        costButton.AffordabilityState = game.CurrentRun.Credits >= cost
            ? ButtonWithIcon.Affordability.canAfford
            : ButtonWithIcon.Affordability.cantAfford;
    }

    public void SetAllVisible(bool isVisible)
    {
        foreach (var tooltip in activeTooltips.Values)
        {
            tooltip.Visual.Visible = isVisible;
        }
    }

    public void ClearAll()
    {
        foreach (var tooltip in activeTooltips.Values)
        {
            GumService.Default.Root.Children.Remove(tooltip.Visual);
        }

        activeTooltips.Clear();
    }
}
