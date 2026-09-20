namespace Gamelab.Map.Train.State
{
    // Slice of Src TrainState used by SpeedLever. Wave B's TrainState implements it.
    // SlowDownIfRunning must keep the Src semantics: Stopped stays Stopped, any other speed
    // becomes Slow. CurrentSpeed values are members of the one shared TrainSpeedSetting.Set.
    public interface ITrainState
    {
        bool VictoryLapActive { get; }
        bool IsCoalOvenBurning { get; }
        TrainSpeedSetting CurrentSpeed { get; set; }
        void SlowDownIfRunning();
    }
}
