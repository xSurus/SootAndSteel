using UnityEngine;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public enum SeatPosition
    {
        Bottom,
        Top
    }

    public interface ICannonSeat
    {
        Rigidbody2D PhysicsBody { get; }
        SeatPosition SeatPosition { get; }
        Vector2 DrawOffset { get; }
        void OnRelease(IPlayerActor player);
        void OnInteract(IPlayerActor player);

        void OnPickup(IPlayerActor player)
        {
        }
    }
}
