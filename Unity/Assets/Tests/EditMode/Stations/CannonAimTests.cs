using System;
using System.Numerics;
using Gamelab.PhysicalEntities.Stations;
using NUnit.Framework;

namespace Gamelab.Tests.Stations
{
    public class CannonAimTests
    {
        private const float Pi = (float)Math.PI;

        [Test]
        public void WrapAngle_InRangeUnchanged_AndUpperEdgeIncluded()
        {
            Assert.AreEqual(1f, CannonAim.WrapAngle(1f), 1e-6f);
            Assert.AreEqual(Pi, CannonAim.WrapAngle(Pi), 1e-6f);
            Assert.AreEqual(0f, CannonAim.WrapAngle(0f), 1e-6f);
        }

        [Test]
        public void WrapAngle_LowerEdgeMapsToPi_AndWrapsBothWays()
        {
            Assert.AreEqual(Pi, CannonAim.WrapAngle(-Pi), 1e-5f);
            Assert.AreEqual(-Pi + 0.5f, CannonAim.WrapAngle(Pi + 0.5f), 1e-5f);
            Assert.AreEqual(Pi - 0.5f, CannonAim.WrapAngle(-Pi - 0.5f), 1e-5f);
            Assert.AreEqual(0.25f, CannonAim.WrapAngle(0.25f + 4 * Pi), 1e-4f);
        }

        [Test]
        public void Step_ClampsToRotationSpeedTimesDt()
        {
            float r = CannonAim.Step(0f, new Vector2(0, 1), 0.1f, 6f, 0.25f); // target pi/2, max 0.6
            Assert.AreEqual(0.6f, r, 1e-5f);
            float neg = CannonAim.Step(0f, new Vector2(0, -1), 0.1f, 6f, 0.25f);
            Assert.AreEqual(-0.6f, neg, 1e-5f);
        }

        [Test]
        public void Step_SnapsWhenWithinStep_AndTakesShortWayAcrossWrap()
        {
            float r = CannonAim.Step(1.5f, new Vector2(0, 1), 1f, 6f, 0.25f);
            Assert.AreEqual(Pi / 2f, r, 1e-5f);
            // current 3.0, target -3.0 (input pointing left/up-ish): short way is +0.283 upwards through pi
            float target = -3f;
            var input = new Vector2((float)Math.Cos(target), (float)Math.Sin(target));
            float w = CannonAim.Step(3f, input, 0.01f, 6f, 0.25f);
            Assert.AreEqual(3.06f, w, 1e-5f);
        }

        [Test]
        public void Step_DeadzoneReturnsCurrent_IncludingBoundary()
        {
            Assert.AreEqual(1f, CannonAim.Step(1f, new Vector2(0.5f, 0f), 1f, 6f, 0.25f)); // LengthSquared == deadzone
            Assert.AreEqual(1f, CannonAim.Step(1f, Vector2.Zero, 1f, 6f, 0.25f));
            Assert.AreNotEqual(1f, CannonAim.Step(1f, new Vector2(0.6f, 0f), 1f, 6f, 0.25f));
        }
    }
}
