using System;
using Gamelab.Enemies;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public class SpeedLever(Vector2 position)
    : AbstractStation("SpeedLever", Color.LightGreen, position)
{

    /// <summary>
    /// When set, interact calls this instead of cycling speed. Used by HubScreen for depart readying.
    /// </summary>
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
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(gameplayContext.State.CurrentSpeed);
        int nextIndex = (currentIndex + 1) % allSpeeds.Count;

        gameplayContext.State.CurrentSpeed = allSpeeds[nextIndex];
    }
}
