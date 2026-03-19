using Gamelab.Map.Train.State;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Interactable;

public interface IInteractable
{
    void Interact(Player interactingPlayer, TrainContext trainContext);

    Body PhysicsBody { get; }
}