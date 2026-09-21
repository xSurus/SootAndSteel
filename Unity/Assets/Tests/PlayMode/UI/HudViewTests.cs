using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Levels;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class HudViewTests
    {
        // Pixel snapping at the 640x480 test panel moves edges by up to a canvas unit or two.
        private const float Tol = 3f;
        private const float Range = 455.5f;

        private readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.DestroyImmediate(o);
            cleanup.Clear();
        }

        private static LevelDefinition Level(params float[] distances)
        {
            var def = new LevelDefinition { LevelDistance = 1000f };
            foreach (float d in distances) def.SpawnEvents.Add(new SpawnEvent { Distance = d });
            return def;
        }

        private HudView Make(HudModel model)
        {
            var go = new GameObject("HudView");
            cleanup.Add(go);
            go.AddComponent<UIDocument>();
            var view = go.AddComponent<HudView>();
            var ps = UiPanel.Create();
            cleanup.Add(ps);
            view.Bind(model, ps);
            return view;
        }

        private static IEnumerator Frames(int n = 3) { for (int i = 0; i < n; i++) yield return null; }

        private static bool HasImage(VisualElement e) =>
            e.resolvedStyle.backgroundImage.texture != null || e.resolvedStyle.backgroundImage.sprite != null;

        private static float Angle(VisualElement e) => e.resolvedStyle.rotate.angle.ToDegrees();

        private static float Wrap(float deg) => ((deg % 360f) + 360f) % 360f;

        [UnityTest]
        public IEnumerator Layout_FollowsGum()
        {
            var view = Make(new HudModel(Level()));
            yield return Frames();
            Assert.AreEqual(969.2f, view.Parchment.layout.width, Tol);
            Assert.AreEqual(200f, view.Parchment.layout.height, Tol);
            Assert.AreEqual(-28f, view.Parchment.layout.y, Tol);
            Assert.AreEqual(181f, view.Track.layout.x, Tol);
            Assert.AreEqual(3f, view.Track.layout.y, Tol);
            Assert.AreEqual(503.5f, view.Track.layout.width, Tol);
            Assert.AreEqual(428.5f, view.GoalX.layout.x, Tol);
            Assert.AreEqual(68f, view.GoalX.layout.y, Tol);
            Assert.AreEqual(64f, view.GoalX.layout.width, Tol);
            Assert.AreEqual(192f, view.Speedometer.layout.width, Tol);
            Assert.AreEqual(194f, view.Speedometer.layout.height, Tol);
            Assert.AreEqual(4f, view.Speedometer.layout.x, Tol);
            Assert.AreEqual(-24f, view.Speedometer.layout.y, Tol);
            Assert.AreEqual(89.6f, view.NeedleContainer.layout.x, Tol);
            Assert.AreEqual(1920f, view.Frost1.layout.width, Tol);
            Assert.AreEqual(1080f, view.Frost1.layout.height, Tol);
            Assert.AreEqual(view.Root.layout.width, view.Root.parent.layout.width, Tol);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Images_Load()
        {
            var view = Make(new HudModel(Level(500f)));
            yield return Frames();
            foreach (var e in new[] { view.Parchment, view.Needle, view.TrainMarker, view.GoalX, view.Dots[0], view.Frost1, view.Frost2, view.Frost3 })
                Assert.IsTrue(HasImage(e), e.name + " " + string.Join(",", e.GetClasses()));
        }

        [UnityTest]
        public IEnumerator Needle_FollowsModel()
        {
            var model = new HudModel(Level());
            var view = Make(model);
            foreach (float speed in new[] { 0f, 300f, 600f })
            {
                for (int i = 0; i < 60; i++) model.Update(0f, speed, 100f, 100f, 600f);
                view.Refresh();
                yield return Frames();
                Assert.AreEqual(Wrap(-model.NeedleDegrees), Wrap(Angle(view.NeedleContainer)), 0.5f, "speed " + speed);
            }
            // Speed 600 has converged to the fast end, so it must differ from speed 0.
            Assert.AreEqual(Wrap(-HudMath.NeedleDegrees(600f)), Wrap(Angle(view.NeedleContainer)), 0.5f);
        }

        [UnityTest]
        public IEnumerator TrainMarker_CentredOnDistanceRatio()
        {
            var model = new HudModel(Level());
            var view = Make(model);
            foreach (var (dist, ratio) in new[] { (0f, 0f), (500f, 0.5f), (1000f, 1f) })
            {
                model.Update(dist, 0f, 100f, 100f, 600f);
                view.Refresh();
                yield return Frames();
                Assert.AreEqual(ratio * Range, view.TrainMarker.layout.x + view.TrainMarker.layout.width / 2f, Tol);
            }
        }

        [UnityTest]
        public IEnumerator Dots_MatchSpawnEventsAndRebuildOnLevelChange()
        {
            var model = new HudModel(Level(0f, 250f, 1000f));
            var view = Make(model);
            yield return Frames();
            Assert.AreEqual(3, view.Dots.Count);
            float[] ratios = { 0f, 0.25f, 1f };
            for (int i = 0; i < 3; i++)
            {
                var d = view.Dots[i];
                Assert.AreEqual(50f, d.layout.width, Tol);
                Assert.AreEqual(73f, d.layout.y, Tol);
                Assert.AreEqual(ratios[i] * Range, d.layout.x + d.layout.width / 2f, Tol);
            }
            model.SetLevel(Level(500f));
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(1, view.Dots.Count);
            Assert.AreEqual(1, view.Root.Q("Dots").childCount);
            Assert.AreEqual(0.5f * Range, view.Dots[0].layout.x + 25f, Tol);
        }

        [UnityTest]
        public IEnumerator Frost_FollowsTemperature()
        {
            var model = new HudModel(Level());
            var view = Make(model);
            model.Update(0f, 0f, 100f, 100f, 600f);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(0f, view.Frost1.resolvedStyle.opacity, 0.01f);
            Assert.AreEqual(0f, view.Frost3.resolvedStyle.opacity, 0.01f);

            model.Update(0f, 0f, 0f, 100f, 600f);
            view.Refresh();
            yield return Frames();
            foreach (var f in new[] { view.Frost1, view.Frost2, view.Frost3 })
                Assert.AreEqual(1f, f.resolvedStyle.opacity, 0.01f);

            model.Update(0f, 0f, 50f, 100f, 600f);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(0.2857f, view.Frost1.resolvedStyle.opacity, 0.01f);
            Assert.AreEqual(0f, view.Frost2.resolvedStyle.opacity, 0.01f);
            Assert.AreEqual(0f, view.Frost3.resolvedStyle.opacity, 0.01f);
        }

        [UnityTest]
        public IEnumerator AllElementsIgnorePicking()
        {
            var view = Make(new HudModel(Level(100f, 200f)));
            yield return Frames();
            Assert.AreEqual(PickingMode.Ignore, view.Root.pickingMode);
            foreach (var e in view.Root.Query<VisualElement>().ToList())
                Assert.AreEqual(PickingMode.Ignore, e.pickingMode, e.name + " " + string.Join(",", e.GetClasses()));
        }

        [UnityTest]
        public IEnumerator DisableEnable_RebuildsAndReflectsModel()
        {
            var model = new HudModel(Level(0f, 500f));
            var view = Make(model);
            model.Update(500f, 600f, 0f, 100f, 600f);
            view.Refresh();
            yield return Frames();
            view.gameObject.SetActive(false);
            yield return Frames();
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual(2, view.Dots.Count);
            Assert.AreEqual(0.5f * Range, view.TrainMarker.layout.x + 16f, Tol);
            Assert.AreEqual(1f, view.Frost1.resolvedStyle.opacity, 0.01f);
            Assert.AreEqual(Wrap(-model.NeedleDegrees), Wrap(Angle(view.NeedleContainer)), 0.5f);
        }

        [UnityTest]
        public IEnumerator Works_WithTimeScaleZero()
        {
            float old = Time.timeScale;
            Time.timeScale = 0f;
            try
            {
                var model = new HudModel(Level(500f));
                var view = Make(model);
                model.Update(1000f, 0f, 0f, 100f, 600f);
                view.Refresh();
                yield return Frames();
                Assert.AreEqual(Range, view.TrainMarker.layout.x + 16f, Tol);
                Assert.AreEqual(1f, view.Frost3.resolvedStyle.opacity, 0.01f);
            }
            finally { Time.timeScale = old; }
        }
    }
}
