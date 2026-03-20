using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Interactable.Stations;

public class SpeedLever() : AbstractStation("SpeedLever", Color.LightGreen)
{
    public override void OnInteractAction(Player interactingPlayer, TrainContext trainContext)
    {
        if (!trainContext.State.IsCoalOvenBurning)
        {
            trainContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(trainContext.State.CurrentSpeed);
        int nextIndex = (currentIndex + 1) % allSpeeds.Count;

        trainContext.State.CurrentSpeed = allSpeeds[nextIndex];
    }
}