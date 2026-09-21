using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.UI
{
    public class HubArtTests
    {
        [Test]
        public void GlyphX_CropsThirdCell()
        {
            var s = UiSpriteCrop.Glyph(XboxButtonAtlas.Face.X);
            Assert.NotNull(s);
            Assert.AreEqual(new Rect(64, 0, 32, 32), s.rect);
            Assert.AreEqual(32f, s.pixelsPerUnit);
            Assert.AreEqual(XboxButtonAtlas.SheetFile.Replace(".png", ""), s.texture.name);
            Assert.AreEqual(s.texture, Resources.Load<Texture2D>("UI/Art/xbox_buttons_spritesheet"));
        }

        [Test]
        public void Crops_AreCached()
        {
            Assert.AreSame(UiSpriteCrop.Glyph(XboxButtonAtlas.Face.B), UiSpriteCrop.Glyph(XboxButtonAtlas.Face.B));
        }

        [Test]
        public void MissingArt_ReturnsNull()
        {
            Assert.IsNull(UiSpriteCrop.Get("nope.png", new SpriteRect(0, 0, 4, 4)));
        }

        [TestCase("BasicCasing")]
        [TestCase("BasicProjectile")]
        [TestCase("BasicPropellant")]
        [TestCase("IdleA0")]
        [TestCase("IdleA1")]
        [TestCase("IdleA2")]
        [TestCase("IdleA3")]
        [TestCase("tooltip_base_9slice_192x192")]
        public void Art_LoadsAsSprite(string name)
        {
            Assert.NotNull(Resources.Load<Sprite>("UI/Art/" + name), name);
        }
    }
}
