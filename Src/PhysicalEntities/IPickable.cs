using Gamelab.Map.Train.State;
using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IPickable : IPhysicalEntity
{
    void OnPickup(Player interactingPlayer, TrainContext trainContext)
    {
    }

    void OnPickupHeld(Player interactingPlayer, TrainContext trainContext, float dt)
    {
    }
}