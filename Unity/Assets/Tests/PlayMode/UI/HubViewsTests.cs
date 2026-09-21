using System.Collections;
using System.Collections.Generic;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Run;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class HubViewsTests
    {
        // Pixel snapping at the 640x480 test panel moves edges by up to a canvas unit or two.
        private const float Tol = 3f;

        private sealed class Item : ITooltipable
        {
            public string Category;
            public int? CostValue;
            public string Function;
            public SpriteRect? Icon;
            public System.Numerics.Vector2 Position => System.Numerics.Vector2.Zero;
            public string GetTitle() => "Component Conveyor";
            public string GetDescription() => "Bullet tracks the nearest enemy.";
            public bool IsVisible => true;
            public string CategoryName => Category;
            public string FunctionalityName => Function;
            public SpriteRect? IconSourceRect => Icon;
            public int? Cost => CostValue;
        }

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

        private static bool HasImage(VisualElement e) =>
            e.resolvedStyle.backgroundImage.texture != null || e.resolvedStyle.backgroundImage.sprite != null;

        private static float Right(VisualElement e) => e.layout.x + e.layout.width;

        // HubOverlay

        [UnityTest]
        public IEnumerator HubOverlay_CreditsAndPlayersFollowModels()
        {
            var credits = new RunCredits();
            var dep = new HubDepartureModel(100f);
            var joined = new[] { 0, 1 };
            dep.Update(0.1f, joined, 0, false, false);
            var view = Make<HubOverlayView>();
            view.Bind(credits, dep, Panel());
            yield return Frames();
            Assert.AreEqual("0", view.Amount.text);
            Assert.AreEqual("Players ready:", view.ReadyLabel.text);
            foreach (var icon in view.ReadyIcons) Assert.IsFalse(Shown(icon));

            credits.AddCredits(120);
            dep.ToggleReady(1);
            yield return Frames();
            Assert.AreEqual("120", view.Amount.text);
            Assert.IsFalse(Shown(view.ReadyIcons[0]));
            Assert.IsTrue(Shown(view.ReadyIcons[1]), "second joined player is the red slot");

            dep.ToggleReady(0);
            dep.Update(0.1f, joined, 0, false, false);
            yield return Frames();
            Assert.AreEqual("Ready for departure!", view.ReadyLabel.text);
            Assert.IsTrue(Shown(view.ReadyIcons[0]));
            Assert.IsTrue(Shown(view.ReadyIcons[1]));
            Assert.IsFalse(Shown(view.ReadyIcons[2]));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator HubOverlay_LayoutFollowsGum()
        {
            var dep = new HubDepartureModel(100f);
            dep.Update(0.1f, new[] { 0 }, 0, false, false);
            var view = Make<HubOverlayView>();
            view.Bind(new RunCredits(), dep, Panel());
            yield return Frames();
            var root = view.Root.layout;
            var currency = view.Root.Q("Currency").layout;
            Assert.AreEqual(40f, root.width - Right(view.Root.Q("Currency")), Tol);
            Assert.AreEqual(40f, currency.y, Tol);
            Assert.AreEqual(80f, currency.width, Tol);
            var coin = view.Root.Q("Coin");
            Assert.AreEqual(-7f, coin.layout.x, Tol);
            Assert.AreEqual(64f, coin.resolvedStyle.width, Tol);
            Assert.AreEqual(55f, view.Amount.layout.x, Tol);
            Assert.IsTrue(HasImage(coin));

            var ready = view.Root.Q("PlayersReady");
            Assert.AreEqual(40f, root.width - Right(ready), Tol);
            Assert.AreEqual(20f, root.height - (ready.layout.y + ready.layout.height), Tol);
            Assert.AreEqual(50f, ready.layout.height, Tol);
            Assert.AreEqual(165f, view.ReadyLabel.resolvedStyle.width, Tol);

            dep.ToggleReady(0);
            dep.Update(0.1f, new[] { 0 }, 0, false, false);
            yield return Frames();
            Assert.AreEqual(230f, view.ReadyLabel.resolvedStyle.width, Tol);
            Assert.AreEqual(230f, view.Root.Q("ReadyIcons").layout.x, Tol);
            Assert.AreEqual(12f, view.ReadyIcons[0].resolvedStyle.width, Tol);
            Assert.AreEqual(12f, view.ReadyIcons[0].resolvedStyle.height, Tol);
        }

        // CraftingHelp

        [UnityTest]
        public IEnumerator CraftingHelp_VisibilityFollowsModelAndLayoutFollowsGum()
        {
            var run = new RunCredits();
            var model = new CraftingHelpModel(run);
            var view = Make<CraftingHelpView>();
            view.Bind(model, Panel());
            yield return Frames();
            Assert.IsTrue(Shown(view.Root));
            Assert.AreEqual(444f, view.Root.resolvedStyle.width, Tol);
            Assert.AreEqual(349f, view.Root.resolvedStyle.height, Tol);
            Assert.AreEqual(0f, view.Root.layout.x, Tol);
            Assert.AreEqual(0f, view.Root.layout.y, Tol);
            var wrapper = view.Root.Q("Wrapper");
            Assert.AreEqual(444f * 0.96f, wrapper.resolvedStyle.width, Tol);
            Assert.AreEqual(349f * 0.95f, wrapper.resolvedStyle.height, Tol);
            var title = view.Root.Q<Label>("Title");
            Assert.AreEqual("Crafting Recipes", title.text);
            Assert.AreEqual(200f, title.resolvedStyle.width, Tol);
            Assert.AreEqual(wrapper.layout.width / 2f, title.layout.center.x, Tol);
            Assert.AreEqual(35f, view.Root.Q("Description").layout.y, Tol);
            Assert.That(view.Root.Q<Label>("Description").text, Does.StartWith("A bullet needs three parts"));
            var button = view.Root.Q("ToggleButton");
            Assert.AreEqual(176f, button.resolvedStyle.width, Tol);
            Assert.AreEqual(276f, button.layout.y, Tol);
            Assert.AreEqual(wrapper.layout.width + 12f, Right(button), Tol);
            Assert.AreEqual("Toggle", button.Q<Label>("Text").text);
            Assert.IsTrue(HasImage(button.Q("Icon")));

            model.Toggle();
            Assert.IsFalse(Shown(view.Root));
            model.Toggle();
            Assert.IsTrue(Shown(view.Root));
        }

        // DialogBubble

        [UnityTest]
        public IEnumerator DialogBubble_PassiveDecisionAndHide()
        {
            var model = new DialogBubbleModel();
            var view = Make<DialogBubbleView>();
            view.Bind(model, Panel());
            yield return Frames();
            Assert.IsFalse(Shown(view.Root));

            model.ShowPassive("Hint", "To depart, each player should interact with the lever.");
            yield return Frames();
            Assert.IsTrue(Shown(view.Root));
            Assert.AreEqual("Hint", view.Speaker.text);
            Assert.AreEqual("To depart, each player should interact with the lever.", view.Body.text);
            Assert.IsFalse(Shown(view.Interactions));

            model.ShowDecision("Hint", "Depart anyway?", "No", "Yes");
            yield return Frames();
            Assert.IsTrue(Shown(view.Interactions));
            Assert.AreEqual("No", view.LeftButton.Q<Label>("Text").text);
            Assert.AreEqual("Yes", view.RightButton.Q<Label>("Text").text);
            Assert.AreEqual(new Rect(96, 0, 32, 32), view.LeftButton.Q("Icon").resolvedStyle.backgroundImage.sprite.rect, "Y glyph");
            Assert.AreEqual(new Rect(64, 0, 32, 32), view.RightButton.Q("Icon").resolvedStyle.backgroundImage.sprite.rect, "X glyph");
            Assert.Less(Right(view.LeftButton), view.RightButton.layout.x);

            // 920 wide, centred, 20 units above the bottom.
            var root = view.Root.layout;
            Assert.AreEqual(920f, view.Bubble.resolvedStyle.width, Tol);
            Assert.AreEqual(root.width / 2f, view.Bubble.layout.center.x, Tol);
            Assert.AreEqual(20f, root.height - (view.Bubble.layout.y + view.Bubble.layout.height), Tol);
            var body = view.Body;
            Assert.AreEqual(42f, body.layout.y, Tol);
            Assert.AreEqual(920f * 0.9f, body.resolvedStyle.width, Tol);
            var footer = view.Root.Q("Footer");
            Assert.AreEqual(40f, footer.resolvedStyle.height, Tol);
            Assert.AreEqual(view.Root.Q("TextWrapper").layout.height, footer.layout.yMax, Tol);
            Assert.AreEqual(body.layout.yMax + 45f, view.Root.Q("TextWrapper").layout.height, Tol);
            Assert.AreEqual(30f, view.Root.Q("Header").resolvedStyle.height, Tol);

            model.Hide();
            yield return Frames();
            Assert.IsFalse(Shown(view.Root));
            model.Hide();
            Assert.IsFalse(Shown(view.Root));
        }

        // ToolTip

        private static TooltipModel Model(Item item, TooltipKind kind, int credits) => TooltipModel.Create(item, kind, credits);

        [UnityTest]
        public IEnumerator ToolTip_TextsAndLayoutFollowGum()
        {
            var item = new Item { Category = "Casing", Function = "Standard", CostValue = 30 };
            var view = Make<ToolTipView>();
            view.Bind(Model(item, TooltipKind.Buyable, 100), Panel());
            yield return Frames();
            Assert.AreEqual("Component Conveyor", view.Title.text);
            Assert.AreEqual("- Standard -", view.Functionality.text);
            Assert.AreEqual("Bullet tracks the nearest enemy.", view.Description.text);
            Assert.AreEqual("Casing", view.CategoryText.text);
            Assert.AreEqual(370f, view.Root.resolvedStyle.width, Tol);
            Assert.AreEqual(266f, view.Root.resolvedStyle.height, Tol);
            var wrapper = view.Root.Q("Wrapper");
            Assert.AreEqual(370f * 0.96f, wrapper.resolvedStyle.width, Tol);
            Assert.AreEqual(266f * 0.9f, wrapper.resolvedStyle.height, Tol);
            Assert.AreEqual(370f * 0.02f, wrapper.layout.x, Tol);
            Assert.AreEqual(266f * 0.05f, wrapper.layout.y, Tol);
            Assert.AreEqual(91f, view.Root.Q("Separator1").layout.y, Tol);
            Assert.AreEqual(339f, view.Root.Q("Separator1").resolvedStyle.width, Tol);
            Assert.AreEqual(204f, view.Root.Q("Separator2").layout.y, Tol);
            Assert.AreEqual(350f, view.Root.Q("Separator2").resolvedStyle.width, Tol);
            Assert.AreEqual(16f, view.Title.layout.x, Tol);
            Assert.AreEqual(165f, view.Title.resolvedStyle.width, Tol);
            Assert.AreEqual(wrapper.layout.height - 176f, view.Title.layout.yMax, Tol);
            Assert.AreEqual(323f, view.Description.resolvedStyle.width, Tol);
            Assert.AreEqual(83f, view.Description.resolvedStyle.height, Tol);
            Assert.AreEqual(20f, view.Description.layout.x, Tol);
            Assert.AreEqual(140f, view.Category.resolvedStyle.width, Tol);
            Assert.AreEqual(40f, view.Category.resolvedStyle.height, Tol);
            Assert.AreEqual(15f, wrapper.layout.width - Right(view.Category), Tol);
            Assert.AreEqual(30f, view.Category.layout.y, Tol);
            Assert.IsTrue(HasImage(view.CategoryIcon));
            var interactions = view.Root.Q("Interactions");
            Assert.AreEqual(wrapper.layout.height, interactions.layout.yMax, Tol);
            Assert.AreEqual(wrapper.layout.height * 0.2f, interactions.layout.height, Tol);
            Assert.AreEqual(0f, view.LeftButton.layout.x, Tol);
            Assert.AreEqual(wrapper.layout.width, Right(view.RightButton), Tol);
            Assert.AreEqual(32f, view.LeftButton.layout.height, Tol);
        }

        [UnityTest]
        public IEnumerator ToolTip_CategoryRowHiddenWithoutCategory()
        {
            var view = Make<ToolTipView>();
            view.Bind(Model(new Item(), TooltipKind.Station, 0), Panel());
            yield return Frames();
            Assert.IsFalse(Shown(view.Category));
            var view2 = Make<ToolTipView>();
            view2.Bind(Model(new Item { Category = "Station" }, TooltipKind.Station, 0), Panel());
            yield return Frames();
            Assert.IsTrue(Shown(view2.Category));
            Assert.AreEqual("Station", view2.CategoryText.text);
            Assert.IsFalse(Shown(view2.CategoryIcon), "no art for this category and no icon rect");
        }

        [UnityTest]
        public IEnumerator ToolTip_ButtonsFollowKind()
        {
            var view = Make<ToolTipView>();
            var item = new Item { CostValue = 25 };
            view.Bind(Model(item, TooltipKind.Buyable, 100), Panel());
            yield return Frames();
            Assert.IsTrue(Shown(view.LeftButton));
            Assert.AreEqual("25", view.LeftButton.Q<Label>("Text").text);
            Assert.IsTrue(view.LeftButton.Q("Icon").ClassListContains("bwi-icon--coin"));
            Assert.IsTrue(HasImage(view.LeftButton.Q("Icon")));
            Assert.AreEqual("Buy", view.RightButton.Q<Label>("Text").text);
            Assert.AreEqual(new Rect(64, 0, 32, 32), view.RightButton.Q("Icon").resolvedStyle.backgroundImage.sprite.rect);

            var lever = Make<ToolTipView>();
            lever.Bind(Model(new Item(), TooltipKind.SpeedLever, 0), Panel());
            yield return Frames();
            Assert.IsFalse(Shown(lever.LeftButton));
            Assert.AreEqual("Speed", lever.RightButton.Q<Label>("Text").text);

            var cannon = Make<ToolTipView>();
            cannon.Bind(Model(new Item(), TooltipKind.CannonSlot, 0), Panel());
            yield return Frames();
            Assert.AreEqual("Sit", cannon.LeftButton.Q<Label>("Text").text);
            Assert.AreEqual(new Rect(96, 0, 32, 32), cannon.LeftButton.Q("Icon").resolvedStyle.backgroundImage.sprite.rect);
            Assert.AreEqual("Fire", cannon.RightButton.Q<Label>("Text").text);

            var none = Make<ToolTipView>();
            none.Bind(Model(new Item(), TooltipKind.None, 0), Panel());
            yield return Frames();
            Assert.IsFalse(Shown(none.LeftButton));
            Assert.IsFalse(Shown(none.RightButton));
        }

        [UnityTest]
        public IEnumerator ToolTip_AffordabilityColourFollowsCredits()
        {
            var model = Model(new Item { CostValue = 50 }, TooltipKind.Buyable, 10);
            var view = Make<ToolTipView>();
            view.Bind(model, Panel());
            yield return Frames();
            var text = view.LeftButton.Q<Label>("Text");
            Color Colour() => text.resolvedStyle.color;
            Assert.AreEqual(new Color32(253, 82, 82, 255), (Color32)Colour());
            model.Refresh(50);
            yield return Frames();
            Assert.AreEqual(new Color32(255, 255, 255, 255), (Color32)Colour());
            model.Refresh(49);
            yield return Frames();
            Assert.AreEqual(new Color32(253, 82, 82, 255), (Color32)Colour());
            Assert.AreEqual(new Color32(255, 255, 255, 255), (Color32)view.RightButton.Q<Label>("Text").resolvedStyle.color);
        }

        [UnityTest]
        public IEnumerator Views_RebuildOnReEnable()
        {
            var model = new DialogBubbleModel();
            var view = Make<DialogBubbleView>();
            view.Bind(model, Panel());
            model.ShowPassive("Hint", "Text");
            yield return Frames();
            view.gameObject.SetActive(false);
            yield return null;
            view.gameObject.SetActive(true);
            yield return Frames();
            Assert.AreEqual("Text", view.Body.text);
            Assert.IsTrue(Shown(view.Root));
            model.Hide();
            Assert.IsFalse(Shown(view.Root));
        }
    }
}
