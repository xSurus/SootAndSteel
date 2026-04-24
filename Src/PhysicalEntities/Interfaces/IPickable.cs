using Gamelab.Players;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface IPickable : IPhysicalEntity
{
    void OnPickup(Player interactingPlayer)
    {
    }

    void OnPickupHeld(Player interactingPlayer, float dt)
    {
    }
}