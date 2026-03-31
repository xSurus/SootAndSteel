using System;
using System.Collections.Generic;

namespace Gamelab.Enemies;

public class EnemySlotManager(Random random)
{
    private static readonly EnemyTrainSlot[] ShooterSlots =
    [
        new(EnemySlotSide.Top, 0.15f),
        new(EnemySlotSide.Top, 0.5f),
        new(EnemySlotSide.Top, 0.85f),
        new(EnemySlotSide.Bottom, 0.15f),
        new(EnemySlotSide.Bottom, 0.5f),
        new(EnemySlotSide.Bottom, 0.85f)
    ];

    private static readonly EnemyTrainSlot[] ThiefSlots =
    [
        new(EnemySlotSide.Top, 0.25f),
        new(EnemySlotSide.Top, 0.75f),
        new(EnemySlotSide.Bottom, 0.25f),
        new(EnemySlotSide.Bottom, 0.75f)
    ];

    private readonly Random random = random;
    private readonly HashSet<EnemyTrainSlot> occupiedSlots = [];

    public bool TryReserveShooterSlot(out EnemyTrainSlot slot)
    {
        return TryReserveSlot(ShooterSlots, out slot);
    }

    public bool TryReserveThiefSlot(out EnemyTrainSlot slot)
    {
        return TryReserveSlot(ThiefSlots, out slot);
    }

    public bool TryReserveShooterSlotOnSide(EnemySlotSide side, out EnemyTrainSlot slot)
    {
        return TryReserveSlotOnSide(ShooterSlots, side, out slot);
    }

    public bool TryReserveThiefSlotOnSide(EnemySlotSide side, out EnemyTrainSlot slot)
    {
        return TryReserveSlotOnSide(ThiefSlots, side, out slot);
    }

    public void ReleaseSlot(EnemyTrainSlot slot)
    {
        occupiedSlots.Remove(slot);
    }

    public void Clear()
    {
        occupiedSlots.Clear();
    }

    private bool TryReserveSlot(IReadOnlyList<EnemyTrainSlot> candidateSlots, out EnemyTrainSlot slot)
    {
        List<EnemyTrainSlot> availableSlots = [];

        foreach (EnemyTrainSlot candidate in candidateSlots)
        {
            if (!occupiedSlots.Contains(candidate))
            {
                availableSlots.Add(candidate);
            }
        }

        if (availableSlots.Count == 0)
        {
            slot = default;
            return false;
        }

        slot = availableSlots[random.Next(availableSlots.Count)];
        occupiedSlots.Add(slot);
        return true;
    }

    private bool TryReserveSlotOnSide(IReadOnlyList<EnemyTrainSlot> candidateSlots, EnemySlotSide side, out EnemyTrainSlot slot)
    {
        List<EnemyTrainSlot> availableSlots = [];

        foreach (EnemyTrainSlot candidate in candidateSlots)
        {
            if (candidate.Side == side && !occupiedSlots.Contains(candidate))
            {
                availableSlots.Add(candidate);
            }
        }

        if (availableSlots.Count == 0)
        {
            return TryReserveSlot(candidateSlots, out slot);
        }

        slot = availableSlots[random.Next(availableSlots.Count)];
        occupiedSlots.Add(slot);
        return true;
    }
}
