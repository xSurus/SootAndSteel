using System;
using System.Collections;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.UI.Runtime;
using Gamelab.UI.ViewModels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class MenuScreensTests
    {
        class FakeInputActions : IInputActions
        {
            public bool Up, Down, Left, Right, Pickup, Pause;
            public void Clear() { Up = Down = Left = Right = Pickup = Pause = false; }
            public bool IsUpJustPressed() => Up;
            public bool IsDownJustPressed() => Down;
            public bool IsLeftJustPressed() => Left;
            public bool IsRightJustPressed() => Right;
            public bool IsPickupJustPressed() => Pickup;
            public bool IsPauseJustPressed() => Pause;
            public System.Numerics.Vector2 GetMovement() => System.Numerics.Vector2.Zero;
            public bool IsInteractJustPressed() => false;
            public bool IsInteractHeld() => false;
            public bool IsInteractJustReleased() => false;
            public bool IsGrabJustPressed() => false;
            public bool IsGrabHeld() => false;
            public bool IsGrabJustReleased() => false;
            public bool IsPickupHeld() => false;
            public bool IsStartJustPressed() => false;
            public bool IsStartHeld() => false;
            public bool IsBackButtonJustPressed() => false;
            public bool IsBackButtonHeld() => false;
        }

        class FakeVolume : IVolumeSettings
        {
            public float Master = 0.2f, Music = 0.5f, Sfx = 1f;
            public float MasterVolume => Master;
            public float MusicVolume => Music;
            public float SfxVolume => Sfx;
            public void SetMasterVolume(float v) => Master = Mathf.Clamp01(v);
            public void SetMusicVolume(float v) => Music = Mathf.Clamp01(v);
            public void SetSfxVolume(float v) => Sfx = Mathf.Clamp01(v);
        }

        private readonly List<UnityEngine.Object> cleanup = new List<UnityEngine.Object>();

        private float savedTimeScale;

        [SetUp]
        public void SetUp()
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            cleanup.Clear();
            Time.timeScale = savedTimeScale;
        }

        private static MainMenuViewModel Menu(bool withContinue)
        {
            var e = new List<MainMenuViewModel.Entry>();
            if (withContinue) e.Add(new MainMenuViewModel.Entry("Continue", null));
            e.Add(new MainMenuViewModel.Entry("New Game", null));
            e.Add(new MainMenuViewModel.Entry("Options", null));
            e.Add(new MainMenuViewModel.Entry("Quit", null));
            return new MainMenuViewModel(e);
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

        private static string LabelOf(VisualElement row) => ((Label)row[1]).text;
        private static bool Selected(VisualElement row, string cls) => row.ClassListContains(cls);

        [UnityTest]
        public IEnumerator MainMenu_RowsFollowEntries()
        {
            var view = Make<MainMenuView>();
            view.Bind(Menu(false), Panel());
            yield return Frames();
            Assert.AreEqual(3, view.Rows.Count);
            Assert.AreEqual("New Game", LabelOf(view.Rows[0]));
            var view2 = Make<MainMenuView>();
            view2.Bind(Menu(true), Panel());
            yield return Frames();
            Assert.AreEqual(4, view2.Rows.Count);
            Assert.AreEqual("Continue", LabelOf(view2.Rows[0]));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator MainMenu_SelectionClassAndTagsFollowModel()
        {
            var vm = Menu(true);
            var view = Make<MainMenuView>();
            view.Bind(vm, Panel());
            vm.EnsurePlayer(0);
            view.Refresh();
            yield return Frames();
            Assert.IsTrue(Selected(view.Rows[0], "mm-row--selected"));
            Assert.AreEqual("P1", ((Label)view.Rows[0][0]).text);
            vm.EnsurePlayer(1);
            vm.MoveSelectionDown(0);
            vm.MoveSelectionDown(1);
            vm.MoveSelectionDown(1);
            yield return null;
            Assert.IsFalse(Selected(view.Rows[0], "mm-row--selected"));
            Assert.IsTrue(Selected(view.Rows[1], "mm-row--selected"));
            Assert.IsTrue(Selected(view.Rows[2], "mm-row--selected"));
            Assert.AreEqual("P1", ((Label)view.Rows[1][0]).text);
            Assert.AreEqual("P2", ((Label)view.Rows[2][0]).text);
            vm.MoveSelectionUp(1);
            Assert.AreEqual("P1 P2", ((Label)view.Rows[1][0]).text);
        }

        [UnityTest]
        public IEnumerator MainMenu_UnsubscribesOnDestroy()
        {
            var vm = Menu(false);
            var view = Make<MainMenuView>();
            view.Bind(vm, Panel());
            var row = view.Rows[1];
            yield return null;
            view.enabled = false;
            yield return null;
            vm.MoveSelectionDown(0);
            vm.SetSelection(0, 1);
            Assert.IsFalse(Selected(row, "mm-row--selected"), "disabled view must not update");
            view.enabled = true;
            yield return null;
            UnityEngine.Object.Destroy(view.gameObject);
            yield return null;
            vm.SetSelection(0, 2);
            Assert.IsFalse(Selected(row, "mm-row--selected"), "destroyed view must not update");
            Assert.IsFalse(Selected(view.Rows[2], "mm-row--selected"));
        }

        [UnityTest]
        public IEnumerator PauseView_SurvivesGameObjectReenable()
        {
            var vm = new PauseMenuModel(new OptionsViewModel(new FakeVolume()));
            var view = Make<PauseMenuView>();
            view.Bind(vm, Panel());
            yield return Frames();
            Assert.AreEqual(DisplayStyle.None, view.Root.style.display.value);
            view.gameObject.SetActive(false);
            yield return null;
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual(4, view.Rows.Count);
            Assert.AreEqual(DisplayStyle.None, view.Root.style.display.value, "still hidden while not paused");
            Assert.IsNotNull(view.Root.panel, "root is in the live tree");
            vm.Toggle();
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, view.Root.style.display.value);
            vm.MoveDown();
            yield return null;
            Assert.IsTrue(Selected(view.Rows[1], "pm-row--selected"));
            Assert.IsNotNull(view.Rows[1].panel);
            vm.Toggle();
        }

        [UnityTest]
        public IEnumerator OptionsView_SurvivesGameObjectReenable()
        {
            var vol = new FakeVolume();
            var vm = new OptionsViewModel(vol);
            var view = Make<OptionsMenuView>();
            view.Bind(vm, Panel());
            yield return Frames();
            Assert.AreEqual(DisplayStyle.None, view.Root.style.display.value);
            view.gameObject.SetActive(false);
            yield return null;
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual(3, view.Rows.Count);
            Assert.AreEqual(3, view.Thumbs.Count);
            Assert.AreEqual(DisplayStyle.None, view.Root.style.display.value, "still hidden while closed");
            vm.Open();
            vm.IncreaseSelected();
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, view.Root.style.display.value);
            Assert.IsNotNull(view.Thumbs[0].panel, "thumb is in the live tree");
            Assert.AreEqual("25 %", view.Percents[0].text);
        }

        [UnityTest]
        public IEnumerator MainMenuView_SurvivesGameObjectReenable()
        {
            var vm = Menu(false);
            var view = Make<MainMenuView>();
            view.Bind(vm, Panel());
            yield return Frames();
            view.gameObject.SetActive(false);
            yield return null;
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual(3, view.Rows.Count);
            Assert.IsNotNull(view.Rows[0].panel);
            vm.EnsurePlayer(0);
            vm.SetSelection(0, 2);
            yield return null;
            Assert.IsTrue(Selected(view.Rows[2], "mm-row--selected"));
            Assert.AreEqual("P1", ((Label)view.Rows[2][0]).text);
        }

        [UnityTest]
        public IEnumerator MainMenu_ControllerHighlightsFirstRowForNewPlayerWithoutInput()
        {
            var input = new FakeInputActions();
            var go = new GameObject("controller");
            cleanup.Add(go);
            var c = go.AddComponent<MainMenuController>();
            c.Configure(false, null, null, null, new FakeVolume(), 0.05f,
                () => new List<IInputActions> { input }, () => { });
            yield return Frames();
            Assert.IsTrue(Selected(c.MenuView.Rows[0], "mm-row--selected"));
            Assert.AreEqual("P1", ((Label)c.MenuView.Rows[0][0]).text);
        }

        [UnityTest]
        public IEnumerator MainMenu_LayoutCentredInPanel()
        {
            var view = Make<MainMenuView>();
            view.Bind(Menu(true), Panel());
            yield return Frames(4);
            var root = view.Root.worldBound;
            Assert.Greater(root.width, 0f);
            Rect first = view.Rows[0].worldBound, last = view.Rows[3].worldBound;
            float midY = (first.yMin + last.yMax) / 2f;
            Assert.AreEqual(root.center.y, midY, 1.5f);
            foreach (var r in view.Rows) Assert.AreEqual(root.center.x, r.worldBound.center.x, 1.5f);
            float gap = view.Rows[1].worldBound.yMin - view.Rows[0].worldBound.yMax;
            Assert.AreEqual(10f * root.height / 1080f, gap, 1.5f);
        }

        [UnityTest]
        public IEnumerator Options_HiddenUntilOpenAndFollowsVolume()
        {
            var vol = new FakeVolume();
            var vm = new OptionsViewModel(vol);
            var view = Make<OptionsMenuView>();
            view.Bind(vm, Panel());
            yield return Frames();
            Assert.AreEqual(DisplayStyle.None, view.Root.resolvedStyle.display);
            vm.Open();
            yield return Frames();
            Assert.AreEqual(DisplayStyle.Flex, view.Root.resolvedStyle.display);
            Assert.AreEqual("20 %", view.Percents[0].text);
            Assert.AreEqual(115f * 0.2f, view.Thumbs[0].resolvedStyle.left, 2f);
            Assert.AreEqual(115f * 0.5f, view.Thumbs[1].resolvedStyle.left, 2f);
            Assert.AreEqual("100 %", view.Percents[2].text);
            vm.IncreaseSelected();
            yield return Frames();
            Assert.AreEqual("25 %", view.Percents[0].text);
            Assert.AreEqual(115f * 0.25f, view.Thumbs[0].resolvedStyle.left, 2f);
            vm.Close();
            yield return null;
            Assert.AreEqual(DisplayStyle.None, view.Root.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Options_SelectionClassFollowsRow()
        {
            var vm = new OptionsViewModel(new FakeVolume());
            var view = Make<OptionsMenuView>();
            view.Bind(vm, Panel());
            vm.Open();
            yield return Frames();
            Assert.IsTrue(Selected(view.Rows[0], "om-row--selected"));
            vm.MoveSelectionDown();
            yield return Frames();
            Assert.IsFalse(Selected(view.Rows[0], "om-row--selected"));
            Assert.IsTrue(Selected(view.Rows[1], "om-row--selected"));
            Assert.AreEqual(DisplayStyle.Flex, view.Rows[1][0].resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, view.Rows[0][0].resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Options_PaperSize()
        // Layout is snapped to physical pixels, and the headless window is smaller than 1920x1080, so allow 3 units.
        {
            var vm = new OptionsViewModel(new FakeVolume());
            var view = Make<OptionsMenuView>();
            view.Bind(vm, Panel());
            vm.Open();
            yield return Frames(4);
            Assert.AreEqual(380f, view.Paper.resolvedStyle.width, 3f);
            Assert.AreEqual(280f, view.Paper.resolvedStyle.height, 3f);
            Assert.AreEqual(340f, view.Rows[0].resolvedStyle.width, 3f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Controller_FullFlow()
        {
            var input = new FakeInputActions();
            int sounds = 0, newGame = 0;
            var go = new GameObject("controller");
            cleanup.Add(go);
            var c = go.AddComponent<MainMenuController>();
            c.Configure(false, null, () => newGame++, null, new FakeVolume(), 0.05f,
                () => new List<IInputActions> { input }, () => sounds++);
            yield return Frames();
            Assert.AreEqual(3, c.MenuView.Rows.Count);

            input.Down = true; yield return null; input.Clear(); yield return null;
            Assert.AreEqual(1, c.Menu.PlayerSelections[0]);
            Assert.AreEqual(1, sounds);
            Assert.IsTrue(Selected(c.MenuView.Rows[1], "mm-row--selected"));

            input.Up = true; yield return null; input.Clear(); yield return null;
            input.Pickup = true; yield return null; input.Clear(); yield return null;
            Assert.AreEqual(1, newGame);

            input.Down = true; yield return null; input.Clear(); yield return null;
            input.Pickup = true; yield return null; input.Clear(); yield return null;
            Assert.IsTrue(c.Options.IsOpen);
            Assert.AreEqual(DisplayStyle.Flex, c.OptionsView.Root.resolvedStyle.display);

            input.Pause = true; yield return null; input.Clear(); yield return null;
            Assert.IsFalse(c.Options.IsOpen);
            Assert.AreEqual(DisplayStyle.None, c.OptionsView.Root.resolvedStyle.display);
        }

        private static bool Shown(VisualElement root) => root.resolvedStyle.display == DisplayStyle.Flex;

        private PauseMenuController MakePause(FakeInputActions input, FakeSound sound, Action exit)
        {
            var go = new GameObject("pause");
            cleanup.Add(go);
            var c = go.AddComponent<PauseMenuController>();
            c.Configure(new FakeVolume(), 0.05f, () => new List<IInputActions> { input }, () => sound.Count++, exit);
            return c;
        }

        class FakeSound { public int Count; }

        private static IEnumerator Tap(FakeInputActions input, Action press)
        {
            press();
            yield return null;
            input.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Pause_VisibilityPerState()
        {
            var vm = new PauseMenuModel(new OptionsViewModel(new FakeVolume()));
            var pause = Make<PauseMenuView>();
            pause.Bind(vm, Panel());
            var controls = Make<ControlsOverlayView>();
            controls.Bind(vm, Panel());
            yield return Frames();
            Assert.IsFalse(Shown(pause.Root));
            Assert.IsFalse(Shown(controls.Root));
            vm.Toggle();
            yield return Frames();
            Assert.IsTrue(Shown(pause.Root));
            Assert.IsFalse(Shown(controls.Root));
            vm.Options.Open();
            yield return Frames();
            Assert.IsFalse(Shown(pause.Root), "options open hides the pause paper");
            vm.Options.Close();
            vm.MoveDown(); vm.MoveDown(); vm.Confirm();
            yield return Frames();
            Assert.IsFalse(Shown(pause.Root));
            Assert.IsTrue(Shown(controls.Root));
            vm.Toggle();
            yield return Frames();
            Assert.IsFalse(Shown(pause.Root));
            Assert.IsFalse(Shown(controls.Root));
        }

        [UnityTest]
        public IEnumerator Pause_RowsAndSelectionFollowModel()
        {
            var vm = new PauseMenuModel(new OptionsViewModel(new FakeVolume()));
            var view = Make<PauseMenuView>();
            view.Bind(vm, Panel());
            vm.Toggle();
            yield return Frames();
            Assert.AreEqual(4, view.Rows.Count);
            Assert.AreEqual("Controls", LabelOf(view.Rows[2]));
            Assert.IsTrue(Selected(view.Rows[0], "pm-row--selected"));
            Assert.AreEqual(DisplayStyle.Flex, view.Rows[0][0].resolvedStyle.display);
            vm.MoveUp();
            yield return Frames();
            Assert.IsTrue(Selected(view.Rows[3], "pm-row--selected"));
            Assert.IsFalse(Selected(view.Rows[0], "pm-row--selected"));
            Assert.AreEqual(DisplayStyle.None, view.Rows[0][0].resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Pause_LayoutNumbers()
        {
            var vm = new PauseMenuModel(new OptionsViewModel(new FakeVolume()));
            var pause = Make<PauseMenuView>();
            pause.Bind(vm, Panel());
            var controls = Make<ControlsOverlayView>();
            controls.Bind(vm, Panel());
            vm.Toggle();
            yield return Frames(4);
            Assert.AreEqual(380f, pause.Paper.resolvedStyle.width, 3f);
            Assert.AreEqual(353f, pause.Paper.resolvedStyle.height, 3f);
            var paper = pause.Paper.layout;
            var menu = pause.Menu.layout;
            Vector3 shift = pause.Menu.resolvedStyle.translate; // translate is not part of layout
            Assert.AreEqual(0.4852f * paper.width, menu.center.x + shift.x, 4f);
            Assert.AreEqual(0.6537f * paper.height, menu.center.y + shift.y, 4f);
            float[] jitter = { 0f, -5f, 6f, 0f };
            for (int i = 0; i < 4; i++)
                Assert.AreEqual(jitter[i], pause.Rows[i].resolvedStyle.translate.x, 0.5f, "row " + i);
            vm.MoveDown(); vm.MoveDown(); vm.Confirm();
            yield return Frames(4);
            Assert.AreEqual(71f, controls.Image.layout.x, 3f);
            Assert.AreEqual(41f, controls.Image.layout.y, 3f);
            var close = controls.Root.Q("Close");
            Assert.AreEqual(611f, close.layout.x, 3f);
            Assert.AreEqual(493f, close.layout.y, 3f);
            var key = controls.Root.Q("CloseKey");
            // The test panel is 640x480 (scale 0.44), so one screen pixel is 2.25 canvas units.
            Assert.AreEqual(32f, key.resolvedStyle.width, 3f);
            Assert.AreEqual(32f, key.resolvedStyle.height, 3f);
            Assert.NotNull(key.resolvedStyle.backgroundImage.sprite);
            Assert.AreEqual(773f, controls.Paper.resolvedStyle.width, 3f);
            Assert.AreEqual(568f, controls.Paper.resolvedStyle.height, 3f);
            Assert.AreEqual(677f, controls.Image.resolvedStyle.width, 3f);
            Assert.AreEqual(470f, controls.Image.resolvedStyle.height, 3f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PauseController_FullFlow()
        {
            Time.timeScale = 0.5f;
            var input = new FakeInputActions();
            var sound = new FakeSound();
            int exits = 0;
            var c = MakePause(input, sound, () => exits++);
            yield return Frames();
            Assert.IsFalse(Shown(c.PauseView.Root));
            Assert.AreEqual(0.5f, Time.timeScale);

            yield return Tap(input, () => input.Pause = true);
            Assert.IsTrue(c.Model.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(Shown(c.PauseView.Root));

            yield return Tap(input, () => input.Down = true);
            yield return Tap(input, () => input.Down = true);
            Assert.AreEqual(2, c.Model.SelectionIndex);
            Assert.AreEqual(2, sound.Count);
            yield return Tap(input, () => input.Pickup = true);
            Assert.IsTrue(c.Model.ControlsOpen);
            Assert.IsTrue(Shown(c.ControlsView.Root));
            Assert.IsFalse(Shown(c.PauseView.Root));
            yield return Tap(input, () => input.Pickup = true);
            Assert.IsFalse(c.Model.ControlsOpen);
            Assert.IsTrue(Shown(c.PauseView.Root));

            yield return Tap(input, () => input.Up = true);
            yield return Tap(input, () => input.Pickup = true);
            Assert.IsTrue(c.Model.Options.IsOpen);
            Assert.IsTrue(Shown(c.OptionsView.Root));
            Assert.IsFalse(Shown(c.PauseView.Root));
            Assert.Greater(c.OptionsView.GetComponent<UIDocument>().sortingOrder, c.PauseView.GetComponent<UIDocument>().sortingOrder);
            yield return Tap(input, () => input.Pause = true);
            Assert.IsFalse(c.Model.Options.IsOpen);
            Assert.IsTrue(c.Model.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(Shown(c.PauseView.Root));

            yield return Tap(input, () => input.Down = true);
            yield return Tap(input, () => input.Down = true);
            yield return Tap(input, () => input.Pickup = true);
            Assert.AreEqual(1, exits);

            yield return Tap(input, () => input.Pause = true);
            Assert.IsFalse(c.Model.IsPaused);
            Assert.AreEqual(0.5f, Time.timeScale);
            Assert.IsFalse(Shown(c.PauseView.Root));
        }

        [UnityTest]
        public IEnumerator PauseController_ReappliesTimeScaleOnReenableWhilePaused()
        {
            var input = new FakeInputActions();
            var c = MakePause(input, new FakeSound(), null);
            yield return Frames();
            yield return Tap(input, () => input.Pause = true);
            Assert.AreEqual(0f, Time.timeScale);
            c.enabled = false;
            Assert.AreEqual(1f, Time.timeScale);
            c.enabled = true;
            Assert.AreEqual(0f, Time.timeScale);
            yield return Tap(input, () => input.Pause = true);
            Assert.IsFalse(c.Model.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator PauseController_RestoresTimeScaleOnDestroy()
        {
            var input = new FakeInputActions();
            var c = MakePause(input, new FakeSound(), null);
            yield return Frames();
            yield return Tap(input, () => input.Pause = true);
            Assert.AreEqual(0f, Time.timeScale);
            UnityEngine.Object.Destroy(c.gameObject);
            yield return null;
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
