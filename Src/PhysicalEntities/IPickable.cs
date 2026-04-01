using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IPickable : IPhysicalEntity
{
    void OnPickup(Player interactingPlayer)
    {
    }

    void OnPickupHeld(Player interactingPlayer, float dt)
    {
    }
}