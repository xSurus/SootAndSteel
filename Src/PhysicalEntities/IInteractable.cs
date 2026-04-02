using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IInteractable : IPhysicalEntity
{
    void OnInteract(Player interactingPlayer)
    {
    }

    void OnInteractHeld(Player interactingPlayer, float dt)
    {
    }
}