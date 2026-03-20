using Gamelab.Map.Train.State;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Interactable;

public interface IInteractable
{
    void OnInteractAction(Player player, TrainContext context)
    {
    }

    void OnInteractActionHeld(Player player, TrainContext context, float dt)
    {
    }

    void OnGrabAction(Player interactingPlayer, TrainContext trainContext)
    {
    }

    void OnGrabActionHeld(Player interactingPlayer, TrainContext trainContext, float dt)
    {
    }

    Body PhysicsBody { get; }
}