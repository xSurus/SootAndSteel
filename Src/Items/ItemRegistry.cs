using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gamelab.Items;

public static class ItemRegistry
{
    private static readonly Dictionary<string, ItemDefinition> Definitions = new();

    public static void Initialize()
    {
        Register(new ItemDefinition("Coal", Color.Black));
        Register(new ItemDefinition("Bullet", Color.LightGray));
    }

    private static void Register(ItemDefinition def)
    {
        Definitions[def.Id] = def;
    }

    public static ItemDefinition Get(string id)
    {
        if (Definitions.TryGetValue(id, out var def)) return def;
        throw new Exception($"Item ID '{id}' does not exist in the registry!");
    }
}
