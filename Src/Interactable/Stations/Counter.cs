using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Interactable.Stations;

public class Counter() : AbstractStation("Counter", Color.SaddleBrown)
{
    public override void OnGrabAction(Player interactingPlayer, TrainContext trainContext)
    {
        if (interactingPlayer.HeldItem == null || HeldItem == null)
        {
            (interactingPlayer.HeldItem, HeldItem) = (HeldItem, interactingPlayer.HeldItem);
        }
    }
}