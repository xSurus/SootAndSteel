using System;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class SpeedLever(Vector2 position)
    : AbstractStation(StationIds.SpeedLever, position)
{
    public Action<Player> OnInteractOverride { get; set; }

    public override void OnInteract(Player interactingPlayer)
    {
        if (OnInteractOverride != null)
        {
            OnInteractOverride(interactingPlayer);
            return;
        }

        if (gameplayContext.State.VictoryLapActive)
        {
            return;
        }

        if (!gameplayContext.State.IsCoalOvenBurning)
        {
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Slow;
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(gameplayContext.State.CurrentSpeed);
        int nextIndex = (currentIndex + 1) % allSpeeds.Count;

        gameplayContext.State.CurrentSpeed = allSpeeds[nextIndex];
    }
}