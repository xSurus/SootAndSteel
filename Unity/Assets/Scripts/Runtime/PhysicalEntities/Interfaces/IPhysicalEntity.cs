using UnityEngine;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IPhysicalEntity
    {
        Rigidbody2D PhysicsBody { get; }
        Vector2 Position { get; set; }
    }
}
