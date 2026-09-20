using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace Gamelab.Tests.Map
{
    public class MapSpriteImportTests
    {
        const string Root = "Assets/Resources/Map";

        // Literal pixel widths for everything that is not in the 100 px family.
        static readonly Dictionary<string, int> Widths = new Dictionary<string, int>
        {
            { "Rail_Tile_01", 233 }, { "Rail_Tile_02", 233 }, { "Hub", 3840 },
            { "Snow_Covered_Pine", 847 }, { "Snow_Covered_Pine_2", 847 },
            { "Wheels1", 222 }, { "Wheels2", 222 }, { "Wheels3", 222 },
            { "House1", 2048 }, { "House2", 1846 }, { "Village_Enter", 1092 },
            { "Stake", 271 }, { "Stake1", 383 }, { "Stake2", 383 }, { "Stake3", 383 },
            { "Vendor", 410 }, { "Town1", 410 },
        };

        [Test]
        public void EveryMapPngHasSpriteSettings()
        {
            var files = Directory.GetFiles(Root, "*.png", SearchOption.AllDirectories);
            Assert.IsNotEmpty(files);
            foreach (var f in files)
            {
                var path = f.Replace('\\', '/');
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.NotNull(ti, path);
                Assert.AreEqual(TextureImporterType.Sprite, ti.textureType, path);
                Assert.AreEqual(UnityEngine.FilterMode.Point, ti.filterMode, path);
                Assert.AreEqual(TextureImporterCompression.Uncompressed, ti.textureCompression, path);
                Assert.IsFalse(ti.mipmapEnabled, path);
                Assert.AreEqual(ExpectedPpu(path), ti.spritePixelsPerUnit, path);
            }
        }

        static float ExpectedPpu(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (path.StartsWith(Root + "/Walls/") || name.EndsWith("_Tile") || name.StartsWith("Train_Tile_")) return 100f;
            return Widths[name];
        }
    }
}
