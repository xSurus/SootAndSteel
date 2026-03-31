using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class SpeedLever(Vector2 position, GameplayContext gameplayContext)
    : AbstractStation("SpeedLever", Color.LightGreen, position, gameplayContext)
{
    public override void OnInteract(Player interactingPlayer, GameplayContext gameplayContext)
    {
        if (!gameplayContext.State.IsCoalOvenBurning)
        {
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(gameplayContext.State.CurrentSpeed);
        int nextIndex = (currentIndex + 1) % allSpeeds.Count;

        gameplayContext.State.CurrentSpeed = allSpeeds[nextIndex];
    }
}