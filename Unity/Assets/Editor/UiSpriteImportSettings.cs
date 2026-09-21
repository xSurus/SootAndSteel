using System;
using UnityEditor;

namespace Gamelab.Editor
{
    /// <summary>
    /// Import settings for UI art under Assets/Resources/UI/Art/ (see CONVENTIONS.md "Sprite import").
    /// Point filter, no compression, no mipmaps, single sprite, PPU = pixel width.
    /// </summary>
    public class UiSpriteImportSettings : AssetPostprocessor
    {
        const string UiArtRoot = "Assets/Resources/UI/Art/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UiArtRoot, StringComparison.Ordinal)) return;
            var (w, _) = MapSpriteImportSettings.ReadPngSize(assetPath);
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = w;
            ti.spritePivot = new UnityEngine.Vector2(0.5f, 0.5f);
            ti.filterMode = UnityEngine.FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.sRGBTexture = true;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 8192;
        }
    }
}
