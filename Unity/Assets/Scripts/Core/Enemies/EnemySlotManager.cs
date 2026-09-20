using System;
using System.Collections.Generic;

namespace Gamelab.Enemies.Core
{
    public class EnemySlotManager
    {
        private static readonly EnemyTrainSlot[] SideAttackSlots =
        {
            new EnemyTrainSlot(EnemySlotSide.Top, 0.15f),
            new EnemyTrainSlot(EnemySlotSide.Top, 0.5f),
            new EnemyTrainSlot(EnemySlotSide.Top, 0.85f),
            new EnemyTrainSlot(EnemySlotSide.Bottom, 0.15f),
            new EnemyTrainSlot(EnemySlotSide.Bottom, 0.5f),
            new EnemyTrainSlot(EnemySlotSide.Bottom, 0.85f)
        };

        private readonly Random random = new Random();
        private readonly HashSet<EnemyTrainSlot> occupiedSlots = new HashSet<EnemyTrainSlot>();

        public bool TryReserveSideAttackSlot(out EnemyTrainSlot slot)
        {
            return TryReserveSlot(SideAttackSlots, out slot);
        }

        // Src TryReserveSlotOnSide: prefer a free slot on the requested side, else fall back to any free slot.
        public bool TryReserveSideAttackSlotOnSide(EnemySlotSide side, out EnemyTrainSlot slot)
        {
            var candidates = new List<EnemyTrainSlot>();
            foreach (EnemyTrainSlot candidate in SideAttackSlots)
            {
                if (candidate.Side == side) candidates.Add(candidate);
            }

            if (TryReserveSlot(candidates, out slot))
            {
                return true;
            }

            return TryReserveSlot(SideAttackSlots, out slot);
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
            var availableSlots = new List<EnemyTrainSlot>();

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
    }
}
