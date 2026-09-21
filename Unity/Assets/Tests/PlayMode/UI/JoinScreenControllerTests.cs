using System;
using System.Collections;
using Gamelab.Players.Runtime;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class JoinScreenControllerTests : InputTestFixture
    {
        private GameObject managerGo;
        private JoinFlowController flow;
        private JoinScreenController controller;
        private int advanced;
        private GameObject prefabGo;

        public override void Setup()
        {
            base.Setup();
            var actions = Resources.Load<InputActionAsset>("GameplayControls");
            prefabGo = new GameObject("PlayerPrefab");
            prefabGo.SetActive(false);
            prefabGo.AddComponent<PlayerInput>();
            prefabGo.AddComponent<Gamelab.Input.Runtime.PlayerInputHandler>();

            managerGo = new GameObject("JoinManager");
            var pim = managerGo.AddComponent<PlayerInputManager>();
            pim.playerPrefab = prefabGo;
            pim.EnableJoining();
            var joinManager = managerGo.AddComponent<PlayerJoinManager>();
            joinManager.Configure(pim, actions);
            flow = managerGo.AddComponent<JoinFlowController>();
            controller = managerGo.AddComponent<JoinScreenController>();
            advanced = 0;
            controller.Bind(flow, () => advanced++);
        }

        public override void TearDown()
        {
            managerGo.GetComponent<PlayerJoinManager>().ResetJoins();
            UnityEngine.Object.DestroyImmediate(managerGo);
            UnityEngine.Object.DestroyImmediate(prefabGo);
            base.TearDown();
        }

        private static IEnumerator Frames(int n = 3) { for (int i = 0; i < n; i++) yield return null; }
        private static UnityEngine.Object Image(VisualElement e) =>
            (UnityEngine.Object)e.resolvedStyle.backgroundImage.sprite ?? e.resolvedStyle.backgroundImage.texture;

        private IEnumerator Join(Gamepad pad)
        {
            Press(pad.buttonSouth);
            yield return null;
            Release(pad.buttonSouth);
            yield return Frames();
        }

        [UnityTest]
        public IEnumerator Start_ShowsSilhouettesAndJoinForAllSlots()
        {
            yield return Frames();
            var view = controller.View;
            Assert.AreEqual("Enter the train", view.Title.text);
            var silhouette = Resources.Load<Sprite>("UI/Art/Silhouette");
            for (int i = 0; i < 4; i++)
            {
                Assert.AreSame(silhouette, Image(view.Figure(i)));
                Assert.AreEqual("Join", view.JoinText(i).text);
            }
        }

        [UnityTest]
        public IEnumerator Joins_UpdateSlotsAndTitle()
        {
            var pads = new[] { InputSystem.AddDevice<Gamepad>(), InputSystem.AddDevice<Gamepad>(),
                InputSystem.AddDevice<Gamepad>(), InputSystem.AddDevice<Gamepad>() };
            var view = controller.View;
            yield return Join(pads[0]);
            Assert.AreEqual("Press start to advance", view.Title.text);
            Assert.AreSame(Resources.Load<Sprite>("UI/Art/IdleA0"), Image(view.Figure(0)));
            Assert.AreEqual("Joined", view.JoinText(0).text);
            Assert.AreEqual("Join", view.JoinText(1).text);
            Assert.AreSame(Resources.Load<Sprite>("UI/Art/Silhouette"), Image(view.Figure(1)));

            for (int i = 1; i < 4; i++) yield return Join(pads[i]);
            string[] heads = { "IdleA0", "IdleA3", "IdleA1", "IdleA2" };
            for (int i = 0; i < 4; i++)
            {
                Assert.AreSame(Resources.Load<Sprite>("UI/Art/" + heads[i]), Image(view.Figure(i)));
                Assert.AreEqual("Joined", view.JoinText(i).text);
            }
        }

        [UnityTest]
        public IEnumerator StartByJoinedPlayer_AdvancesOnce()
        {
            var pad = InputSystem.AddDevice<Gamepad>();
            yield return Join(pad);
            Assert.AreEqual(0, advanced);
            Press(pad.startButton);
            yield return null;
            Release(pad.startButton);
            yield return Frames();
            Assert.AreEqual(1, advanced);
        }

        [Test]
        public void SecondBind_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => controller.Bind(flow, () => { }));
        }
    }
}
