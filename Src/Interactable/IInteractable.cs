using Gamelab.Map.Train.State;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Interactable;

public interface IInteractable
{
    void OnInteract(Player interactingPlayer, TrainContext context)
    {
    }

    void OnInteractHeld(Player interactingPlayer, TrainContext context, float dt)
    {
    }

    void OnGrab(Player interactingPlayer, TrainContext trainContext)
    {
    }

    void OnGrabHeld(Player interactingPlayer, TrainContext trainContext, float dt)
    {
    }

    Body PhysicsBody { get; }
}