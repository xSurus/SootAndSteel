using Gamelab.Enemies.Core;
using NUnit.Framework;

namespace Gamelab.Tests.Enemies
{
    public class RifleEnemyBrainTests
    {
        private static RifleEnemyBrain Ready(float cooldown = 2f, float flee = 0.75f)
        {
            var b = new RifleEnemyBrain(cooldown, flee, cooldown);
            b.ArriveAtApproach();
            return b;
        }

        [Test]
        public void Initial_State()
        {
            var b = new RifleEnemyBrain(2f, 0.75f, 0f);
            Assert.AreEqual(HorseState.ApproachingSideAttackSlot, b.Horse);
            Assert.AreEqual(RiderState.Idle, b.Rider);
            Assert.IsFalse(b.WasNeutralized);
        }

        [Test]
        public void ArriveAtApproach_Holds()
        {
            var b = new RifleEnemyBrain(2f, 0.75f, 0f);
            b.ArriveAtApproach();
            Assert.AreEqual(HorseState.HoldingSideAttackSlot, b.Horse);
        }

        [Test]
        public void TryShoot_NotHolding_False()
        {
            var b = new RifleEnemyBrain(2f, 0.75f, 5f);
            Assert.IsFalse(b.TryShoot());
            Assert.AreEqual(RiderState.Idle, b.Rider);
        }

        [Test]
        public void TryShoot_CooldownNotReached_False()
        {
            var b = new RifleEnemyBrain(2f, 0.75f, 1.99f);
            b.ArriveAtApproach();
            Assert.IsFalse(b.TryShoot());
        }

        [Test]
        public void TryShoot_CooldownExactlyReached_True()
        {
            var b = new RifleEnemyBrain(2f, 0.75f, 1f);
            b.ArriveAtApproach();
            b.Tick(1f);
            Assert.IsTrue(b.TryShoot());
            Assert.AreEqual(RiderState.Aiming, b.Rider);
        }

        [Test]
        public void TryShoot_ResetsCooldown()
        {
            var b = Ready();
            Assert.IsTrue(b.TryShoot());
            b.UpdateRider(1f);
            b.UpdateRider(0.4f);
            Assert.AreEqual(RiderState.Idle, b.Rider);
            Assert.IsFalse(b.TryShoot());
            b.Tick(1.99f);
            Assert.IsFalse(b.TryShoot());
            b.Tick(0.01f);
            Assert.IsTrue(b.TryShoot());
        }

        [Test]
        public void TryShoot_WhileAiming_False()
        {
            var b = Ready(0f);
            Assert.IsTrue(b.TryShoot());
            b.Tick(10f);
            Assert.IsFalse(b.TryShoot());
        }

        [Test]
        public void Aiming_DoesNotFireBeforeOneSecond()
        {
            var b = Ready();
            b.TryShoot();
            Assert.IsFalse(b.UpdateRider(0.5f));
            Assert.IsFalse(b.UpdateRider(0.49f));
            Assert.AreEqual(RiderState.Aiming, b.Rider);
            Assert.IsTrue(b.UpdateRider(0.02f));
        }

        [Test]
        public void Aiming_ExactBoundary_Fires()
        {
            var b = Ready();
            b.TryShoot();
            Assert.IsTrue(b.UpdateRider(1f));
            Assert.AreEqual(RiderState.Recoil, b.Rider);
        }

        [Test]
        public void Recoil_EndsAtPointFour()
        {
            var b = Ready();
            b.TryShoot();
            b.UpdateRider(1f);
            Assert.IsFalse(b.UpdateRider(0.2f));
            Assert.AreEqual(RiderState.Recoil, b.Rider);
            Assert.IsFalse(b.UpdateRider(0.2f));
            Assert.AreEqual(RiderState.Idle, b.Rider);
        }

        [Test]
        public void Recoil_ExactBoundary_Idle()
        {
            var b = Ready();
            b.TryShoot();
            b.UpdateRider(1f);
            b.UpdateRider(0.4f);
            Assert.AreEqual(RiderState.Idle, b.Rider);
        }

        [Test]
        public void Idle_UpdateRider_NoFire()
        {
            var b = Ready();
            Assert.IsFalse(b.UpdateRider(10f));
            Assert.AreEqual(RiderState.Idle, b.Rider);
        }

        [Test]
        public void Death_SetsDeadAndNeutralized()
        {
            var b = Ready();
            Assert.IsTrue(b.TryStartFleeingDeath());
            Assert.AreEqual(RiderState.Dead, b.Rider);
            Assert.IsTrue(b.WasNeutralized);
            Assert.AreEqual(HorseState.HoldingSideAttackSlot, b.Horse);
        }

        [Test]
        public void DoubleDeath_False()
        {
            var b = Ready();
            Assert.IsTrue(b.TryStartFleeingDeath());
            Assert.IsFalse(b.TryStartFleeingDeath());
        }

        [Test]
        public void Dead_IgnoresTryShoot()
        {
            var b = Ready(0f);
            b.TryStartFleeingDeath();
            Assert.IsFalse(b.TryShoot());
            Assert.AreEqual(RiderState.Dead, b.Rider);
        }

        [Test]
        public void Death_DuringAiming_NeverFires()
        {
            var b = Ready();
            b.TryShoot();
            b.TryStartFleeingDeath();
            Assert.IsFalse(b.UpdateRider(5f));
        }

        [Test]
        public void Flee_AfterDelay()
        {
            var b = Ready(2f, 0.75f);
            b.TryStartFleeingDeath();
            b.UpdateRider(0.74f);
            Assert.AreEqual(HorseState.HoldingSideAttackSlot, b.Horse);
            Assert.AreEqual(0f, b.FleeDirection);
            b.UpdateRider(0.01f);
            Assert.AreEqual(HorseState.Fleeing, b.Horse);
            Assert.AreEqual(1f, b.FleeDirection);
        }

        [Test]
        public void Flee_IsIdempotent()
        {
            var b = Ready(2f, 0.75f);
            b.TryStartFleeingDeath();
            b.UpdateRider(1f);
            b.UpdateRider(1f);
            Assert.AreEqual(HorseState.Fleeing, b.Horse);
            Assert.AreEqual(1f, b.FleeDirection);
        }

        [Test]
        public void Death_WhileApproaching_StillFleesAfterDelay()
        {
            var b = new RifleEnemyBrain(2f, 0.5f, 0f);
            b.TryStartFleeingDeath();
            b.UpdateRider(0.5f);
            Assert.AreEqual(HorseState.Fleeing, b.Horse);
        }
    }
}
