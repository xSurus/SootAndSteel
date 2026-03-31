using System.Collections.Generic;

namespace Gamelab.Utils;

public static class CraftingUtility
{
    public static bool IsValidPartialRecipe(List<string> current, List<string> required)
    {
        List<string> requiredCopy = new List<string>(required);

        foreach (string item in current)
        {
            if (!requiredCopy.Remove(item)) return false;
        }

        return true;
    }

    public static bool IsCompleteMatch(List<string> current, List<string> required)
    {
        if (current.Count != required.Count) return false;
        return IsValidPartialRecipe(current, required);
    }
}