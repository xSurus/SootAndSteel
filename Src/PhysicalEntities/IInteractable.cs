using Gamelab.Map.Train.State;
using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IInteractable : IPhysicalEntity
{
    void OnInteract(Player interactingPlayer, GameplayContext context)
    {
    }

    void OnInteractHeld(Player interactingPlayer, GameplayContext context, float dt)
    {
    }
}