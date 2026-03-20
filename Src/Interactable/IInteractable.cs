using Gamelab.Map.Train.State;
using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Interactable;

public interface IInteractable
{
    void OnPrimaryAction(Player player, TrainContext context)
    {
    }

    void OnPrimaryActionHeld(Player player, TrainContext context, float dt)
    {
    }

    void OnCarryAction(Player interactingPlayer, TrainContext trainContext)
    {
    }

    void OnCarryActionHeld(Player interactingPlayer, TrainContext trainContext, float dt)
    {
    }

    Body PhysicsBody { get; }
}