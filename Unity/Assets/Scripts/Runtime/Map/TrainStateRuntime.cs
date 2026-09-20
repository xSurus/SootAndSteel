using Gamelab.Config;
using UnityEngine;

namespace Gamelab.Map.Train.State
{
    /// <summary>Owns the shared TrainSpeedSetting.Set and the Core TrainState, and ticks it.</summary>
    public sealed class TrainStateRuntime : MonoBehaviour, ITrainState
    {
        public TrainSpeedSetting.Set Speeds { get; } = TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default);
        public TrainState State { get; }

        public TrainStateRuntime()
        {
            State = new TrainState(Speeds, TrainStateTuning.Default);
        }

        public bool VictoryLapActive => State.VictoryLapActive;
        public bool IsCoalOvenBurning => State.IsCoalOvenBurning;
        public TrainSpeedSetting CurrentSpeed { get => State.CurrentSpeed; set => State.CurrentSpeed = value; }
        public void SlowDownIfRunning() => State.SlowDownIfRunning();

        public void Tick(float dt) => State.Update(dt);

        /// <summary>Set false when another owner (for example a pause or test) calls Tick.</summary>
        public bool SelfTick = true;

        private void Update() { if (SelfTick) Tick(Time.deltaTime); }
    }
}
