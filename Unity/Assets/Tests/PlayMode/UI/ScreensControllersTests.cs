using System;
using System.Collections;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.Screens;
using Gamelab.Services.Sound;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class ScreensControllersTests
    {
        private sealed class FakeInput : IInputActions
        {
            public bool Pickup, Start;
            public bool IsUpJustPressed() => false;
            public bool IsDownJustPressed() => false;
            public bool IsLeftJustPressed() => false;
            public bool IsRightJustPressed() => false;
            public bool IsPickupJustPressed() => Pickup;
            public bool IsPauseJustPressed() => false;
            public System.Numerics.Vector2 GetMovement() => System.Numerics.Vector2.Zero;
            public bool IsInteractJustPressed() => false;
            public bool IsInteractHeld() => false;
            public bool IsInteractJustReleased() => false;
            public bool IsGrabJustPressed() => false;
            public bool IsGrabHeld() => false;
            public bool IsGrabJustReleased() => false;
            public bool IsPickupHeld() => false;
            public bool IsStartJustPressed() => Start;
            public bool IsStartHeld() => false;
            public bool IsBackButtonJustPressed() => false;
            public bool IsBackButtonHeld() => false;
        }

        private const float Dt = 1f / 60f;

        private readonly List<UnityEngine.Object> cleanup = new List<UnityEngine.Object>();
        private readonly List<string> sounds = new List<string>();
        private readonly List<int> granted = new List<int>();
        private FakeInput p0, p1;
        private int done;

        [SetUp]
        public void SetUp()
        {
            sounds.Clear();
            granted.Clear();
            p0 = new FakeInput();
            p1 = new FakeInput();
            done = 0;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            cleanup.Clear();
        }

        private Func<IReadOnlyList<IInputActions>> Players => () => new List<IInputActions> { p0, p1 };

        // driven false disables the component so only the test's Tick calls advance the screen.
        private PostLevelStatsController MakeLevel(bool driven = false)
        {
            var go = new GameObject("PostLevelStatsController");
            cleanup.Add(go);
            var c = go.AddComponent<PostLevelStatsController>();
            c.Configure(50f, 100f, 1, granted.Add, () => done++, Players, sounds.Add);
            c.enabled = driven;
            return c;
        }

        private FailScreenController MakeFail(bool driven = false)
        {
            var go = new GameObject("FailScreenController");
            cleanup.Add(go);
            var c = go.AddComponent<FailScreenController>();
            c.Configure(FailureReason.TrainFrozenFurnaceOut, new PostDeathStatsSnapshot(7, 1234.5f, 3, 2), 3,
                new DateTime(2026, 1, 17, 4, 53, 0), () => done++, Players, sounds.Add);
            c.enabled = driven;
            return c;
        }

        private static void Run(Action<float> tick, float seconds)
        {
            int n = Mathf.RoundToInt(seconds / Dt);
            for (int i = 0; i < n; i++) tick(Dt);
        }

        [UnityTest]
        public IEnumerator PostLevel_RevealThenConfirmThenContinueOnce()
        {
            var c = MakeLevel();
            yield return null;
            int total = c.Model.Rewards.TotalCredits;
            Run(c.Tick, 3.6f); // reveal 2.16 s plus count-up 1.1 s
            Assert.IsTrue(c.Model.DeliveryVisible && c.Model.TimeVisible && c.Model.SummaryVisible);
            Assert.AreEqual(total, c.Model.DisplayedTotal);
            CollectionAssert.Contains(sounds, Sounds.Craft);
            CollectionAssert.Contains(sounds, Sounds.PickupItem);
            Assert.AreEqual(0, granted.Count);
            Assert.AreEqual(0, done);

            p0.Pickup = true;
            c.Tick(Dt);
            p0.Pickup = false;
            CollectionAssert.AreEqual(new[] { total }, granted);
            Run(c.Tick, 0.25f + 0.90f + 1.0f - 0.15f);
            Assert.AreEqual(0, done, "before the fade ends");
            Run(c.Tick, 0.4f);
            Assert.AreEqual(1, done);
            Run(c.Tick, 2f);
            Assert.AreEqual(1, done, "only once");
            Assert.AreEqual(1, granted.Count);
        }

        [UnityTest]
        public IEnumerator PostLevel_ConfirmMidReveal_GrantsFullTotalNow_StartAlsoConfirms()
        {
            var c = MakeLevel();
            yield return null;
            Run(c.Tick, 0.7f);
            Assert.IsFalse(c.Model.SummaryVisible);
            p1.Start = true;
            c.Tick(Dt);
            p1.Start = false;
            CollectionAssert.AreEqual(new[] { c.Model.Rewards.TotalCredits }, granted);
            Assert.AreEqual(c.Model.Rewards.TotalCredits, c.Model.DisplayedTotal);
            Assert.AreEqual("+" + c.Model.Rewards.TotalCredits, c.View.Summary.text);
        }

        [UnityTest]
        public IEnumerator PostLevel_ViewTexts()
        {
            var c = MakeLevel();
            yield return null;
            var r = c.Model.Rewards;
            Assert.AreEqual(StageNaming.GetStageTitle(1), c.View.StageTitle.text);
            Assert.AreEqual(LevelRewardBreakdown.FormatSignedAmount(r.DeliveryReward),
                c.View.Root.Q<Label>("DeliveryAmount").text);
            Assert.AreEqual(LevelRewardBreakdown.FormatSignedAmount(r.TimeAdjustment),
                c.View.Root.Q<Label>("TimeAmount").text);
            p0.Pickup = true;
            c.Tick(Dt);
            Assert.IsTrue(c.View.Summary.text.EndsWith("+" + r.TotalCredits));
        }

        [UnityTest]
        public IEnumerator Fail_TypewriterConfirmReturnOnce()
        {
            var c = MakeFail();
            yield return null;
            Assert.AreEqual("", c.View.CauseText.text);
            Run(c.Tick, 0.2f);
            string partial = c.View.CauseText.text;
            Assert.IsNotEmpty(partial);
            Run(c.Tick, 0.1f);
            Assert.Greater(c.View.CauseText.text.Length, partial.Length);
            CollectionAssert.Contains(sounds, Sounds.PickupItem);
            StringAssert.Contains("N. 03", c.View.IncidentLine.text);

            p0.Pickup = true;
            c.Tick(Dt);
            p0.Pickup = false;
            Assert.AreEqual(FailureReasonText.Get(FailureReason.TrainFrozenFurnaceOut), c.View.CauseText.text);
            CollectionAssert.Contains(sounds, Sounds.MenuSelect);
            Run(c.Tick, 1.0f);
            Assert.AreEqual(0, done);
            Run(c.Tick, 0.3f);
            Assert.AreEqual(1, done);
            Run(c.Tick, 2f);
            Assert.AreEqual(1, done);
        }

        [UnityTest]
        public IEnumerator Update_AdvancesWhileTimeScaleIsZero()
        {
            float saved = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                var level = MakeLevel(driven: true);
                var fail = MakeFail(driven: true);
                float start = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - start < 0.3f) yield return null;
                Assert.Less(level.Model.FadeOpacity, 1f, "fade advanced");
                Assert.IsNotEmpty(fail.Model.CauseText, "typewriter advanced");
            }
            finally
            {
                Time.timeScale = saved;
            }
        }

        [UnityTest]
        public IEnumerator SecondConfigure_Throws()
        {
            var level = MakeLevel();
            var fail = MakeFail();
            yield return null;
            Assert.Throws<InvalidOperationException>(() =>
                level.Configure(50f, 100f, 1, _ => { }, () => { }, Players, _ => { }));
            Assert.Throws<InvalidOperationException>(() =>
                fail.Configure(FailureReason.TrainFrozenFurnaceOut, default, 1, DateTime.Now, () => { }, Players, _ => { }));
        }
    }
}
