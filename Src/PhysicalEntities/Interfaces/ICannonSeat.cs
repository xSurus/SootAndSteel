using Gamelab.Players;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface ICannonSeat
{
    Body PhysicsBody { get; }
    void OnRelease(Player player);
    void OnInteract(Player player);
    void OnPickup(Player player) { }
}
