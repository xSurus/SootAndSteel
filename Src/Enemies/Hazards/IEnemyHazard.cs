using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.Enemies.Hazards;

public interface IEnemyHazard : IPhysicalEntity, IUpdatable
{
    bool ShouldRemove { get; }
    bool CountsAsActiveThreat { get; }
    IEnemyHazard TryCreateHazard();
    void RemovePhysicsBody();
}