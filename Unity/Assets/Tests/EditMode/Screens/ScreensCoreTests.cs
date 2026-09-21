using System;
using Gamelab.Screens;
using Gamelab.UI;
using Gamelab.Utils;
using NUnit.Framework;

namespace Gamelab.Tests.Screens
{
    // Goldens come from evaluating the Src expressions in float32 (numpy) with a throwaway script.
    public class ScreensCoreTests
    {
        private const float Tol = 1e-4f;

        [TestCase(60f, 60f, 45, 0, 45, "On Time", "Met scheduled arrival")]
        [TestCase(30f, 60f, 45, 8, 53, "Time Bonus", "Arrived ahead of schedule")]
        [TestCase(240f, 60f, 45, -10, 35, "Time Penalty", "Arrived behind schedule")]
        [TestCase(0f, 60f, 45, 470, 515, "Time Bonus", "Arrived ahead of schedule")]
        [TestCase(0.05f, 60f, 45, 470, 515, "Time Bonus", "Arrived ahead of schedule")]
        [TestCase(1000f, 1f, 45, -19, 26, "Time Penalty", "Arrived behind schedule")]
        [TestCase(45f, 60f, 45, 3, 48, "Time Bonus", "Arrived ahead of schedule")]
        [TestCase(60f, 59f, 45, 0, 45, "On Time", "Met scheduled arrival")]
        public void RewardBreakdownDefaults(float actual, float reference, int delivery, int adj, int total, string label, string desc)
        {
            var b = LevelRewardBreakdown.FromCompletion(actual, reference);
            Assert.AreEqual(delivery, b.DeliveryReward);
            Assert.AreEqual(adj, b.TimeAdjustment);
            Assert.AreEqual(total, b.TotalCredits);
            Assert.AreEqual(label, b.TimeLineLabel);
            Assert.AreEqual(desc, b.TimeLineDescription);
        }

        [TestCase(30f, 60f, 60, 21, 81)]
        [TestCase(60f, 60f, 60, 0, 60)]
        [TestCase(240f, 60f, 60, -25, 35)]
        [TestCase(1000f, 1f, 60, -48, 12)]
        public void RewardBreakdownCustomBaseAndBonus(float actual, float reference, int delivery, int adj, int total)
        {
            var b = LevelRewardBreakdown.FromCompletion(actual, reference, 10, 50);
            Assert.AreEqual(delivery, b.DeliveryReward);
            Assert.AreEqual(adj, b.TimeAdjustment);
            Assert.AreEqual(total, b.TotalCredits);
        }

        [Test]
        public void RewardRoundingIsBankers()
        {
            // 20 * sqrt(81 / 64) = 22.5 exactly. MathF.Round gives 22 (Src behavior), so adjustment is +2.
            Assert.AreEqual(2, LevelRewardBreakdown.FromCompletion(64f, 81f).TimeAdjustment);
        }

        [Test]
        public void FormatSignedAmount()
        {
            Assert.AreEqual("+0", LevelRewardBreakdown.FormatSignedAmount(0));
            Assert.AreEqual("+8", LevelRewardBreakdown.FormatSignedAmount(8));
            Assert.AreEqual("-10", LevelRewardBreakdown.FormatSignedAmount(-10));
        }

        [TestCase(1, "First Departure")]
        [TestCase(2, "Unstable Schedule")]
        [TestCase(3, "Signal Failure")]
        [TestCase(4, "Critical Junction")]
        [TestCase(5, "Runaway Line")]
        [TestCase(6, "Full Steam")]
        [TestCase(7, "FULL STEAM!")]
        [TestCase(9, "FULL STEAM!!!")]
        [TestCase(0, "Full Steam")]
        [TestCase(-3, "Full Steam")]
        public void StageTitles(int stage, string expected) => Assert.AreEqual(expected, StageNaming.GetStageTitle(stage));

        [Test]
        public void EasingSmoothStep()
        {
            Assert.AreEqual(0f, Easing.SmoothStep(0f), Tol);
            Assert.AreEqual(0.15625f, Easing.SmoothStep(0.25f), Tol);
            Assert.AreEqual(0.5f, Easing.SmoothStep(0.5f), Tol);
            Assert.AreEqual(1f, Easing.SmoothStep(1f), Tol);
            Assert.AreEqual(0.5f, Easing.SmoothStepClamped(2f, 1f, 3f), Tol);
            Assert.AreEqual(0f, Easing.SmoothStepClamped(0f, 1f, 3f), Tol);
            Assert.AreEqual(1f, Easing.SmoothStepClamped(9f, 1f, 3f), Tol);
            Assert.AreEqual(1f, Easing.SmoothStepClamped(1f, 1f, 1f), Tol);
            Assert.AreEqual(0f, Easing.SmoothStepClamped(0.9f, 1f, 1f), Tol);
        }

        [Test]
        public void FailureReasonTextsAreExactSrStrings()
        {
            Assert.AreEqual("ALL WORKERS INCAPACITATED. ALL CARGO WAS STOLEN. TRAIN AND CREW WAS LEFT TO THE WEATHER",
                FailureReasonText.Get(FailureReason.AllPlayersKnockedOut));
            Assert.AreEqual("TRAIN HULL WAS DESTROYED BY HORSE RIDERS, THE COLD CREPT INAND TOOK OUT THE CREW. ALL CARGO LOST",
                FailureReasonText.Get(FailureReason.TrainFrozenHullBreached));
            Assert.AreEqual("OVEN COULD NOT BE KEPT BURNING, CREW GOT TAKEN OUT BY THE COLD SIBERIAN WINTER, TRAIN WAS LEFT TO THE WEATHER.",
                FailureReasonText.Get(FailureReason.TrainFrozenFurnaceOut));
            Assert.AreEqual("TRAIN WAS BREACHED, FURNANCE WAS FOUND OUT,CREW WAS WIPED OUT BY THE COLD. WILD ANIMALS TRACKS FOUND ON THE TRACKS, NO CREW FOUND ",
                FailureReasonText.Get(FailureReason.TrainFrozenBreachesAndFurnaceOut));
            Assert.AreEqual("NO INFORMATION AVAILABLE; NO CREW FOUND", FailureReasonText.Get(FailureReason.TrainFrozenOther));
            Assert.AreEqual(string.Empty, FailureReasonText.Get((FailureReason)99));
        }

        [TestCase(0.4f, "0 m")]
        [TestCase(0.5f, "0 m")]
        [TestCase(1.5f, "2 m")]
        [TestCase(1234.5f, "1234 m")]
        [TestCase(1234.6f, "1235 m")]
        public void DistanceUsesBankersRounding(float meters, string expected)
            => Assert.AreEqual(expected, PostDeathStatsText.Distance(new PostDeathStatsSnapshot(0, meters, 0, 0)));

        [Test]
        public void StatValueStrings()
        {
            var s = new PostDeathStatsSnapshot(12, 100f, -2, -1);
            Assert.AreEqual("12", PostDeathStatsText.EnemiesDefeated(s));
            Assert.AreEqual("0", PostDeathStatsText.StagesDefeated(s));
            Assert.AreEqual("0", PostDeathStatsText.UpgradesBought(s));
            var t = new PostDeathStatsSnapshot(0, 0f, 3, 7);
            Assert.AreEqual("3", PostDeathStatsText.StagesDefeated(t));
            Assert.AreEqual("7", PostDeathStatsText.UpgradesBought(t));
        }

        [Test]
        public void IncidentLine()
        {
            var now = new DateTime(2026, 3, 5, 9, 7, 0);
            Assert.AreEqual("N. 03 / F · FILED 05 MAR 2026 · 09:07", PostDeathStatsText.BuildIncidentLine(3, now));
            Assert.AreEqual("N. 01 / F · FILED 05 MAR 2026 · 09:07", PostDeathStatsText.BuildIncidentLine(0, now));
            Assert.AreEqual("N. 12 / F · FILED 05 MAR 2026 · 09:07", PostDeathStatsText.BuildIncidentLine(12, now));
        }

        private static float[] Run(FadeTransition f, float dt, int n)
        {
            var r = new float[n];
            for (int i = 0; i < n; i++) { f.Update(dt); r[i] = f.Opacity; }
            return r;
        }

        private static void AssertSeq(float[] expected, float[] actual)
        {
            for (int i = 0; i < expected.Length; i++) Assert.AreEqual(expected[i], actual[i], Tol, "step " + i);
        }

        [Test]
        public void FadeInCurve()
        {
            var f = new FadeTransition();
            f.FadeIn(1f);
            Assert.IsFalse(f.IsDone);
            AssertSeq(new[] { 0.15625f, 0.5f, 0.84375f, 1f, 1f }, Run(f, 0.25f, 5));
            Assert.IsTrue(f.IsDone);
        }

        [Test]
        public void FadeOutFromSnap()
        {
            var f = new FadeTransition();
            f.SnapTo(1f);
            Assert.IsTrue(f.IsDone);
            f.FadeOut(1f);
            AssertSeq(new[] { 0.84375f, 0.5f, 0.15625f, 0f, 0f }, Run(f, 0.25f, 5));
            Assert.IsTrue(f.IsDone);
        }

        [Test]
        public void FadeWithDelayHoldsThenFades()
        {
            var f = new FadeTransition();
            f.FadeIn(1f, 0.5f);
            AssertSeq(new[] { 0f, 0.15625f, 0.5f, 0.84375f, 1f, 1f }, Run(f, 0.25f, 6));
        }

        [Test]
        public void FadeLinearAndShortDuration()
        {
            var f = new FadeTransition();
            f.FadeTo(1f, 0.5f, 0f, false);
            AssertSeq(new[] { 0.2f, 0.4f, 0.6f, 0.8f, 1f }, Run(f, 0.1f, 5));
            var g = new FadeTransition();
            g.FadeIn(0.5f);
            AssertSeq(new[] { 0.104f, 0.352f, 0.648f, 0.896f, 1f, 1f }, Run(g, 0.1f, 6));
        }

        [Test]
        public void FadeToClampsAndSnapCancels()
        {
            var f = new FadeTransition();
            f.FadeTo(5f, 0.5f);
            Run(f, 1f, 1);
            Assert.AreEqual(1f, f.Opacity, Tol);
            f.FadeIn(10f);
            f.SnapTo(-1f);
            Assert.AreEqual(0f, f.Opacity, Tol);
            Assert.IsTrue(f.IsDone);
            Run(f, 1f, 3);
            Assert.AreEqual(0f, f.Opacity, Tol);
        }
    }
}
