using System.Collections;
using System.Collections.Generic;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class JoinSkipViewsTests
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
        private static bool HasImage(VisualElement e) =>
            e.resolvedStyle.backgroundImage.texture != null || e.resolvedStyle.backgroundImage.sprite != null;
        private static Object Image(VisualElement e) =>
            (Object)e.resolvedStyle.backgroundImage.sprite ?? e.resolvedStyle.backgroundImage.texture;

        // JoinScreenView

        private JoinScreenView Join(int[] count, out JoinScreenModel model)
        {
            var m = new JoinScreenModel(() => count[0]);
            model = m;
            var view = Make<JoinScreenView>();
            view.Bind(m, Panel());
            return view;
        }

        [UnityTest]
        public IEnumerator Join_LayoutFollowsGum()
        {
            var view = Join(new[] { 0 }, out _);
            yield return Frames();
            Assert.AreEqual("Enter the train", view.Title.text);
            Assert.AreEqual(60f, view.Title.resolvedStyle.fontSize, 0.01f);
            Assert.AreEqual(54f, view.Title.layout.x, Tol);
            Assert.AreEqual(108f, view.Title.layout.y, Tol);
            Assert.AreEqual(1728f, view.Title.layout.width, Tol);
            Assert.AreEqual(Color.black, view.Root.resolvedStyle.backgroundColor);

            Assert.AreEqual(96f, view.Figures.layout.x, Tol);
            Assert.AreEqual(378f, view.Figures.layout.y, Tol);
            Assert.AreEqual(1728f, view.Figures.layout.width, Tol);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(432f * i, view.Slot(i).layout.x, Tol);
                Assert.AreEqual(432f, view.Slot(i).layout.width, Tol);
                var player = view.Figure(i).parent;
                Assert.AreEqual(21.6f, player.layout.x, Tol);
                Assert.AreEqual(345.6f, player.layout.width, Tol);
                Assert.AreEqual(345.6f, player.layout.height, Tol);
                Assert.AreEqual(345.6f, view.Figure(i).layout.width, Tol);
                Assert.AreEqual(345.6f, view.Figure(i).layout.height, Tol);
                // Join button centred on 55 percent of the Player width, 110 percent down.
                var join = view.JoinButton(i);
                // translate is not part of layout rects.
                Assert.AreEqual(190f, join.layout.x + join.resolvedStyle.translate.x + join.layout.width / 2f, Tol);
                Assert.AreEqual(380f, join.layout.y, Tol);
                Assert.AreEqual(32f, join.layout.height, Tol);
            }
        }

        [UnityTest]
        public IEnumerator Join_SlotVisualsFollowJoinedCount()
        {
            var count = new[] { 0 };
            var view = Join(count, out var model);
            yield return Frames();
            var silhouette = Resources.Load<Sprite>("UI/Art/Silhouette");
            Assert.IsNotNull(silhouette);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreSame(silhouette, Image(view.Figure(i)));
                Assert.AreEqual("Join", view.JoinText(i).text);
                Assert.IsTrue(HasImage(view.JoinIcon(i)));
            }
            var glyphA = Image(view.JoinIcon(0));

            count[0] = 1;
            model.Refresh();
            yield return Frames();
            Assert.AreEqual("Press start to advance", view.Title.text);
            Assert.AreSame(Resources.Load<Sprite>("UI/Art/IdleA0"), Image(view.Figure(0)));
            Assert.AreEqual("Joined", view.JoinText(0).text);
            Assert.AreNotSame(glyphA, Image(view.JoinIcon(0)));
            Assert.AreSame(glyphA, Image(view.JoinIcon(1)));
            Assert.AreSame(silhouette, Image(view.Figure(1)));
            Assert.AreEqual("Join", view.JoinText(1).text);

            count[0] = 4;
            model.Refresh();
            yield return Frames();
            string[] heads = { "IdleA0", "IdleA3", "IdleA1", "IdleA2" };
            for (int i = 0; i < 4; i++)
            {
                Assert.AreSame(Resources.Load<Sprite>("UI/Art/" + heads[i]), Image(view.Figure(i)));
                Assert.AreEqual("Joined", view.JoinText(i).text);
            }

            count[0] = 0;
            model.Refresh();
            yield return Frames();
            Assert.AreEqual("Enter the train", view.Title.text);
            Assert.AreSame(silhouette, Image(view.Figure(3)));
        }

        [UnityTest]
        public IEnumerator Join_SurvivesDisableEnable()
        {
            var count = new[] { 2 };
            var view = Join(count, out var model);
            model.Refresh();
            yield return Frames();
            view.gameObject.SetActive(false);
            yield return Frames();
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual("Press start to advance", view.Title.text);
            Assert.AreSame(Resources.Load<Sprite>("UI/Art/IdleA3"), Image(view.Figure(1)));
            Assert.AreEqual("Join", view.JoinText(2).text);
            Assert.AreEqual(1728f, view.Figures.layout.width, Tol);
            count[0] = 3;
            model.Refresh();
            yield return Frames();
            Assert.AreEqual("Joined", view.JoinText(2).text);
        }

        // SkipTutorialView

        private SkipTutorialView Skip(SkipTutorialModel model)
        {
            var view = Make<SkipTutorialView>();
            view.Bind(model, Panel());
            return view;
        }

        [UnityTest]
        public IEnumerator Skip_LayoutAndTexts()
        {
            var view = Skip(new SkipTutorialModel());
            yield return Frames();
            var host = view.Root.parent.layout;
            Assert.AreEqual("Hold to skip tutorial", view.Text.text);
            Assert.AreEqual(20f, view.Text.resolvedStyle.fontSize, 0.01f);
            Assert.AreEqual(host.width - 50f, view.Root.layout.xMax, Tol);
            Assert.AreEqual(host.height - 40f, view.Root.layout.yMax, Tol);
            Assert.AreEqual(50f, view.Text.layout.x, Tol);
            Assert.AreEqual(182f, view.Text.layout.width, Tol);
            Assert.IsTrue(HasImage(view.Icon));
            Assert.AreEqual(32f, view.Icon.layout.width, Tol);
            Assert.AreEqual(26f, view.Icon.layout.x + 16f, Tol);
            Assert.AreEqual(Resources.Load<Texture2D>("UI/Art/spr_xbox_select_32").width, 32);
        }

        [UnityTest]
        public IEnumerator Skip_BarFollowsRatio()
        {
            var model = new SkipTutorialModel();
            var view = Skip(model);
            yield return Frames();
            float center = view.Root.layout.width / 2f;
            Assert.AreEqual(0f, view.ProgressBar.layout.width, Tol);
            Assert.AreEqual(5f, view.ProgressBar.layout.height, Tol);
            Assert.AreEqual(45f, view.ProgressBar.parent.layout.y, Tol);

            model.Update(0.375f, true); // progress 0.75 of 1.5
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(0.5f, model.ProgressRatio, 0.001f);
            Assert.AreEqual(108.5f, view.ProgressBar.layout.width, Tol);
            Assert.AreEqual(105.5f, view.Fill.layout.width, Tol);
            Assert.AreEqual(2f, view.Fill.layout.height, Tol);
            // Shrinks about the centre.
            Assert.AreEqual(center, view.ProgressBar.layout.center.x, Tol);

            model.Update(1f, true);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(217f, view.ProgressBar.layout.width, Tol);
            Assert.AreEqual(214f, view.Fill.layout.width, Tol);
            Assert.AreEqual(center, view.ProgressBar.layout.center.x, Tol);

            model.Update(1f, false);
            view.Refresh();
            yield return Frames();
            Assert.AreEqual(0f, view.ProgressBar.layout.width, Tol);
        }

        [UnityTest]
        public IEnumerator Skip_SurvivesDisableEnable()
        {
            var model = new SkipTutorialModel();
            var view = Skip(model);
            model.Update(0.375f, true);
            view.Refresh();
            yield return Frames();
            view.gameObject.SetActive(false);
            yield return Frames();
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual("Hold to skip tutorial", view.Text.text);
            Assert.AreEqual(108.5f, view.ProgressBar.layout.width, Tol);
            Assert.IsTrue(HasImage(view.Icon));
        }
    }
}
