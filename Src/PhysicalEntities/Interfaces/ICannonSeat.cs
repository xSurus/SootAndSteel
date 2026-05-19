using Gamelab.Players;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Interfaces;

public enum SeatPosition
{
    Bottom,
    Top
}

public interface ICannonSeat
{
    Body PhysicsBody { get; }
    SeatPosition SeatPosition { get; }
    Vector2 DrawOffset { get; }
    void OnRelease(Player player);
    void OnInteract(Player player);

    void OnPickup(Player player)
    {
    }
}