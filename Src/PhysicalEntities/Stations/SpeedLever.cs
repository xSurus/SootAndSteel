using System;
using FmodForFoxes.Studio;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class SpeedLever
    : AbstractStation, IInteractable
{
    public Action<Player> OnInteractOverride { get; set; }

    private int nextIndex;
    private EventInstance leverSound;

    public SpeedLever(Vector2 position) : base(StationIds.SpeedLever, position)
    {
        soundService.LoadSound(Sounds.SpeedChange);
        leverSound = soundService.GetSoundInstance(Sounds.SpeedChange);
        soundService.RegisterParameter(leverSound, "New Speed Setting", () => nextIndex);
    }

    public void OnInteract(Player interactingPlayer)
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
            gameplayContext.State.SlowDownIfRunning();
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(gameplayContext.State.CurrentSpeed);
        nextIndex = (currentIndex + 1) % allSpeeds.Count;

        gameplayContext.State.CurrentSpeed = allSpeeds[nextIndex];
        leverSound.Start();
    }
}