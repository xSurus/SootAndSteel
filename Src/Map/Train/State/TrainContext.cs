namespace Gamelab.Map.Train.State;

public class TrainContext(TrainMap map, TrainState state)
{
    public TrainMap Map { get; } = map;
    public TrainState State { get; } = state;
}