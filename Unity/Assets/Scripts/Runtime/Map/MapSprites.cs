using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamelab.Map
{
    /// <summary>Loads map sprites from Resources/Map, e.g. "Train_Tile_A" or "Walls/WallTileTop".</summary>
    public static class MapSprites
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string name)
        {
            if (cache.TryGetValue(name, out Sprite s) && s != null) return s;
            s = Resources.Load<Sprite>("Map/" + name);
            if (s == null) throw new InvalidOperationException("Map sprite not found: Resources/Map/" + name);
            cache[name] = s;
            return s;
        }
    }
}
