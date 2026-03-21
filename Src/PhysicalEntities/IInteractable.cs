using Gamelab.Map.Train.State;
using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IInteractable : IPhysicalEntity
{
    void OnInteract(Player interactingPlayer, TrainContext context)
    {
    }

    void OnInteractHeld(Player interactingPlayer, TrainContext context, float dt)
    {
    }
}