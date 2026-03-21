using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class Counter(Vector2 position, TrainContext trainContext)
    : AbstractStation("Counter", Color.SaddleBrown, position, trainContext)
{
    public override void OnPickup(Player interactingPlayer, TrainContext trainContext)
    {
        if (interactingPlayer.HeldItem == null || HeldItem == null)
        {
            (interactingPlayer.HeldItem, HeldItem) = (HeldItem, interactingPlayer.HeldItem);
        }
    }
}