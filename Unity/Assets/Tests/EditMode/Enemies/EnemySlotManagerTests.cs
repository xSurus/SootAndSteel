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
            manager.TryReserveSideAttackSlot(out EnemyTrainSlot slot);
            manager.ReleaseSlot(slot);

            manager.Clear();
            Assert.IsTrue(manager.TryReserveSideAttackSlot(out _));
        }
    }
}
