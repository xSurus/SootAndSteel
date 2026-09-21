using System.Collections.Generic;
using System.Linq;
using Gamelab.Screens;
using Gamelab.Services.Sound;
using Gamelab.UI;
using NUnit.Framework;

namespace Gamelab.Tests.UI
{
    // Goldens come from a throwaway script that evaluates the Src StampRevealAnimator, PostLevelStatsScreen and
    // FailScreen update logic step by step in float32 (fixed dt of 1f/60f, and 0.5 or 0.3 for the big-dt cases).
    // Frames are 1-based update counts.
    public class ScreenTimelineTests
    {
        private const float D = 1f / 60f;
        private const float Tol = 1e-4f;
        private const string Pick = Sounds.PickupItem;
        private const string Craft = Sounds.Craft;

        private static LevelRewardBreakdown Rewards(int total) =>
            new LevelRewardBreakdown(25, total - 25, total, "Time bonus", "x");

        private sealed class PostRun
        {
            public readonly PostLevelStatsModel Model;
            public readonly List<(int Frame, string Sound)> Cues = new List<(int, string)>();
            public readonly List<int> Grants = new List<int>();
            public int Frame;
            public int RequestFrame;
            public int Requests;

            public PostRun(int total = 45)
            {
                Model = new PostLevelStatsModel(Rewards(total), Grants.Add);
                Model.Cue += s => Cues.Add((Frame, s));
                Model.ContinueRequested += () => { Requests++; if (RequestFrame == 0) RequestFrame = Frame; };
            }

            public void Step(float dt, bool confirm = false)
            {
                Frame++;
                Model.Update(dt, confirm);
            }

            public void RunTo(int frame, float dt = D, params int[] confirmFrames)
            {
                while (Frame < frame) Step(dt, confirmFrames.Contains(Frame + 1));
            }
        }

        [Test]
        public void StampTimerPopsThenSettlesAndFiresCueOnce()
        {
            var st = new StampRevealTimer(0.25f);
            var cues = new List<string>();
            st.Cue += cues.Add;
            Assert.IsFalse(st.Visible);
            Assert.AreEqual(1f, st.ScaleFactor);
            Assert.AreEqual(0f, st.RotationOffset);

            st.Update(D);
            Assert.IsFalse(st.Active);
            Assert.IsEmpty(cues);

            st.Trigger();
            Assert.IsTrue(st.Visible);
            Assert.IsTrue(st.Active);
            Assert.AreEqual(1.35f, st.ScaleFactor, Tol);
            Assert.AreEqual(-10f, st.RotationOffset, Tol);

            st.Update(D);
            Assert.AreEqual(1.34554f, st.ScaleFactor, Tol);
            Assert.AreEqual(-9.8726f, st.RotationOffset, Tol);

            for (int i = 2; i <= 7; i++) st.Update(D);
            Assert.IsEmpty(cues);
            st.Update(D);
            CollectionAssert.AreEqual(new[] { Sounds.Stamp }, cues);

            for (int i = 0; i < 20; i++) st.Update(D);
            Assert.AreEqual(1, cues.Count);
            Assert.IsFalse(st.Active);
            Assert.IsTrue(st.Visible);
            Assert.AreEqual(1f, st.ScaleFactor, Tol);
            Assert.AreEqual(0f, st.RotationOffset, Tol);
        }

        [Test]
        public void PostLevelCueOrderAndFramesAt60Fps()
        {
            var r = new PostRun();
            r.RunTo(300);
            var expected = new List<(int, string)>
            {
                (36, Craft), (64, Pick), (91, Pick), (113, Pick),
                (130, Pick), (137, Pick), (144, Pick), (151, Pick), (158, Pick), (165, Pick),
                (172, Pick), (179, Pick), (186, Pick), (193, Pick), (196, Craft),
            };
            CollectionAssert.AreEqual(expected, r.Cues);
            Assert.IsEmpty(r.Grants);
            Assert.AreEqual(PostLevelStatsPhase.IntroReveal, r.Model.Phase);
        }

        [Test]
        public void PostLevelVisibilityAndCountUpCurve()
        {
            var r = new PostRun();
            r.RunTo(1);
            Assert.AreEqual("+0", r.Model.SummaryText);
            Assert.IsFalse(r.Model.DeliveryVisible);
            r.RunTo(63);
            Assert.IsFalse(r.Model.DeliveryVisible);
            r.RunTo(64);
            Assert.IsTrue(r.Model.DeliveryVisible);
            Assert.IsFalse(r.Model.TimeVisible);
            r.RunTo(91);
            Assert.IsTrue(r.Model.TimeVisible);
            Assert.IsFalse(r.Model.SummaryVisible);
            r.RunTo(129);
            Assert.IsFalse(r.Model.SummaryVisible);
            r.RunTo(130);
            Assert.IsTrue(r.Model.SummaryVisible);

            var golden = new Dictionary<int, int> { { 140, 3 }, { 150, 11 }, { 160, 20 }, { 170, 31 }, { 180, 39 }, { 190, 44 }, { 200, 45 } };
            foreach (var kv in golden)
            {
                r.RunTo(kv.Key);
                Assert.AreEqual(kv.Value, r.Model.DisplayedTotal, "frame " + kv.Key);
                Assert.AreEqual("+" + kv.Value, r.Model.SummaryText);
            }
        }

        [Test]
        public void PostLevelStartFadeOpacity()
        {
            var r = new PostRun();
            r.RunTo(1);
            Assert.AreEqual(0.999176f, r.Model.FadeOpacity, Tol);
            r.RunTo(30);
            Assert.AreEqual(0.5f, r.Model.FadeOpacity, Tol);
            r.RunTo(37);
            Assert.AreEqual(0.328176f, r.Model.FadeOpacity, Tol);
            r.RunTo(60);
            Assert.AreEqual(0f, r.Model.FadeOpacity, Tol);
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(5, 6)]
        [TestCase(45, 10)]
        [TestCase(200, 10)]
        public void PostLevelCreditTickCounts(int total, int ticks)
        {
            var r = new PostRun(total);
            r.RunTo(300);
            // Cues after the three intro cues and the title cue are count-up ticks plus the final Craft.
            var countUp = r.Cues.Skip(4).ToList();
            Assert.AreEqual(ticks, countUp.Count(c => c.Sound == Pick));
            Assert.AreEqual((196, Craft), countUp.Last());
            Assert.AreEqual(total, r.Model.DisplayedTotal);
        }

        [Test]
        public void PostLevelBigDtFiresEveryCueInOrder()
        {
            var r = new PostRun();
            r.RunTo(10, 0.5f);
            var expected = new List<(int, string)>
            {
                (2, Craft), (3, Pick), (3, Pick), (4, Pick), (5, Pick), (6, Pick), (7, Pick), (7, Craft),
            };
            CollectionAssert.AreEqual(expected, r.Cues);
            Assert.AreEqual(0f, r.Model.FadeOpacity, Tol);
            Assert.AreEqual(45, r.Model.DisplayedTotal);
        }

        [Test]
        public void PostLevelBigDtCountUpValues()
        {
            var r = new PostRun();
            r.RunTo(4, 0.5f);
            Assert.AreEqual(0, r.Model.DisplayedTotal);
            r.RunTo(5, 0.5f);
            Assert.AreEqual(19, r.Model.DisplayedTotal);
            r.RunTo(6, 0.5f);
            Assert.AreEqual(44, r.Model.DisplayedTotal);
        }

        [Test]
        public void ConfirmMidRevealRevealsAllGrantsOnceAndStamps()
        {
            var r = new PostRun();
            r.RunTo(46, D, 45, 46);
            CollectionAssert.AreEqual(new[] { (36, Craft) }, r.Cues);
            Assert.AreEqual(new[] { 45 }, r.Grants);
            Assert.AreEqual(PostLevelStatsPhase.ContinueStamp, r.Model.Phase);
            Assert.IsTrue(r.Model.DeliveryVisible && r.Model.TimeVisible && r.Model.SummaryVisible);
            Assert.AreEqual(45, r.Model.DisplayedTotal);
            Assert.AreEqual("+45", r.Model.SummaryText);
            Assert.IsTrue(r.Model.Stamp.Visible);
            Assert.AreEqual(1.34554f, r.Model.Stamp.ScaleFactor, Tol);
            Assert.AreEqual(-9.8726f, r.Model.Stamp.RotationOffset, Tol);
            Assert.AreEqual(0.137926f, r.Model.FadeOpacity, Tol);

            r.RunTo(300, D, 45, 46);
            CollectionAssert.AreEqual(new[] { (36, Craft), (53, Sounds.Stamp) }, r.Cues);
            Assert.AreEqual(1, r.Grants.Count);
            Assert.AreEqual(1, r.Requests);
            Assert.AreEqual(176, r.RequestFrame);
            Assert.AreEqual(PostLevelStatsPhase.FadeOutToHub, r.Model.Phase);
            Assert.AreEqual(1f, r.Model.FadeOpacity, Tol);
        }

        [Test]
        public void ConfirmMidRevealPhaseAndFadeSamples()
        {
            var r = new PostRun();
            r.RunTo(120, D, 45);
            Assert.AreEqual(PostLevelStatsPhase.FadeOutToHub, r.Model.Phase);
            Assert.AreEqual(0.019676f, r.Model.FadeOpacity, Tol);
            r.RunTo(150, D, 45);
            Assert.AreEqual(0.623843f, r.Model.FadeOpacity, Tol);
        }

        [Test]
        public void ConfirmAfterFullRevealKeepsCuesAndGrantsOnce()
        {
            var r = new PostRun();
            r.RunTo(300, D, 230, 231);
            Assert.AreEqual(16, r.Cues.Count);
            Assert.AreEqual((196, Craft), r.Cues[14]);
            Assert.AreEqual((238, Sounds.Stamp), r.Cues[15]);
            Assert.AreEqual(new[] { 45 }, r.Grants);
            Assert.AreEqual(PostLevelStatsPhase.FadeOutToHub, r.Model.Phase);
        }

        [Test]
        public void ConfirmOnSameFrameAsCountUpEndStillGrants()
        {
            var r = new PostRun();
            r.RunTo(196, D, 196);
            Assert.AreEqual((196, Craft), r.Cues.Last());
            Assert.AreEqual(1, r.Grants.Count);
            Assert.AreEqual(PostLevelStatsPhase.ContinueStamp, r.Model.Phase);
        }

        [Test]
        public void ContinueRequestedFiresOnce()
        {
            var r = new PostRun();
            r.RunTo(400, D, 45);
            Assert.AreEqual(1, r.Requests);
        }

        // Fail screen

        private sealed class FailRun
        {
            public readonly FailScreenModel Model;
            public readonly List<(int Frame, string Sound)> Cues = new List<(int, string)>();
            public int Frame;
            public int Requests;
            public int RequestFrame;

            public FailRun(FailureReason reason = FailureReason.TrainFrozenHullBreached)
            {
                Model = new FailScreenModel(reason);
                Model.Cue += s => Cues.Add((Frame, s));
                Model.ReturnRequested += () => { Requests++; if (RequestFrame == 0) RequestFrame = Frame; };
            }

            public void RunTo(int frame, float dt = D, params int[] confirmFrames)
            {
                while (Frame < frame)
                {
                    Frame++;
                    Model.Update(dt, confirmFrames.Contains(Frame));
                }
            }
        }

        private static readonly string HullText = FailureReasonText.Get(FailureReason.TrainFrozenHullBreached);

        [Test]
        public void TypewriterPrefixAtSampledFrames()
        {
            var r = new FailRun();
            Assert.AreEqual(string.Empty, r.Model.CauseText);
            Assert.AreEqual(96, HullText.Length);
            var golden = new Dictionary<int, int> { { 1, 0 }, { 2, 1 }, { 10, 9 }, { 30, 27 }, { 60, 55 }, { 90, 82 }, { 95, 87 }, { 100, 91 }, { 110, 96 } };
            foreach (var kv in golden)
            {
                r.RunTo(kv.Key);
                Assert.AreEqual(HullText.Substring(0, kv.Value), r.Model.CauseText, "frame " + kv.Key);
            }
            Assert.AreEqual(FailScreenPhase.WaitingForReturn, r.Model.Phase);
        }

        [Test]
        public void TypewriterTicksEverySixthFrameAndStopsWhenDone()
        {
            var r = new FailRun();
            r.RunTo(200);
            Assert.AreEqual(17, r.Cues.Count);
            Assert.IsTrue(r.Cues.All(c => c.Sound == Pick));
            CollectionAssert.AreEqual(Enumerable.Range(1, 17).Select(i => i * 6), r.Cues.Select(c => c.Frame));
            Assert.AreEqual(0, r.Requests);
        }

        [Test]
        public void FailBigDtTicksEveryFrame()
        {
            var r = new FailRun();
            r.RunTo(4, 0.5f);
            Assert.AreEqual(4, r.Cues.Count);
        }

        [Test]
        public void FailBigDtPrefixes()
        {
            var r = new FailRun();
            r.RunTo(1, 0.5f);
            Assert.AreEqual(HullText.Substring(0, 27), r.Model.CauseText);
            r.RunTo(2, 0.5f);
            Assert.AreEqual(HullText.Substring(0, 55), r.Model.CauseText);
            r.RunTo(3, 0.5f);
            Assert.AreEqual(HullText.Substring(0, 82), r.Model.CauseText);
            r.RunTo(4, 0.5f);
            Assert.AreEqual(HullText, r.Model.CauseText);
        }

        [Test]
        public void ConfirmMidTypingCompletesTextStampsAndReturnsOnce()
        {
            var r = new FailRun();
            r.RunTo(19);
            Assert.AreEqual(HullText.Substring(0, 17), r.Model.CauseText);
            r.RunTo(20, D, 20);
            Assert.AreEqual(HullText, r.Model.CauseText);
            Assert.AreEqual(FailScreenPhase.Stamping, r.Model.Phase);
            Assert.IsTrue(r.Model.Stamp.Visible);
            r.RunTo(300, D, 20, 21);
            var expected = new List<(int, string)>
            {
                (6, Pick), (12, Pick), (18, Pick), (20, Sounds.MenuSelect), (27, Sounds.Stamp),
            };
            CollectionAssert.AreEqual(expected, r.Cues);
            Assert.AreEqual(1, r.Requests);
            Assert.AreEqual(87, r.RequestFrame);
        }

        [Test]
        public void ConfirmAfterTextDoneReturnsAtFrame217()
        {
            var r = new FailRun();
            r.RunTo(300, D, 150);
            Assert.AreEqual(17 + 2, r.Cues.Count);
            Assert.AreEqual((150, Sounds.MenuSelect), r.Cues[17]);
            Assert.AreEqual((157, Sounds.Stamp), r.Cues[18]);
            Assert.AreEqual(1, r.Requests);
            Assert.AreEqual(217, r.RequestFrame);
        }

        [Test]
        public void FailBigDtConfirmOnFirstFrame()
        {
            var r = new FailRun();
            r.RunTo(6, 0.3f, 1);
            CollectionAssert.AreEqual(new[] { (1, Pick), (1, Sounds.MenuSelect), (2, Sounds.Stamp) }, r.Cues);
            Assert.AreEqual(5, r.RequestFrame);
            Assert.AreEqual(1, r.Requests);
        }

        [Test]
        public void FailStampUsesItsOwnPopSettings()
        {
            var r = new FailRun();
            r.RunTo(1, D, 1);
            Assert.AreEqual(1.4f, r.Model.Stamp.ScaleFactor, Tol);
            Assert.AreEqual(-8f, r.Model.Stamp.RotationOffset, Tol);
        }
    }
}
