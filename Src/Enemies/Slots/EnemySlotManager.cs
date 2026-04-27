using System;
using System.Collections.Generic;

namespace Gamelab.Enemies.Slots;

public class EnemySlotManager()
{
    private static readonly EnemyTrainSlot[] SideAttackSlots =
    [
        new(EnemySlotSide.Top, 0.15f),
        new(EnemySlotSide.Top, 0.5f),
        new(EnemySlotSide.Top, 0.85f),
        new(EnemySlotSide.Bottom, 0.15f),
        new(EnemySlotSide.Bottom, 0.5f),
        new(EnemySlotSide.Bottom, 0.85f)
    ];

    private readonly Random random = Random.Shared;
    private readonly HashSet<EnemyTrainSlot> occupiedSlots = [];

    public bool TryReserveSideAttackSlot(out EnemyTrainSlot slot)
    {
        return TryReserveSlot(SideAttackSlots, out slot);
    }

    public bool TryReserveSideAttackSlotOnSide(EnemySlotSide side, out EnemyTrainSlot slot)
    {
        return TryReserveSlotOnSide(SideAttackSlots, side, out slot);
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

    private bool TryReserveSlotOnSide(IReadOnlyList<EnemyTrainSlot> candidateSlots, EnemySlotSide side,
        out EnemyTrainSlot slot)
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
