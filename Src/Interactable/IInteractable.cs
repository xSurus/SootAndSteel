using Gamelab.Map.Train.State;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Interactable;

public interface IInteractable
{
    void Interact(Player interactingPlayer, TrainContext trainContext);

    void HoldInteract(Player interactingPlayer, TrainContext trainContext, float dt);

    Body PhysicsBody { get; }
}