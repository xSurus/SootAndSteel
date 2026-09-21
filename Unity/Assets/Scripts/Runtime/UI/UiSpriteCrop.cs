using System.Collections.Generic;
using System.IO;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.UI.Runtime
{
    /// <summary>
    /// Cuts source-rect sprites out of the sheets under Resources/UI/Art. Src rects have a top-left
    /// origin, Unity's have a bottom-left one, so Y is flipped here.
    /// </summary>
    public static class UiSpriteCrop
    {
        // Lives for the domain lifetime (never cleared); the art set is small and fixed.
        private static readonly Dictionary<(string, int, int, int, int), Sprite> cache =
            new Dictionary<(string, int, int, int, int), Sprite>();

        /// <summary>Returns null when the texture is missing.</summary>
        public static Sprite Get(string artName, SpriteRect rect)
        {
            var key = (artName, rect.X, rect.Y, rect.Width, rect.Height);
            if (cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = Resources.Load<Texture2D>("UI/Art/" + Path.GetFileNameWithoutExtension(artName));
            if (tex == null) return null;
            var sprite = Sprite.Create(tex, new Rect(rect.X, tex.height - rect.Y - rect.Height, rect.Width, rect.Height),
                new Vector2(0.5f, 0.5f), rect.Width);
            cache[key] = sprite;
            return sprite;
        }

        public static Sprite Glyph(XboxButtonAtlas.Face face) =>
            Get(XboxButtonAtlas.SheetFile, XboxButtonAtlas.GetSourceRect(face));
    }
}
