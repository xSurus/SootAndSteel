using Gamelab.Players;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface ICannonSeat
{
    Body PhysicsBody { get; }
    Vector2 DrawOffset => Vector2.Zero;
    void OnRelease(Player player);
    void OnInteract(Player player);
    void OnPickup(Player player) { }
}
