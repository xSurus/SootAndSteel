using System.Numerics;
using NUnit.Framework;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    public class BulletTargetingMathTests
    {
        [Test]
        public void GetDistanceToLine_PointOnLine_ReturnsZero()
        {
            float distance = BulletTargetingMath.GetDistanceToLine(
                new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 0));

            Assert.AreEqual(0f, distance, 0.0001f);
        }

        [Test]
        public void GetDistanceToLine_PointBeforeSegmentStart_ReturnsMaxValue()
        {
            float distance = BulletTargetingMath.GetDistanceToLine(
                new Vector2(0, 0), new Vector2(10, 0), new Vector2(-5, 0));

            Assert.AreEqual(float.MaxValue, distance);
        }

        [Test]
        public void GetDistanceToLine_PointOffLine_ReturnsPerpendicularDistance()
        {
            float distance = BulletTargetingMath.GetDistanceToLine(
                new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 3));

            Assert.AreEqual(3f, distance, 0.0001f);
        }
    }
}
