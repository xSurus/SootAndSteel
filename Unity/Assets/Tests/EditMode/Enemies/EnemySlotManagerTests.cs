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
    }
}
