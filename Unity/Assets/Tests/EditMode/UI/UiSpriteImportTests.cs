using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace Gamelab.Tests.UI
{
    public class UiSpriteImportTests
    {
        const string Root = "Assets/Resources/UI/Art";

        [Test]
        public void EveryUiPngHasSpriteSettings()
        {
            var files = Directory.GetFiles(Root, "*.png");
            Assert.AreEqual(33, files.Length);
            foreach (var f in files)
            {
                var path = f.Replace('\\', '/');
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.NotNull(ti, path);
                Assert.AreEqual(TextureImporterType.Sprite, ti.textureType, path);
                Assert.AreEqual(SpriteImportMode.Single, ti.spriteImportMode, path);
                Assert.AreEqual(UnityEngine.FilterMode.Point, ti.filterMode, path);
                Assert.AreEqual(TextureImporterCompression.Uncompressed, ti.textureCompression, path);
                Assert.IsFalse(ti.mipmapEnabled, path);
                Assert.AreEqual(8192, ti.maxTextureSize, path);
                var b = File.ReadAllBytes(path);
                int w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
                Assert.AreEqual((float)w, ti.spritePixelsPerUnit, path);
            }
        }

        [Test]
        public void EveryUiPngImportsAsFullRect()
        {
            foreach (var f in Directory.GetFiles(Root, "*.png"))
            {
                var path = f.Replace('\\', '/');
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                var ts = new TextureImporterSettings();
                ti.ReadTextureSettings(ts);
                Assert.AreEqual(UnityEngine.SpriteMeshType.FullRect, ts.spriteMeshType, path);
            }
        }

        [Test]
        public void FontsImportAsFonts()
        {
            foreach (var n in new[] { "UbuntuMono-Regular", "SpecialElite-Regular", "LibreBodoni" })
                Assert.NotNull(AssetDatabase.LoadAssetAtPath<UnityEngine.Font>($"Assets/Resources/UI/Fonts/{n}.ttf"), n);
        }
    }
}
