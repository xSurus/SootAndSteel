using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.UI
{
    public class ScreensArtTests
    {
        [TestCase("spr_waybill_paper")]
        [TestCase("spr_waybill_punch")]
        [TestCase("spr_waybill_paid_stamp")]
        [TestCase("spr_incident_paper")]
        [TestCase("spr_filed_closed_stamp")]
        [TestCase("Silhouette")]
        [TestCase("IdleA0")]
        [TestCase("IdleA1")]
        [TestCase("IdleA2")]
        [TestCase("IdleA3")]
        public void Art_LoadsWithPngSize(string name)
        {
            // Read the size from the PNG IHDR chunk, independent of Unity's importer.
            var b = File.ReadAllBytes("Assets/Resources/UI/Art/" + name + ".png");
            int w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
            int h = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
            var s = Resources.Load<Sprite>("UI/Art/" + name);
            Assert.NotNull(s, name);
            Assert.AreEqual(new Rect(0, 0, w, h), s.rect, name);
            Assert.AreEqual(w, s.texture.width, name);
            Assert.AreEqual(h, s.texture.height, name);
        }
    }
}
