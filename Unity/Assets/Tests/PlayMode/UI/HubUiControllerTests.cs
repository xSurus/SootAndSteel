using System.Collections;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.Map;
using Gamelab.Players;
using Gamelab.Run;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class HubUiControllerTests
    {
        private sealed class FakeInput : IInputActions
        {
            public bool Interact, Grab, Back;
            public bool IsUpJustPressed() => false;
            public bool IsDownJustPressed() => false;
            public bool IsLeftJustPressed() => false;
            public bool IsRightJustPressed() => false;
            public bool IsPickupJustPressed() => false;
            public bool IsPauseJustPressed() => false;
            public System.Numerics.Vector2 GetMovement() => System.Numerics.Vector2.Zero;
            public bool IsInteractJustPressed() => Interact;
            public bool IsInteractHeld() => false;
            public bool IsInteractJustReleased() => false;
            public bool IsGrabJustPressed() => Grab;
            public bool IsGrabHeld() => false;
            public bool IsGrabJustReleased() => false;
            public bool IsPickupHeld() => false;
            public bool IsStartJustPressed() => false;
            public bool IsStartHeld() => false;
            public bool IsBackButtonJustPressed() => Back;
            public bool IsBackButtonHeld() => false;
        }

        private sealed class FakeProjector : IWorldToScreen
        {
            public Vector2 ScreenSize { get; set; } = new Vector2(1920, 1080);
            public Vector2 Scale = Vector2.one;
            public Vector2 ToScreen(Vector2 worldPx) => new Vector2(worldPx.x * Scale.x, worldPx.y * Scale.y);
        }

        private sealed class Item : Gamelab.PhysicalEntities.Interfaces.ITooltipable
        {
            public System.Numerics.Vector2 Pos;
            public bool Visible = true;
            public System.Numerics.Vector2 Position => Pos;
            public string GetTitle() => "Item";
            public string GetDescription() => "Text";
            public bool IsVisible => Visible;
        }

        private readonly List<Object> cleanup = new List<Object>();
        private RunCredits credits;
        private HubDepartureModel departure;
        private CraftingHelpModel help;
        private FakeInput p0, p1;
        private FakeProjector projector;
        private int pending;
        private int departs;
        private HubUiController ctl;

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.DestroyImmediate(o);
            cleanup.Clear();
        }

        private IEnumerator Setup()
        {
            pending = 0;
            departs = 0;
            credits = new RunCredits();
            departure = new HubDepartureModel(0.75f);
            help = new CraftingHelpModel(credits);
            p0 = new FakeInput();
            p1 = new FakeInput();
            projector = new FakeProjector();
            var slots = new List<PlayerSlot> { new PlayerSlot(0, p0), new PlayerSlot(1, p1) };
            var go = new GameObject("HubUi");
            cleanup.Add(go);
            ctl = go.AddComponent<HubUiController>();
            ctl.enabled = false; // ticks are driven by the test
            ctl.Bind(credits, departure, help, () => slots, () => pending, projector);
            ctl.DepartRequested += () => departs++;
            yield return null;
            yield return null;
            ctl.Tick(0.01f); // registers the joined players
        }

        private void Press(bool back = false, bool interact = false, bool grab = false, float dt = 0.01f)
        {
            p0.Back = back; p0.Interact = interact; p0.Grab = grab;
            ctl.Tick(dt);
            p0.Back = p0.Interact = p0.Grab = false;
        }

        [UnityTest]
        public IEnumerator FullFlow_ReadyDecisionGrabInteractDepart()
        {
            yield return Setup();
            pending = 1;
            Press(); // the lever seam reads the pending count of the last tick
            departure.ToggleReady(0);
            departure.ToggleReady(1);
            yield return null;
            Assert.IsTrue(departure.IsDecisionOpen);
            Assert.AreEqual("Ready for departure!", ctl.OverlayView.ReadyLabel.text);
            Assert.AreEqual(DisplayStyle.Flex, ctl.BubbleView.Root.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, ctl.BubbleView.Interactions.resolvedStyle.display);

            Press(dt: 0.3f); // input block
            Press(grab: true);
            Assert.IsFalse(departure.IsDecisionOpen);
            Assert.IsFalse(departure.IsReady(1), "Grab un-readies the deciding player");
            Assert.AreEqual(0, departs);

            departure.ToggleReady(1);
            Assert.IsTrue(departure.IsDecisionOpen);
            Press(dt: 0.3f);
            Press(interact: true);
            Assert.IsFalse(departure.IsDecisionOpen);
            Press(dt: 0.5f);
            Assert.AreEqual(0, departs);
            Press(dt: 0.5f);
            Assert.AreEqual(1, departs);
        }

        [UnityTest]
        public IEnumerator NoPendingItems_HoldTimerRaisesDepartRequested()
        {
            yield return Setup();
            departure.ToggleReady(0);
            departure.ToggleReady(1);
            Press(dt: 0.5f);
            Assert.AreEqual(0, departs);
            Press(dt: 0.5f);
            Assert.AreEqual(1, departs);
        }

        [UnityTest]
        public IEnumerator Back_TogglesCraftingHelp()
        {
            yield return Setup();
            yield return null;
            Assert.IsTrue(help.Visible);
            Assert.AreEqual(DisplayStyle.Flex, ctl.HelpView.Root.style.display.value);
            Press(back: true);
            Assert.IsFalse(help.Visible);
            Assert.AreEqual(DisplayStyle.None, ctl.HelpView.Root.style.display.value);
            Press(back: true);
            Assert.IsTrue(help.Visible);
        }

        [UnityTest]
        public IEnumerator Tooltip_PositionsFromProjectorViaTooltipLayout()
        {
            yield return Setup();
            var item = new Item { Pos = new System.Numerics.Vector2(960, 540) };
            ctl.Tooltips.Set("a", item, TooltipKind.Buyable);
            yield return null;
            ctl.Tick(0.01f);
            var root = ctl.Tooltips.ViewOf("a").Root;
            // Ideal: x = 960 - 185, above = 540 - 40 - 266 - 25.
            Assert.AreEqual(775f, root.style.left.value.value, 0.01f);
            Assert.AreEqual(209f, root.style.top.value.value, 0.01f);

            // Half-size screen: screen pixels map to canvas units by 1 / 0.5.
            projector.ScreenSize = new Vector2(960, 540);
            projector.Scale = new Vector2(0.5f, 0.5f);
            ctl.Tick(0.01f);
            Assert.AreEqual(775f, root.style.left.value.value, 0.01f);
            Assert.AreEqual(209f, root.style.top.value.value, 0.01f);

            item.Visible = false;
            ctl.Tick(0.01f);
            Assert.AreEqual(DisplayStyle.None, root.style.display.value);
            ctl.Tooltips.Clear("a");
            Assert.AreEqual(0, ctl.Tooltips.Count);
        }

        [UnityTest]
        public IEnumerator Tooltip_LeftButtonFollowsCreditsAndKind()
        {
            yield return Setup();
            var item = new Item { Pos = new System.Numerics.Vector2(960, 540) };
            ctl.Tooltips.Set("a", item, TooltipKind.Counter);
            yield return null;
            Assert.AreEqual(DisplayStyle.None, ctl.Tooltips.ViewOf("a").LeftButton.style.display.value);
            ctl.Tooltips.Set("a", item, TooltipKind.Station);
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, ctl.Tooltips.ViewOf("a").LeftButton.style.display.value);
        }

        [UnityTest]
        public IEnumerator Tooltips_Overlapping_AreSeparated()
        {
            yield return Setup();
            ctl.Tooltips.Set("a", new Item { Pos = new System.Numerics.Vector2(960, 540) }, TooltipKind.Station);
            ctl.Tooltips.Set("b", new Item { Pos = new System.Numerics.Vector2(980, 540) }, TooltipKind.Station);
            yield return null;
            ctl.Tick(0.01f);
            var a = ctl.Tooltips.ViewOf("a").Root;
            var b = ctl.Tooltips.ViewOf("b").Root;
            float dx = Mathf.Abs(a.style.left.value.value - b.style.left.value.value);
            float dy = Mathf.Abs(a.style.top.value.value - b.style.top.value.value);
            Assert.IsTrue(dx >= TooltipLayout.PanelWidth || dy >= TooltipLayout.PanelHeight,
                "panels overlap: dx " + dx + " dy " + dy);
        }

        [UnityTest]
        public IEnumerator CameraWorldToScreen_MirroredCamera_MapsSrcYDownToTopLeftPixels()
        {
            var go = new GameObject("Cam");
            cleanup.Add(go);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<MirroredCamera>();
            yield return null;
            Assert.Greater(cam.pixelHeight, 0);
            var proj = new CameraWorldToScreen(cam);
            Vector2 c = proj.ToScreen(Vector2.zero);
            Assert.AreEqual(cam.pixelWidth / 2f, c.x, 1f);
            Assert.AreEqual(cam.pixelHeight / 2f, c.y, 1f);
            // 200 px = 2 m = 0.2 of the 10 m visible height. Larger Src Y is lower on screen (larger top-left Y).
            float unit = cam.pixelHeight * 0.2f;
            Assert.AreEqual(unit, proj.ToScreen(new Vector2(0f, 200f)).y - c.y, 1f);
            Assert.AreEqual(unit, proj.ToScreen(new Vector2(200f, 0f)).x - c.x, 1f);
            Assert.AreEqual(new Vector2(cam.pixelWidth, cam.pixelHeight), proj.ScreenSize);
        }
    }
}
