using System.Numerics;
using NUnit.Framework;
using Gamelab.Input;
using Gamelab.Players;

namespace Gamelab.Tests.Players
{
    /// <summary>Minimal stub so roster/extension tests don't need a real Unity input backend.</summary>
    internal class StubInputActions : IInputActions
    {
        public bool PickupJustPressed;
        public bool StartJustPressed;

        public Vector2 GetMovement() => Vector2.Zero;
        public bool IsInteractJustPressed() => false;
        public bool IsInteractHeld() => false;
        public bool IsInteractJustReleased() => false;
        public bool IsGrabJustPressed() => false;
        public bool IsGrabHeld() => false;
        public bool IsGrabJustReleased() => false;
        public bool IsPickupJustPressed() => PickupJustPressed;
        public bool IsPickupHeld() => false;
        public bool IsStartJustPressed() => StartJustPressed;
        public bool IsStartHeld() => false;
        public bool IsPauseJustPressed() => false;
        public bool IsBackButtonJustPressed() => false;
        public bool IsBackButtonHeld() => false;
        public bool IsUpJustPressed() => false;
        public bool IsDownJustPressed() => false;
        public bool IsLeftJustPressed() => false;
        public bool IsRightJustPressed() => false;
    }

    public class PlayerRosterTests
    {
        [Test]
        public void JoinPlayer_AssignsSequentialIndices()
        {
            var roster = new PlayerRoster();

            Assert.IsTrue(roster.JoinPlayer(new StubInputActions()));
            Assert.IsTrue(roster.JoinPlayer(new StubInputActions()));

            Assert.AreEqual(2, roster.Slots.Count);
            Assert.AreEqual(0, roster.Slots[0].PlayerIndex);
            Assert.AreEqual(1, roster.Slots[1].PlayerIndex);
        }

        [Test]
        public void JoinPlayer_AtCapacity_ReturnsFalseAndDoesNotAdd()
        {
            var roster = new PlayerRoster();
            for (int i = 0; i < PlayerRoster.MaxPlayers; i++)
                Assert.IsTrue(roster.JoinPlayer(new StubInputActions()));

            bool joined = roster.JoinPlayer(new StubInputActions());

            Assert.IsFalse(joined);
            Assert.AreEqual(4, roster.Slots.Count);
        }

        [Test]
        public void Reset_ClearsAllSlots()
        {
            var roster = new PlayerRoster();
            roster.JoinPlayer(new StubInputActions());

            roster.Reset();

            Assert.AreEqual(0, roster.Slots.Count);
        }

        [Test]
        public void AnyPressedMenuConfirm_TrueWhenAnyoneJustPressedStart()
        {
            var roster = new PlayerRoster();
            roster.JoinPlayer(new StubInputActions());
            var second = new StubInputActions { StartJustPressed = true };
            roster.JoinPlayer(second);

            Assert.IsTrue(roster.AnyPressedMenuConfirm());
        }

        [Test]
        public void AnyPressedMenuConfirm_EnumerableOverload()
        {
            Assert.IsTrue(new[] { new StubInputActions { PickupJustPressed = true } }.AnyPressedMenuConfirm());
            Assert.IsTrue(new[] { new StubInputActions { StartJustPressed = true } }.AnyPressedMenuConfirm());
            Assert.IsFalse(new[] { new StubInputActions() }.AnyPressedMenuConfirm());
            Assert.IsFalse(new StubInputActions[0].AnyPressedMenuConfirm());
        }

        [Test]
        public void AnyPressedMenuConfirm_FalseWhenNobodyPressedAnything()
        {
            var roster = new PlayerRoster();
            roster.JoinPlayer(new StubInputActions());

            Assert.IsFalse(roster.AnyPressedMenuConfirm());
        }
    }
}
