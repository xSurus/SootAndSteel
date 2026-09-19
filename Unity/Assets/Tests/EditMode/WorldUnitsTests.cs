using NUnit.Framework;
using System.Numerics;

namespace Gamelab.Tests
{
    public class WorldUnitsTests
    {
        [Test]
        public void PixelsPerMeter_Is100()
        {
            Assert.AreEqual(100f, WorldUnits.PixelsPerMeter);
        }

        [Test]
        public void FloatConversions_RoundTrip()
        {
            Assert.AreEqual(2.5f, WorldUnits.ToMeters(250f), 1e-5f);
            Assert.AreEqual(250f, WorldUnits.ToPixels(2.5f), 1e-4f);
            Assert.AreEqual(37f, WorldUnits.ToPixels(WorldUnits.ToMeters(37f)), 1e-4f);
        }

        [Test]
        public void VectorConversions_ScaleBothComponents()
        {
            Vector2 m = WorldUnits.ToMeters(new Vector2(150f, -50f));
            Assert.AreEqual(1.5f, m.X, 1e-5f);
            Assert.AreEqual(-0.5f, m.Y, 1e-5f);
            Vector2 p = WorldUnits.ToPixels(m);
            Assert.AreEqual(150f, p.X, 1e-4f);
            Assert.AreEqual(-50f, p.Y, 1e-4f);
        }
    }
}
