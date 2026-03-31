using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class Counter(Vector2 position, GameplayContext gameplayContext)
    : AbstractStation("Counter", Color.SaddleBrown, position, gameplayContext)
{
    public override void OnPickup(Player interactingPlayer, GameplayContext gameplayContext)
    {
        if (interactingPlayer.HeldItem == null || HeldItem == null)
        {
            (interactingPlayer.HeldItem, HeldItem) = (HeldItem, interactingPlayer.HeldItem);
        }
    }
}