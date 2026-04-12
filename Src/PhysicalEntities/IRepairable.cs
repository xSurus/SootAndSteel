using Gamelab.Entities;

namespace Gamelab.PhysicalEntities;

public interface IRepairable : IDamageable
{
    bool IsBroken { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
    void Repair(float amount);
}
