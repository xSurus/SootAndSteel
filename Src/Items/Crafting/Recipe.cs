using System.Collections.Generic;

namespace Gamelab.Items.Crafting;

public class Recipe(string outputItemId, float craftingTime, params string[] requiredItems)
{
    public string OutputItemId { get; } = outputItemId;
    public List<string> RequiredItemIds { get; } = [..requiredItems];
    public float CraftingTime { get; } = craftingTime;
}