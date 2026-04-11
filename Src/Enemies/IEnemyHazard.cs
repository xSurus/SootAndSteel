using Gamelab.PhysicalEntities;

namespace Gamelab.Enemies;

public interface IEnemyHazard : IPhysicalEntity, IUpdatable
{
    bool ShouldRemove { get; }
    bool CountsAsActiveThreat { get; }
    void RemovePhysicsBody();
}
