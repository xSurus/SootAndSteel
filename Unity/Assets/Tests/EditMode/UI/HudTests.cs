using Gamelab.Levels;
using Gamelab.UI;
using NUnit.Framework;

namespace Gamelab.Tests.UI
{
    // Goldens come from evaluating the Src GameplayHud expressions in float32 with a script.
    public class HudTests
    {
        private const float Tol = 1e-4f;

        private static LevelDefinition Level(float distance, params float[] spawns)
        {
            var def = new LevelDefinition { LevelDistance = distance };
            foreach (var s in spawns) def.SpawnEvents.Add(new SpawnEvent { Distance = s });
            return def;
        }

        [Test]
        public void DistanceRatioClampsAndFloorsLevelDistance()
        {
            Assert.AreEqual(0.5f, HudMath.DistanceRatio(500f, 1000f), Tol);
            Assert.AreEqual(1f, HudMath.DistanceRatio(2000f, 1000f), Tol);
            Assert.AreEqual(0f, HudMath.DistanceRatio(-5f, 1000f), Tol);
            Assert.AreEqual(1f, HudMath.DistanceRatio(5f, 0f), Tol);
            Assert.AreEqual(1f, HudMath.DistanceRatio(5f, -10f), Tol);
            Assert.AreEqual(0.5f, HudMath.DistanceRatio(0.5f, 0f), Tol);
        }

        [Test]
        public void TrackTravelRange()
        {
            Assert.AreEqual(455.5f, HudMath.TrackTravelRange(503.5f), Tol);
            Assert.AreEqual(0f, HudMath.TrackTravelRange(1f), Tol);
            Assert.AreEqual(0f, HudMath.TrackTravelRange(30f), Tol);
        }

        [Test]
        public void SpeedRatioFloorsMaxSpeed()
        {
            Assert.AreEqual(0.5f, HudMath.SpeedRatio(150f, 300f), Tol);
            Assert.AreEqual(1f, HudMath.SpeedRatio(900f, 300f), Tol);
            Assert.AreEqual(0f, HudMath.SpeedRatio(-4f, 300f), Tol);
            Assert.AreEqual(1f, HudMath.SpeedRatio(5f, 0f), Tol);
            Assert.AreEqual(0.5f, HudMath.SpeedRatio(0.5f, 0f), Tol);
        }

        [TestCase(0f, 115.70990f)]
        [TestCase(150f, 57.018776f)]
        [TestCase(300f, 358.32764f)]
        [TestCase(450f, 299.63653f)]
        [TestCase(600f, 240.94540f)]
        [TestCase(700f, 240.94540f)]
        [TestCase(-50f, 115.70990f)]
        public void NeedleDegrees(float speed, float expected)
        {
            Assert.AreEqual(expected, HudMath.NeedleDegrees(speed), Tol);
        }

        // ratio, expected layers 1..3
        [TestCase(0.7f, 0f, 0f, 0f)]
        [TestCase(0.5f, 0.2857143f, 0f, 0f)]
        [TestCase(0.3f, 0.5714285f, 0f, 0f)]
        [TestCase(0.2f, 0.7142857f, 0.3333334f, 0f)]
        [TestCase(0.1f, 0.8571428f, 0.6666666f, 0f)]
        [TestCase(0.05f, 0.9285714f, 0.8333333f, 0.5f)]
        [TestCase(0f, 1f, 1f, 1f)]
        [TestCase(1f, 0f, 0f, 0f)]
        public void FrostOpacityThresholds(float ratio, float l1, float l2, float l3)
        {
            Assert.AreEqual(l1, HudMath.FrostOpacity(1, ratio), Tol);
            Assert.AreEqual(l2, HudMath.FrostOpacity(2, ratio), Tol);
            Assert.AreEqual(l3, HudMath.FrostOpacity(3, ratio), Tol);
        }

        [Test]
        public void DotRatioClamps()
        {
            Assert.AreEqual(0.25f, HudMath.DotRatio(750f, 3000f), Tol);
            Assert.AreEqual(1f, HudMath.DotRatio(9000f, 3000f), Tol);
            Assert.AreEqual(0f, HudMath.DotRatio(-1f, 3000f), Tol);
            Assert.AreEqual(1f, HudMath.DotRatio(9f, 0f), Tol);
        }

        [Test]
        public void InitialStateHasNoFrost()
        {
            var m = new HudModel(Level(3000f));
            Assert.AreEqual(1f, m.TemperatureRatio);
            Assert.AreEqual(0f, m.FrostOpacity1);
            Assert.AreEqual(0f, m.FrostOpacity2);
            Assert.AreEqual(0f, m.FrostOpacity3);
            Assert.AreEqual(0f, m.SmoothedSpeedRatio);
        }

        [Test]
        public void SmoothingStepsByPointOneFive()
        {
            var m = new HudModel(Level(3000f));
            float[] ratio = { 0.15f, 0.2775f, 0.385875f, 0.4779938f };
            float[] needle = { 80.49522f, 50.562744f, 25.12014f, 3.493927f };
            for (int i = 0; i < 4; i++)
            {
                m.Update(0f, 600f, 10f, 10f, 600f);
                Assert.AreEqual(ratio[i], m.SmoothedSpeedRatio, Tol);
                Assert.AreEqual(needle[i], m.NeedleDegrees, Tol);
            }
            for (int i = 0; i < 100; i++) m.Update(0f, 600f, 10f, 10f, 600f);
            Assert.AreEqual(1f, m.SmoothedSpeedRatio, Tol);
            Assert.AreEqual(240.9454f, m.NeedleDegrees, 1e-2f);
        }

        [Test]
        public void UpdateComputesDistanceAndTemperature()
        {
            var m = new HudModel(Level(2000f));
            m.Update(500f, 0f, 5f, 20f, 300f);
            Assert.AreEqual(0.25f, m.DistanceRatio, Tol);
            Assert.AreEqual(0.25f, m.TemperatureRatio, Tol);
            Assert.AreEqual(0.6428571f, m.FrostOpacity1, Tol);
            m.Update(-10f, 0f, -5f, 0f, 300f);
            Assert.AreEqual(0f, m.DistanceRatio, Tol);
            Assert.AreEqual(0f, m.TemperatureRatio, Tol);
            Assert.AreEqual(1f, m.FrostOpacity3, Tol);
        }

        [Test]
        public void NullLevelUsesDistanceOne()
        {
            var m = new HudModel(null);
            m.Update(5f, 0f, 1f, 1f, 300f);
            Assert.AreEqual(1f, m.DistanceRatio, Tol);
            Assert.AreEqual(0, m.DotRatios.Count);
            m.SetLevel(null);
            Assert.AreEqual(0, m.DotRatios.Count);
        }

        [Test]
        public void SetLevelBumpsVersionAndBuildsDots()
        {
            var m = new HudModel(Level(1000f, 250f, 5000f));
            int v = m.LevelVersion;
            Assert.AreEqual(2, m.DotRatios.Count);
            Assert.AreEqual(0.25f, m.DotRatios[0], Tol);
            Assert.AreEqual(1f, m.DotRatios[1], Tol);
            m.SetLevel(Level(1000f, 1f, 2f, 3f));
            Assert.AreEqual(v + 1, m.LevelVersion);
            Assert.AreEqual(3, m.DotRatios.Count);
        }
    }
}
