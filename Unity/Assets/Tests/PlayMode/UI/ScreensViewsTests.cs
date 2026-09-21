using System.Collections;
using System.Collections.Generic;
using Gamelab.Screens;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class ScreensViewsTests
    {
        // Pixel snapping at the 640x480 test panel (canvas scale 0.444) moves edges by up to a canvas unit or two.
        private const float Tol = 3f;

        private readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.DestroyImmediate(o);
            cleanup.Clear();
        }

        private T Make<T>() where T : Component
        {
            var go = new GameObject(typeof(T).Name);
            cleanup.Add(go);
            go.AddComponent<UIDocument>();
            return go.AddComponent<T>();
        }

        private PanelSettings Panel()
        {
            var ps = UiPanel.Create();
            cleanup.Add(ps);
            return ps;
        }

        private static IEnumerator Frames(int n = 3) { for (int i = 0; i < n; i++) yield return null; }
        private static bool Shown(VisualElement e) => e.resolvedStyle.display == DisplayStyle.Flex;
        private static float Right(VisualElement e) => e.layout.x + e.layout.width;
        private static bool HasImage(VisualElement e) =>
            e.resolvedStyle.backgroundImage.texture != null || e.resolvedStyle.backgroundImage.sprite != null;

        private static void AssertColor(Color expected, Color actual, float delta = 0.01f)
        {
            Assert.AreEqual(expected.r, actual.r, delta, "r");
            Assert.AreEqual(expected.g, actual.g, delta, "g");
            Assert.AreEqual(expected.b, actual.b, delta, "b");
            Assert.AreEqual(expected.a, actual.a, delta, "a");
        }

        // Without background-size the art keeps its pixel size instead of stretching to the Gum box.
        private static void AssertStretched(VisualElement e)
        {
            var size = e.resolvedStyle.backgroundSize;
            Assert.AreEqual(LengthUnit.Percent, size.x.unit);
            Assert.AreEqual(LengthUnit.Percent, size.y.unit);
            Assert.AreEqual(100f, size.x.value, 0.01f);
            Assert.AreEqual(100f, size.y.value, 0.01f);
        }

        private static PostLevelStatsModel Level(out int total)
        {
            var rewards = LevelRewardBreakdown.FromCompletion(50f, 100f);
            total = rewards.TotalCredits;
            return new PostLevelStatsModel(rewards, _ => { });
        }

        // StampView

        [Test]
        public void StampView_FollowsTimer()
        {
            var timer = new StampRevealTimer(0.25f);
            var el = new VisualElement();
            StampView.Apply(el, timer, 10f);
            Assert.AreEqual(DisplayStyle.None, el.style.display.value);

            timer.Trigger();
            StampView.Apply(el, timer, 45f);
            Assert.AreEqual(-(45f - 10f), el.style.rotate.value.angle.ToDegrees(), 0.001f);
            StampView.Apply(el, timer, 10f);
            Assert.AreEqual(DisplayStyle.Flex, el.style.display.value);
            Assert.AreEqual(1.35f, el.style.scale.value.value.x, 0.001f);
            Assert.AreEqual(1.35f, el.style.scale.value.value.y, 0.001f);
            // Gum base 10 plus the pop offset -10 is 0, negated for clockwise USS.
            Assert.AreEqual(0f, el.style.rotate.value.angle.ToDegrees(), 0.001f);
            Assert.AreEqual(0f, el.style.transformOrigin.value.x.value, 0.001f);
            Assert.AreEqual(0f, el.style.transformOrigin.value.y.value, 0.001f);

            timer.Update(1f);
            StampView.Apply(el, timer, 10f);
            Assert.AreEqual(1f, el.style.scale.value.value.x, 0.001f);
            Assert.AreEqual(-10f, el.style.rotate.value.angle.ToDegrees(), 0.001f);
        }

        // PostLevelStatsView

        [UnityTest]
        public IEnumerator PostLevel_LayoutFollowsGum()
        {
            var model = Level(out _);
            model.Update(0.016f, true); // reveals every row and the stamp, hidden elements have no layout
            var view = Make<PostLevelStatsView>();
            view.Bind(model, "First Departure", Panel());
            yield return Frames();
            var root = view.Root.layout;
            var paper = view.Paper;
            Assert.AreEqual(600f, paper.layout.width, Tol);
            Assert.AreEqual(400f, paper.layout.height, Tol);
            // Centred on the root, then the Gum container offset (-13,-65).
            Assert.AreEqual(root.width / 2f - 13f, paper.layout.x + 300f, Tol);
            Assert.AreEqual(root.height / 2f - 65f, paper.layout.y + 200f, Tol);
            Assert.IsTrue(HasImage(paper));
            AssertColor(Color.white, view.Root.resolvedStyle.backgroundColor);

            Assert.AreEqual(15, view.Punches.childCount);
            Assert.AreEqual(7f, view.Punches[0].layout.x, Tol);
            Assert.AreEqual(20f, view.Punches[0].layout.y, Tol);
            Assert.AreEqual(370f, view.Punches[14].layout.y, Tol);
            Assert.AreEqual(50f, view.Punches[0].layout.width, Tol);
            AssertStretched(view.Punches[0]);
            AssertStretched(view.Stamp);

            Assert.AreEqual(40f, view.StageTitle.resolvedStyle.fontSize, 0.01f);
            Assert.AreEqual(47f, view.StageTitle.layout.y, Tol);
            Assert.AreEqual(130f, view.DeliveryRow.layout.y, Tol);
            Assert.AreEqual(164f, view.TimeRow.layout.y, Tol);
            Assert.AreEqual(500f, view.DeliveryRow.layout.width, Tol);
            Assert.AreEqual(50f, view.DeliveryRow.layout.x, Tol);
            Assert.AreEqual(22f, view.Root.Q<Label>("DeliveryLabel").resolvedStyle.fontSize, 0.01f);
            Assert.AreEqual(Right(view.DeliveryRow), Right(view.Root.Q("DeliveryAmount")) + view.DeliveryRow.layout.x, Tol);
            Assert.AreEqual(263f, view.SummaryRow.layout.y, Tol);
            Assert.AreEqual(422f, view.Stamp.layout.x, Tol);
            Assert.AreEqual(30f, view.Stamp.layout.y, Tol);
            Assert.AreEqual(50f, view.Stamp.layout.width, Tol);
            // Gum button at (1081,691) in the offset root, whose paper origin is (660,340).
            Assert.AreEqual(421f, view.ContinueButton.layout.x, Tol);
            Assert.AreEqual(351f, view.ContinueButton.layout.y, Tol);
        }

        [UnityTest]
        public IEnumerator PostLevel_TextsAndButton()
        {
            var model = Level(out _);
            var view = Make<PostLevelStatsView>();
            view.Bind(model, "First Departure", Panel());
            yield return Frames();
            var r = model.Rewards;
            Assert.AreEqual("First Departure", view.StageTitle.text);
            Assert.AreEqual("01", view.Root.Q<Label>("DeliveryIndex").text);
            Assert.AreEqual("Delivery Reward", view.Root.Q<Label>("DeliveryLabel").text);
            Assert.AreEqual("Coal shipment delivered", view.Root.Q<Label>("DeliveryDescription").text);
            Assert.AreEqual(LevelRewardBreakdown.FormatSignedAmount(r.DeliveryReward), view.Root.Q<Label>("DeliveryAmount").text);
            Assert.AreEqual("02", view.Root.Q<Label>("TimeIndex").text);
            Assert.AreEqual(r.TimeLineLabel, view.Root.Q<Label>("TimeLabel").text);
            Assert.AreEqual(r.TimeLineDescription, view.Root.Q<Label>("TimeDescription").text);
            Assert.AreEqual(LevelRewardBreakdown.FormatSignedAmount(r.TimeAdjustment), view.Root.Q<Label>("TimeAmount").text);
            Assert.AreEqual("Account Credited", view.SummaryDescription.text);
            Assert.AreEqual("Continue", view.ContinueButton.Q<Label>("Text").text);
            Assert.IsTrue(HasImage(view.ContinueButton.Q("Icon")), "A glyph");
        }

        [UnityTest]
        public IEnumerator PostLevel_FollowsModel()
        {
            var model = Level(out int total);
            var view = Make<PostLevelStatsView>();
            view.Bind(model, "First Departure", Panel());
            yield return Frames();
            Assert.IsFalse(Shown(view.DeliveryRow));
            Assert.IsFalse(Shown(view.TimeRow));
            Assert.IsFalse(Shown(view.SummaryRow));
            Assert.IsFalse(Shown(view.Stamp));
            Assert.AreEqual(1f, view.Fade.resolvedStyle.opacity, 0.01f);
            Assert.AreEqual(PickingMode.Ignore, view.Fade.pickingMode);

            model.Update(1.2f, false); // past the delivery reveal (1.05)
            view.Refresh();
            yield return Frames();
            Assert.IsTrue(Shown(view.DeliveryRow));
            Assert.IsFalse(Shown(view.TimeRow));
            Assert.AreEqual(model.FadeOpacity, view.Fade.resolvedStyle.opacity, 0.01f);
            Assert.Less(view.Fade.resolvedStyle.opacity, 1f);

            model.Update(0.5f, false);
            view.Refresh();
            yield return Frames();
            Assert.IsTrue(Shown(view.TimeRow));
            Assert.IsFalse(Shown(view.SummaryRow));

            model.Update(0.8f, false);
            model.Update(0.6f, false);
            view.Refresh();
            yield return Frames();
            Assert.IsTrue(Shown(view.SummaryRow));
            Assert.AreEqual(model.SummaryText, view.Summary.text);

            model.Update(0.016f, true);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual("+" + total, view.Summary.text);
            Assert.IsTrue(Shown(view.Stamp));
            Assert.AreEqual(model.Stamp.ScaleFactor, view.Stamp.resolvedStyle.scale.value.x, 0.001f);

            model.Update(1.0f, false);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(1f, view.Stamp.resolvedStyle.scale.value.x, 0.001f);
            Assert.AreEqual(-10f, view.Stamp.resolvedStyle.rotate.angle.ToDegrees(), 0.01f);
            Assert.AreEqual(model.FadeOpacity, view.Fade.resolvedStyle.opacity, 0.01f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PostLevel_SurvivesDisableEnable()
        {
            var model = Level(out _);
            var view = Make<PostLevelStatsView>();
            view.Bind(model, "First Departure", Panel());
            yield return Frames();
            model.Update(1.2f, false);
            view.gameObject.SetActive(false);
            yield return Frames();
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual(15, view.Punches.childCount);
            Assert.AreEqual("First Departure", view.StageTitle.text);
            Assert.IsTrue(Shown(view.DeliveryRow));
            Assert.AreEqual(600f, view.Paper.layout.width, Tol);
        }

        // FailScreenView

        private static FailScreenModel Fail() => new FailScreenModel(FailureReason.TrainFrozenFurnaceOut);

        private static readonly PostDeathStatsSnapshot Stats = new PostDeathStatsSnapshot(7, 1234.5f, 3, 2);

        [UnityTest]
        public IEnumerator Fail_LayoutFollowsGum()
        {
            var model = Fail();
            model.Update(0.016f, true); // shows the stamp, hidden elements have no layout
            var view = Make<FailScreenView>();
            view.Bind(model, Stats, "N. 03 / F · FILED 17 JAN 2026 · 04:53", Panel());
            yield return Frames();
            var root = view.Root.layout;
            var paper = view.Paper;
            Assert.AreEqual(600f, paper.layout.width, Tol);
            Assert.AreEqual((root.width - 600f) / 2f, paper.layout.x, Tol);
            Assert.AreEqual((root.height - paper.layout.height) / 2f, paper.layout.y, Tol);
            AssertColor(new Color(20f / 255f, 0f, 0f), view.Root.resolvedStyle.backgroundColor);
            var vig = view.Vignette;
            Assert.AreEqual(root.width, vig.layout.width, Tol);
            Assert.AreEqual(root.height, vig.layout.height, Tol);
            Assert.AreEqual(199f / 255f, vig.resolvedStyle.backgroundColor.a, 0.01f);
            Assert.AreEqual(0f, vig.resolvedStyle.backgroundColor.r, 0.01f);
            Assert.IsTrue(HasImage(view.Root.Q("Background")));

            Assert.AreEqual(175f, view.Root.Q("Hole1").layout.x, Tol);
            Assert.AreEqual(375f, view.Root.Q("Hole2").layout.x, Tol);
            Assert.AreEqual(13f, view.Root.Q("Hole1").layout.y, Tol);
            Assert.AreEqual(40f, view.Root.Q<Label>("TitleText").resolvedStyle.fontSize, 0.01f);
            Assert.AreEqual(480f, view.Stamp.layout.x, Tol);
            Assert.AreEqual(39f, view.Stamp.layout.y, Tol);
            AssertStretched(view.Root.Q("Hole1"));
            AssertStretched(view.Stamp);

            // Stats grid: two columns of 250, label 65 percent of it, value 35 percent.
            var line = view.Root.Q("EnemiesLine");
            Assert.AreEqual(250f, line.layout.width, Tol);
            Assert.AreEqual(162.5f, line.Q<Label>(className: "fsv-stat-label").layout.width, Tol);
            Assert.AreEqual(87.5f, line.Q(className: "fsv-stat-item").layout.width, Tol);
            // Two 19 px lines with 10 px between them fill the 48 px column.
            Assert.AreEqual(29f, view.Root.Q("DistanceLine").layout.y, Tol);
            Assert.AreEqual(48f, view.Root.Q("DistanceLine").layout.yMax, Tol);
            Assert.Greater(view.Root.Q("Column2").layout.x, view.Root.Q("Column1").layout.x + 200f);
            Assert.Greater(view.Root.Q("StagesLine").worldBound.x, view.Root.Q("EnemiesLine").worldBound.x);
            Assert.AreEqual(view.Root.Q("EnemiesLine").worldBound.y, view.Root.Q("StagesLine").worldBound.y, Tol);

            // Return button is right aligned at the bottom of the box, inside the paper.
            var mainBox = view.Root.Q("MainBox");
            Assert.AreEqual(mainBox.layout.width, Right(view.ReturnButton), Tol);
            Assert.LessOrEqual(view.ReturnButton.worldBound.yMax, paper.worldBound.yMax + Tol);
        }

        [UnityTest]
        public IEnumerator Fail_TextsFollowModel()
        {
            var model = Fail();
            var view = Make<FailScreenView>();
            view.Bind(model, Stats, "LINE", Panel());
            yield return Frames();
            Assert.AreEqual("LINE", view.IncidentLine.text);
            Assert.AreEqual(PostDeathStatsText.EnemiesDefeated(Stats), view.EnemiesStat.text);
            Assert.AreEqual("1234 m", view.DistanceStat.text);
            Assert.AreEqual("3", view.StagesStat.text);
            Assert.AreEqual("2", view.UpgradesStat.text);
            Assert.AreEqual("Return", view.ReturnButton.Q<Label>("Text").text);
            Assert.IsTrue(HasImage(view.ReturnButton.Q("Icon")), "A glyph");
            Assert.AreEqual("", view.CauseText.text);
            Assert.IsFalse(Shown(view.Stamp));

            model.Update(0.2f, false);
            view.Refresh();
            Assert.AreEqual(model.CauseText, view.CauseText.text);
            Assert.IsNotEmpty(view.CauseText.text);

            model.Update(0.016f, true);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(FailureReasonText.Get(FailureReason.TrainFrozenFurnaceOut), view.CauseText.text);
            Assert.IsTrue(Shown(view.Stamp));
            Assert.AreEqual(1.4f, view.Stamp.resolvedStyle.scale.value.x, 0.001f);
            // Gum base 10 plus pop offset -8 is 2, negated.
            Assert.AreEqual(-2f, view.Stamp.resolvedStyle.rotate.angle.ToDegrees(), 0.01f);

            model.Update(1f, false);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(1f, view.Stamp.resolvedStyle.scale.value.x, 0.001f);
            Assert.AreEqual(-10f, view.Stamp.resolvedStyle.rotate.angle.ToDegrees(), 0.01f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Fail_SurvivesDisableEnable()
        {
            var model = Fail();
            var view = Make<FailScreenView>();
            view.Bind(model, Stats, "LINE", Panel());
            yield return Frames();
            model.Update(0.2f, false);
            view.gameObject.SetActive(false);
            yield return Frames();
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual("LINE", view.IncidentLine.text);
            Assert.AreEqual(model.CauseText, view.CauseText.text);
            Assert.AreEqual(600f, view.Paper.layout.width, Tol);
        }
    }
}
