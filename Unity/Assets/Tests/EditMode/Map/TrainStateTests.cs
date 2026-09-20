using Gamelab.Config;
using Gamelab.Map.Train.State;
using NUnit.Framework;

namespace Gamelab.Tests.Map
{
    public class TrainStateTests
    {
        private TrainSpeedSetting.Set speeds;
        private TrainState state;

        [SetUp]
        public void SetUp()
        {
            speeds = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
            state = new TrainState(speeds, TrainStateTuning.Default);
        }

        [Test]
        public void Initial_State()
        {
            Assert.AreSame(speeds.Default, state.CurrentSpeed);
            Assert.AreEqual(300f, state.actualSpeed);
            Assert.AreEqual(100f, state.Temperature);
            Assert.IsTrue(state.IsCoalOvenBurning);
            Assert.IsTrue(state.FuelBurningEnabled);
            Assert.IsFalse(state.VictoryLapActive);
            Assert.AreEqual(1f, state.MaintenanceScale);
        }

        [Test]
        public void Accelerates_At50PerSecond_AndStopsExactlyAtTarget()
        {
            state.CurrentSpeed = speeds.Fast;
            state.Update(1f);
            Assert.AreEqual(350f, state.actualSpeed, 1e-4f);
            state.Update(100f);
            Assert.AreEqual(600f, state.actualSpeed);
        }

        [Test]
        public void Decelerates_AndStopsExactlyAtTarget()
        {
            state.CurrentSpeed = speeds.Slow;
            state.Update(1f);
            Assert.AreEqual(250f, state.actualSpeed, 1e-4f);
            state.Update(100f);
            Assert.AreEqual(150f, state.actualSpeed);
        }

        [Test]
        public void Distance_IntegratesActualSpeed()
        {
            state.Update(2f);
            Assert.AreEqual(600f, state.DistanceTraveled, 1e-3f);
        }

        [Test]
        public void SpeedChanged_FiresOncePerChange()
        {
            int n = 0;
            TrainSpeedSetting last = null;
            state.OnSpeedChanged += s => { n++; last = s; };
            state.CurrentSpeed = speeds.Default;
            Assert.AreEqual(0, n);
            state.CurrentSpeed = speeds.Fast;
            state.CurrentSpeed = speeds.Fast;
            Assert.AreEqual(1, n);
            Assert.AreSame(speeds.Fast, last);
        }

        [Test]
        public void SlowDownIfRunning_SlowsUnlessStopped()
        {
            state.SlowDownIfRunning();
            Assert.AreSame(speeds.Slow, state.CurrentSpeed);
            state.CurrentSpeed = speeds.Stopped;
            state.SlowDownIfRunning();
            Assert.AreSame(speeds.Stopped, state.CurrentSpeed);
        }

        [Test]
        public void BreachedWalls_DecreaseTemperature_ScaledByMaintenanceAndDecreaseScale()
        {
            state.numberBreachedWalls = 2;
            state.Update(1f);
            Assert.AreEqual(94f, state.Temperature, 1e-4f);

            state.Temperature = 100f;
            state.ConfigurePlayerScaling(1.35f);
            state.TemperatureDecreaseScale = 2f;
            state.Update(1f);
            Assert.AreEqual(100f - 3f * 1.35f * 2f * 2f, state.Temperature, 1e-4f);
        }

        [Test]
        public void EngineOff_Decreases_AndDoesNotIncrease()
        {
            state.IsCoalOvenBurning = false;
            state.Temperature = 50f;
            state.Update(2f);
            Assert.AreEqual(44f, state.Temperature, 1e-4f);
        }

        [Test]
        public void Increase_OnlyWhenBurningAndNoBreaches()
        {
            state.Temperature = 50f;
            state.Update(1f);
            Assert.AreEqual(56f, state.Temperature, 1e-4f);

            state.Temperature = 50f;
            state.numberBreachedWalls = 1;
            state.Update(1f);
            Assert.AreEqual(47f, state.Temperature, 1e-4f);
        }

        [Test]
        public void Temperature_ClampsAtMax_AndFreezesAtZero()
        {
            state.IncreaseTemperature(50f);
            Assert.AreEqual(100f, state.Temperature);
            int frozen = 0;
            state.OnTrainFrozen += () => frozen++;
            state.DecreaseTemperature(40f);
            Assert.AreEqual(0, frozen);
            state.DecreaseTemperature(1000f);
            Assert.AreEqual(0f, state.Temperature);
            Assert.AreEqual(1, frozen);
        }

        [Test]
        public void MaintenanceScale_Formula()
        {
            Assert.AreEqual(1f, Gamelab.Levels.PlayerCountScaling.GetMaintenanceScale(0));
            Assert.AreEqual(1f, Gamelab.Levels.PlayerCountScaling.GetMaintenanceScale(1));
            Assert.AreEqual(1.7f, Gamelab.Levels.PlayerCountScaling.GetMaintenanceScale(3), 1e-5f);
            Assert.AreEqual(2.05f, Gamelab.Levels.PlayerCountScaling.GetMaintenanceScale(9), 1e-5f);
        }
    }
}
