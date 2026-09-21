using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.UI
{
    public class HudArtTests
    {
        [TestCase("GaugeDistance", 4846, 1000)]
        [TestCase("GaugeHand", 1000, 1000)]
        [TestCase("spr_hud_train_marker", 32, 28)]
        [TestCase("spr_hud_goal_x", 100, 100)]
        [TestCase("spr_hud_enemy", 100, 100)]
        [TestCase("FrostScreen1", 1920, 1080)]
        [TestCase("FrostScreen2", 1920, 1080)]
        [TestCase("FrostScreen3", 1920, 1080)]
        public void Art_LoadsWithSourceSize(string name, int w, int h)
        {
            var s = Resources.Load<Sprite>("UI/Art/" + name);
            Assert.NotNull(s, name);
            Assert.AreEqual(w, s.texture.width, name);
            Assert.AreEqual(h, s.texture.height, name);
        }
    }
}
