using Gamelab.Map.Train.State;
using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IPickable : IPhysicalEntity
{
    void OnPickup(Player interactingPlayer, GameplayContext gameplayContext)
    {
    }

    void OnPickupHeld(Player interactingPlayer, GameplayContext gameplayContext, float dt)
    {
    }
}