using Gamelab.Config;
using Gamelab.Map.Train.State;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class TrainSpeedSettingTests
    {
        [Test]
        public void CreateSet_UsesEffectiveSrcValues()
        {
            var s = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
            Assert.AreEqual(("Stopped", 0f, 0f), (s.Stopped.Name, s.Stopped.TargetSpeed, s.Stopped.BurnMultiplier));
            Assert.AreEqual(("Slow", 150f, 0.5f), (s.Slow.Name, s.Slow.TargetSpeed, s.Slow.BurnMultiplier));
            Assert.AreEqual(("Default", 300f, 1f), (s.Default.Name, s.Default.TargetSpeed, s.Default.BurnMultiplier));
            Assert.AreEqual(("Fast", 600f, 4f), (s.Fast.Name, s.Fast.TargetSpeed, s.Fast.BurnMultiplier));
        }

        [Test]
        public void All_IsSlowDefaultFast_ByReference_WithoutStopped()
        {
            var s = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
            Assert.AreEqual(3, s.All.Count);
            Assert.AreSame(s.Slow, s.All[0]);
            Assert.AreSame(s.Default, s.All[1]);
            Assert.AreSame(s.Fast, s.All[2]);
        }

        [Test]
        public void Sets_AreIndependent()
        {
            var a = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
            var b = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
            Assert.AreNotSame(a.Slow, b.Slow);
        }

        [Test]
        public void CannonTuning_DefaultsMatchSrc()
        {
            var c = CannonTuning.Default;
            Assert.AreEqual(0.5f, c.CannonCooldown);
            Assert.AreEqual(6f, c.CannonRotationSpeed);
            Assert.AreEqual(80, c.TrainTileSize);
            Assert.AreEqual(0.25f, c.InputMovementDeadzoneSquared);
        }
    }
}
