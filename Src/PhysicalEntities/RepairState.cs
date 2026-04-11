using System;

namespace Gamelab.PhysicalEntities;

public class RepairState(float maxHealth)
{
    public float MaxHealth { get; } = maxHealth;
    public float CurrentHealth { get; private set; } = maxHealth;
    public bool IsBroken => CurrentHealth <= 0f;

    public bool ApplyDamage(float amount)
    {
        if (amount <= 0f || IsBroken)
        {
            return false;
        }

        float before = CurrentHealth;
        CurrentHealth = Math.Max(0f, CurrentHealth - amount);
        return before > 0f && CurrentHealth <= 0f;
    }

    public bool Repair(float amount)
    {
        if (amount <= 0f || CurrentHealth >= MaxHealth)
        {
            return false;
        }

        float before = CurrentHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        return before < MaxHealth && CurrentHealth >= MaxHealth;
    }
}
