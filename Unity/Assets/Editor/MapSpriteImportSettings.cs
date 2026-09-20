using System;
using System.IO;
using UnityEditor;

namespace Gamelab.Editor
{
    /// <summary>
    /// Import settings for map art under Assets/Resources/Map/ (see CONVENTIONS.md "Sprite import").
    /// Point filter, no compression, no mipmaps, single sprite, pivot center.
    /// Pixels Per Unit is the sheet's tile width: the PNG's own width, except for the
    /// 100 px tile family (see PpuFor).
    /// </summary>
    public class MapSpriteImportSettings : AssetPostprocessor
    {
        const string MapRoot = "Assets/Resources/Map/";

        /// <summary>
        /// PPU rule. Floor tiles and all wall pieces share the 100 px tile scale in Src,
        /// including the narrow wall strips. Everything else (rails, pines, houses, NPCs)
        /// uses its own pixel width.
        /// </summary>
        public static int PpuFor(string assetPath, int pixelWidth)
        {
            if (assetPath.StartsWith(MapRoot + "Walls/", StringComparison.Ordinal)) return 100;
            switch (Path.GetFileNameWithoutExtension(assetPath))
            {
                case "Ice_Tile":
                case "Snow_Tile":
                case "Train_Tile_A":
                case "Train_Tile_B":
                    return 100;
                default:
                    return pixelWidth;
            }
        }

        /// <summary>Reads (width, height) from the PNG IHDR chunk (big-endian at byte 16 and 20).</summary>
        public static (int width, int height) ReadPngSize(string path)
        {
            var b = new byte[24];
            using (var s = File.OpenRead(path))
            {
                if (s.Read(b, 0, 24) < 24) throw new IOException("Not a PNG: " + path);
            }
            int w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
            int h = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
            return (w, h);
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(MapRoot, StringComparison.Ordinal)) return;
            var (w, _) = ReadPngSize(assetPath);
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = PpuFor(assetPath, w);
            ti.spritePivot = new UnityEngine.Vector2(0.5f, 0.5f);
            ti.filterMode = UnityEngine.FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.sRGBTexture = true;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 8192;
            var ts = new TextureImporterSettings();
            ti.ReadTextureSettings(ts);
            ts.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
            ts.spriteAlignment = (int)UnityEngine.SpriteAlignment.Center;
            ti.SetTextureSettings(ts);
        }
    }
}
