using System.Collections.Generic;
using Gamelab.Players;
using Gamelab.Run;
using Gamelab.Tests.EditMode.UI;
using Gamelab.UI;
using NUnit.Framework;

namespace Gamelab.Tests.UI
{
    public class HubStateTests
    {
        private static readonly int[] One = { 0 };
        private static readonly int[] Two = { 0, 1 };

        private static void Run(HubDepartureModel m, float seconds, IReadOnlyList<int> joined, int pending,
            bool interact = false, bool grab = false)
        {
            m.Update(seconds, joined, pending, interact, grab);
        }

        [Test]
        public void ToggleReadyAndAllReady()
        {
            var m = new HubDepartureModel();
            Run(m, 0.1f, Two, 0);
            m.ToggleReady(0);
            Run(m, 0.01f, Two, 0);
            Assert.IsTrue(m.IsReady(0));
            Assert.IsFalse(m.AllReady);
            m.ToggleReady(1);
            Run(m, 0.01f, Two, 0);
            Assert.IsTrue(m.AllReady);
            m.ToggleReady(1);
            Assert.IsFalse(m.IsReady(1));
        }

        [Test]
        public void ReadySlotsFollowJoinedListOrder()
        {
            var m = new HubDepartureModel();
            var joined = new[] { 2, 0 };
            Run(m, 0.1f, joined, 0);
            m.ToggleReady(0);
            Assert.AreEqual(new[] { false, true, false, false }, m.ReadySlots);
        }

        [Test]
        public void LeavingPlayerIsPruned()
        {
            var m = new HubDepartureModel();
            Run(m, 0.1f, Two, 0);
            m.ToggleReady(1);
            Run(m, 0.1f, One, 0);
            Assert.IsFalse(m.IsReady(1));
            Assert.AreEqual(0, m.ReadyPlayers.Count);
        }

        [Test]
        public void HintAppearsAfterFiveSecondsAndHidesAfterEight()
        {
            var m = new HubDepartureModel();
            Run(m, 4.9f, One, 0);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
            Run(m, 0.2f, One, 0);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Passive, m.Bubble.Mode);
            Assert.AreEqual("To depart, each player should interact with the lever.", m.Bubble.Text);
            Assert.AreEqual("Hint", m.Bubble.Speaker);
            Run(m, 7.9f, One, 0);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Passive, m.Bubble.Mode);
            Run(m, 0.2f, One, 0);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
            Run(m, 1f, One, 0);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
        }

        [Test]
        public void HintNotShownWhenAllReady()
        {
            var m = new HubDepartureModel(100f);
            Run(m, 0.1f, One, 0);
            m.ToggleReady(0);
            Run(m, 6f, One, 0);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
        }

        [Test]
        public void DecisionOpensViaToggleReadyWhenLastPlayerReadiesWithPending()
        {
            var m = new HubDepartureModel();
            Run(m, 6f, Two, 1);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Passive, m.Bubble.Mode);
            m.ToggleReady(0);
            Assert.IsFalse(m.IsDecisionOpen);
            m.ToggleReady(1);
            Assert.IsTrue(m.IsDecisionOpen);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Decision, m.Bubble.Mode);
            Assert.AreEqual("Bought items are still off-board. Depart anyway?", m.Bubble.Text);
            Assert.AreEqual("No", m.Bubble.LeftLabel);
            Assert.AreEqual("Yes", m.Bubble.RightLabel);
        }

        [Test]
        public void DecisionOpensViaUpdateFallbackAndHidesHint()
        {
            var m = new HubDepartureModel();
            var m2 = new HubDepartureModel();
            Run(m2, 0.1f, One, 0);
            m2.ToggleReady(0);
            Run(m2, 0.01f, One, 1); // pending appears after ready
            Assert.IsTrue(m2.IsDecisionOpen);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Decision, m2.Bubble.Mode);
        }

        [Test]
        public void EarlyPressIsIgnoredDuringInputBlock()
        {
            var m = new HubDepartureModel();
            Run(m, 0.1f, One, 0);
            m.ToggleReady(0);
            Run(m, 0.01f, One, 1);
            Assert.IsTrue(m.IsDecisionOpen);
            Run(m, 0.1f, One, 1, interact: true);
            Assert.IsTrue(m.IsDecisionOpen);
            Run(m, 0.15f, One, 1);
            Run(m, 0.01f, One, 1, interact: true);
            Assert.IsFalse(m.IsDecisionOpen);
        }

        private static HubDepartureModel OpenDecisionModel(out List<int> departs)
        {
            var m = new HubDepartureModel();
            var list = new List<int>();
            m.DepartRequested += () => list.Add(1);
            departs = list;
            Run(m, 0.1f, One, 1);
            m.ToggleReady(0);
            Assert.IsTrue(m.IsDecisionOpen);
            Run(m, 0.3f, One, 1); // clears the input block
            return m;
        }

        [Test]
        public void GrabSetsNotReadyAndCloses()
        {
            var m = OpenDecisionModel(out var departs);
            Run(m, 0.01f, One, 1, grab: true);
            Assert.IsFalse(m.IsDecisionOpen);
            Assert.IsFalse(m.IsReady(0));
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
            Run(m, 2f, One, 1);
            Assert.AreEqual(0, departs.Count);
        }

        [Test]
        public void InteractAllowsDepartAfterHoldAndRaisesOnce()
        {
            var m = OpenDecisionModel(out var departs);
            Run(m, 0.01f, One, 1, interact: true);
            Assert.IsFalse(m.IsDecisionOpen);
            Assert.IsFalse(m.Departing);
            Run(m, 0.5f, One, 1);
            Assert.AreEqual(0, departs.Count);
            Run(m, 0.3f, One, 1);
            Assert.AreEqual(1, departs.Count);
            Assert.IsTrue(m.Departing);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
            Run(m, 2f, One, 1);
            Assert.AreEqual(1, departs.Count);
        }

        [Test]
        public void NoDepartureWithPendingItemsWithoutAllowance()
        {
            var m = OpenDecisionModel(out var departs);
            Run(m, 5f, One, 1);
            Assert.AreEqual(0, departs.Count);
            Assert.AreEqual(0f, m.HoldTimer);
        }

        [Test]
        public void DepartsWithoutPendingAfterHold()
        {
            var m = new HubDepartureModel();
            int departs = 0;
            m.DepartRequested += () => departs++;
            Run(m, 0.1f, One, 0);
            m.ToggleReady(0);
            Run(m, 0.5f, One, 0);
            Assert.AreEqual(0, departs);
            Run(m, 0.3f, One, 0);
            Assert.AreEqual(1, departs);
        }

        [Test]
        public void AllowanceDroppedWhenPendingGrows()
        {
            var m = OpenDecisionModel(out var departs);
            Run(m, 0.01f, One, 1, interact: true);
            Run(m, 0.3f, One, 1);
            Run(m, 0.01f, One, 2); // grows: allowance dropped, decision reopens
            Assert.IsTrue(m.IsDecisionOpen);
            Assert.AreEqual(0f, m.HoldTimer);
            Assert.AreEqual(0, departs.Count);
        }

        [Test]
        public void DepartingStopsFurtherUpdates()
        {
            var m = new HubDepartureModel();
            Run(m, 0.1f, One, 0);
            m.ToggleReady(0);
            Run(m, 1f, One, 0);
            Assert.IsTrue(m.Departing);
            float hold = m.HoldTimer;
            Run(m, 5f, One, 0);
            Assert.AreEqual(hold, m.HoldTimer);
            Assert.AreEqual(DialogBubbleModel.BubbleMode.Hidden, m.Bubble.Mode);
        }

        [Test]
        public void ToggleReadyIgnoredWhileDecisionOpen()
        {
            var m = OpenDecisionModel(out _);
            m.ToggleReady(0);
            Assert.IsTrue(m.IsReady(0));
        }

        [Test]
        public void CraftingHelpPersistsToRunCredits()
        {
            var run = new RunCredits();
            var help = new CraftingHelpModel(run);
            int changed = 0;
            help.Changed += () => changed++;
            Assert.IsTrue(help.Visible);
            help.Toggle();
            Assert.IsFalse(run.HubScreenCraftingHelpVisible);
            Assert.AreEqual(1, changed);
            help.Visible = false;
            Assert.AreEqual(1, changed);
            Assert.IsFalse(new CraftingHelpModel(run).Visible);
        }

        [Test]
        public void UpdateRaisesChangedWhenReadySlotsOrAllReadyChange()
        {
            var m = new HubDepartureModel(100f);
            int changed = 0;
            m.Changed += () => changed++;
            Run(m, 0.1f, One, 0);
            m.ToggleReady(0);
            changed = 0;
            Run(m, 0.1f, One, 0);
            Assert.AreEqual(0, changed, "nothing changed");
            Run(m, 0.1f, Two, 0);
            Assert.AreEqual(1, changed, "new player joined, AllReady off");
            Run(m, 0.1f, new[] { 1, 0 }, 0);
            Assert.AreEqual(2, changed, "joined order changed");
        }

        [Test]
        public void ToggleReadyIgnoredWhileDeparting()
        {
            var m = new HubDepartureModel();
            Run(m, 0.1f, One, 0);
            m.ToggleReady(0);
            Run(m, 1f, One, 0);
            Assert.IsTrue(m.Departing);
            m.ToggleReady(0);
            Assert.IsTrue(m.IsReady(0));
        }

        [Test]
        public void HubInputForwardsInteractAsAllowance()
        {
            var a = new FakeInputActions();
            var slots = new List<PlayerSlot> { new PlayerSlot(0, a) };
            var input = new HubInput(() => slots);
            var help = new CraftingHelpModel(new RunCredits());
            var dep = new HubDepartureModel(0.5f);
            int departs = 0;
            dep.DepartRequested += () => departs++;
            input.Tick(0.1f, dep, help, 1);
            dep.ToggleReady(0);
            Assert.IsTrue(dep.IsDecisionOpen);
            input.Tick(0.3f, dep, help, 1);
            a.Interact = true;
            input.Tick(0.1f, dep, help, 1);
            a.Clear();
            Assert.IsFalse(dep.IsDecisionOpen);
            input.Tick(0.6f, dep, help, 1);
            Assert.AreEqual(1, departs);
        }

        [Test]
        public void HubInputGrabWinsWhenBothPressed()
        {
            var a = new FakeInputActions();
            var slots = new List<PlayerSlot> { new PlayerSlot(0, a) };
            var input = new HubInput(() => slots);
            var help = new CraftingHelpModel(new RunCredits());
            var dep = new HubDepartureModel(0.5f);
            input.Tick(0.1f, dep, help, 1);
            dep.ToggleReady(0);
            input.Tick(0.3f, dep, help, 1);
            a.Interact = true;
            a.Grab = true;
            input.Tick(0.1f, dep, help, 1);
            Assert.IsFalse(dep.IsDecisionOpen);
            Assert.IsFalse(dep.IsReady(0));
        }

        [Test]
        public void HubInputPollsAllSlots()
        {
            var a = new FakeInputActions();
            var b = new FakeInputActions();
            var slots = new List<PlayerSlot> { new PlayerSlot(2, a), new PlayerSlot(0, b) };
            var input = new HubInput(() => slots);
            var run = new RunCredits();
            var help = new CraftingHelpModel(run);
            var dep = new HubDepartureModel(100f);

            a.Back = true;
            b.Back = true;
            input.Tick(0.1f, dep, help, 0);
            Assert.IsFalse(help.Visible);

            a.Clear();
            b.Clear();
            dep.ToggleReady(0);
            input.Tick(0.1f, dep, help, 0);
            Assert.AreEqual(new[] { false, true, false, false }, dep.ReadySlots);
            Assert.IsFalse(help.Visible);

            // decision path: interact and grab forwarded
            input.Tick(0.1f, dep, help, 1);
            dep.ToggleReady(2);
            Assert.IsTrue(dep.IsDecisionOpen);
            input.Tick(0.3f, dep, help, 1);
            b.Grab = true;
            input.Tick(0.1f, dep, help, 1);
            Assert.IsFalse(dep.IsDecisionOpen);
            Assert.IsFalse(dep.IsReady(2));
        }

        [Test]
        public void DialogBubbleRaisesChangedOnlyOnChange()
        {
            var b = new DialogBubbleModel();
            int n = 0;
            b.Changed += () => n++;
            b.Hide();
            Assert.AreEqual(0, n);
            b.ShowPassive("Hint", "x");
            b.ShowPassive("Hint", "x");
            Assert.AreEqual(1, n);
            b.ShowDecision("Hint", "x", "No", "Yes");
            Assert.AreEqual(2, n);
            b.Hide();
            Assert.AreEqual(3, n);
        }
    }
}
