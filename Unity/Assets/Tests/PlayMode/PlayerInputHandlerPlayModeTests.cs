using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Gamelab.Input.Runtime;

namespace Gamelab.Tests.Input
{
    public class PlayerInputHandlerPlayModeTests : InputTestFixture
    {
        private Keyboard keyboard;
        private GameObject go;
        private PlayerInput playerInput;
        private PlayerInputHandler handler;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();

            var actions = Resources.Load<InputActionAsset>("GameplayControls");
            Assert.IsNotNull(actions, "Expected Assets/Resources/GameplayControls.inputactions (a .inputactions asset placed under a Resources folder so Resources.Load can find it in tests and at runtime)");

            go = new GameObject("TestPlayer");
            playerInput = go.AddComponent<PlayerInput>();
            playerInput.actions = actions;
            playerInput.defaultActionMap = "Player";
            handler = go.AddComponent<PlayerInputHandler>();
            handler.SetActions(playerInput);
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(go);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator PressingE_ReportsInteractJustPressed()
        {
            Press(keyboard.eKey);
            yield return null;

            Assert.IsTrue(handler.IsInteractJustPressed());
        }

        [UnityTest]
        public IEnumerator HoldingWASD_ReportsMovementVector()
        {
            Press(keyboard.dKey);
            yield return null;

            System.Numerics.Vector2 movement = handler.GetMovement();
            Assert.Greater(movement.X, 0f);
            Assert.AreEqual(0f, movement.Y, 0.001f);
        }

        [UnityTest]
        public IEnumerator HoldingW_ReportsNegativeY_MonoGameConvention()
        {
            Press(keyboard.wKey);
            yield return null;
            Assert.Less(handler.GetMovement().Y, 0f);
        }
    }
}
