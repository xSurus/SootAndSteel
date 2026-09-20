using Gamelab.Config;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.Stations
{
    public class SpeedLeverRuntimeTests
    {
        private sealed class FakeState : ITrainState
        {
            public bool VictoryLapActive { get; set; }
            public bool IsCoalOvenBurning { get; set; } = true;
            public TrainSpeedSetting CurrentSpeed { get; set; }
            public TrainSpeedSetting.Set Speeds;
            public int SlowDowns;

            // Src TrainState.SlowDownIfRunning: Stopped stays Stopped, otherwise Slow.
            public void SlowDownIfRunning()
            {
                SlowDowns++;
                if (CurrentSpeed != Speeds.Stopped) CurrentSpeed = Speeds.Slow;
            }
        }

        private GameObject go;
        private SpeedLeverRuntime lever;
        private FakeState state;
        private TrainSpeedSetting.Set speeds;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Lever");
            go.AddComponent<Rigidbody2D>();
            lever = go.AddComponent<SpeedLeverRuntime>();
            lever.Initialize(new StationCatalogEntry { stationId = "" });
            speeds = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
            state = new FakeState { Speeds = speeds, CurrentSpeed = speeds.Default };
            lever.State = state;
            lever.Speeds = speeds;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        private void Interact() => ((IInteractable)lever).OnInteract(null);

        [Test]
        public void StationId_Defaults() => Assert.AreEqual(StationIds.SpeedLever, lever.StationId);

        [Test]
        public void Cycles_SlowDefaultFast_ThenWraps()
        {
            state.CurrentSpeed = speeds.Slow;
            Interact();
            Assert.AreSame(speeds.Default, state.CurrentSpeed);
            Assert.AreEqual(1, lever.NextIndex);
            Interact();
            Assert.AreSame(speeds.Fast, state.CurrentSpeed);
            Interact();
            Assert.AreSame(speeds.Slow, state.CurrentSpeed);
            Assert.AreEqual(0, lever.NextIndex);
        }

        [Test]
        public void FromStopped_GoesToSlow()
        {
            state.CurrentSpeed = speeds.Stopped;
            Interact();
            Assert.AreSame(speeds.Slow, state.CurrentSpeed);
            Assert.AreEqual(0, lever.NextIndex);
        }

        [Test]
        public void VictoryLap_DoesNothing()
        {
            state.VictoryLapActive = true;
            state.IsCoalOvenBurning = false;
            Interact();
            Assert.AreSame(speeds.Default, state.CurrentSpeed);
            Assert.AreEqual(0, state.SlowDowns);
        }

        [Test]
        public void OvenOut_SlowsDownIfRunning_StoppedStaysStopped()
        {
            state.IsCoalOvenBurning = false;
            Interact();
            Assert.AreSame(speeds.Slow, state.CurrentSpeed);
            state.CurrentSpeed = speeds.Stopped;
            Interact();
            Assert.AreSame(speeds.Stopped, state.CurrentSpeed);
            Assert.AreEqual(2, state.SlowDowns);
        }

        [Test]
        public void Override_TakesPrecedence()
        {
            int calls = 0;
            lever.OnInteractOverride = _ => calls++;
            state.VictoryLapActive = true;
            Interact();
            Assert.AreEqual(1, calls);
            Assert.AreSame(speeds.Default, state.CurrentSpeed);
        }
    }
}
