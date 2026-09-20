using NUnit.Framework;
using Gamelab.Enemies.Core;

namespace Gamelab.Tests.Enemies
{
    public class EnemySlotManagerTests
    {
        [Test]
        public void TryReserveSideAttackSlot_TwiceForAllSlots_EventuallyFails()
        {
            var manager = new EnemySlotManager();
            int reserved = 0;
            while (manager.TryReserveSideAttackSlot(out _))
            {
                reserved++;
                Assert.Less(reserved, 100, "Should not be able to reserve unboundedly.");
            }

            Assert.AreEqual(6, reserved);
        }

        [Test]
        public void ReleaseSlot_MakesSlotReservableAgain()
        {
            var manager = new EnemySlotManager();
            EnemyTrainSlot first = default;
            bool haveFirst = false;
            for (int i = 0; i < 6; i++)
            {
                Assert.IsTrue(manager.TryReserveSideAttackSlot(out EnemyTrainSlot s));
                if (!haveFirst) { first = s; haveFirst = true; }
            }

            Assert.IsFalse(manager.TryReserveSideAttackSlot(out _), "All slots taken.");
            manager.ReleaseSlot(first);
            Assert.IsTrue(manager.TryReserveSideAttackSlot(out EnemyTrainSlot again));
            Assert.AreEqual(first, again);
        }

        [Test]
        public void TryReserveSideAttackSlotOnSide_ReservesOnlyRequestedSideAndFailsWhenFull()
        {
            var manager = new EnemySlotManager();
            EnemyTrainSlot first = default;
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(manager.TryReserveSideAttackSlotOnSide(EnemySlotSide.Top, out EnemyTrainSlot s));
                Assert.AreEqual(EnemySlotSide.Top, s.Side);
                if (i == 0) first = s;
            }

            Assert.IsFalse(manager.TryReserveSideAttackSlotOnSide(EnemySlotSide.Top, out _));
            Assert.IsTrue(manager.TryReserveSideAttackSlotOnSide(EnemySlotSide.Bottom, out EnemyTrainSlot b));
            Assert.AreEqual(EnemySlotSide.Bottom, b.Side);

            manager.ReleaseSlot(first);
            Assert.IsTrue(manager.TryReserveSideAttackSlotOnSide(EnemySlotSide.Top, out EnemyTrainSlot again));
            Assert.AreEqual(first, again);
        }
    }
}
