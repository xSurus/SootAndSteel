using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Gamelab.Players.Runtime;

namespace Gamelab.Tests.Players
{
    public class PlayerJoinManagerPlayModeTests : InputTestFixture
    {
        private GameObject managerGo;
        private PlayerJoinManager joinManager;
        private PlayerInputManager pim;

        public override void Setup()
        {
            base.Setup();

            var actions = Resources.Load<InputActionAsset>("GameplayControls");
            var prefabGo = new GameObject("PlayerPrefab");
            // Deactivate before adding components: AddComponent() on an active
            // GameObject fires Awake() synchronously, and PlayerInputHandler.Awake()
            // would auto-wire from PlayerInput.actions before Configure() below has a
            // chance to assign it, throwing a NullReferenceException. Deactivating
            // first defers Awake() until PlayerInputManager clones+activates this
            // prefab per join, by which point Configure() has assigned the actions
            // asset to the prefab's PlayerInput (and the clone inherits it).
            prefabGo.SetActive(false);
            prefabGo.AddComponent<PlayerInput>();
            prefabGo.AddComponent<Gamelab.Input.Runtime.PlayerInputHandler>();

            managerGo = new GameObject("JoinManager");
            pim = managerGo.AddComponent<PlayerInputManager>();
            pim.playerPrefab = prefabGo;
            pim.EnableJoining();
            joinManager = managerGo.AddComponent<PlayerJoinManager>();
            joinManager.Configure(pim, actions);
        }

        public override void TearDown()
        {
            joinManager.ResetJoins();
            Object.DestroyImmediate(managerGo);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator TwoSimulatedGamepads_BothJoin_WithDistinctSplitScreenIndices()
        {
            var pad1 = InputSystem.AddDevice<Gamepad>();
            var pad2 = InputSystem.AddDevice<Gamepad>();

            Press(pad1.buttonSouth);
            yield return null;
            Release(pad1.buttonSouth);
            yield return null;

            Press(pad2.buttonSouth);
            yield return null;
            Release(pad2.buttonSouth);
            yield return null;

            Assert.AreEqual(2, joinManager.Roster.Slots.Count, "both simulated controllers should have joined");
            int split0 = joinManager.GetSplitScreenIndex(0);
            int split1 = joinManager.GetSplitScreenIndex(1);
            Assert.AreNotEqual(split0, split1, "each joined player must get a distinct split-screen index");
        }

        [UnityTest]
        public IEnumerator KeyboardSpace_Joins_AlongsideGamepad()
        {
            var pad1 = InputSystem.AddDevice<Gamepad>();
            var keyboard = InputSystem.AddDevice<Keyboard>();

            Press(pad1.buttonSouth);
            yield return null;
            Release(pad1.buttonSouth);
            yield return null;

            Press(keyboard.spaceKey);
            yield return null;
            Release(keyboard.spaceKey);
            yield return null;

            Assert.AreEqual(2, joinManager.Roster.Slots.Count, "gamepad and keyboard should both be able to join");
        }

        [UnityTest]
        public IEnumerator TwoPlayers_HaveIndependentActionInstances_AndInputs()
        {
            var pad1 = InputSystem.AddDevice<Gamepad>();
            var pad2 = InputSystem.AddDevice<Gamepad>();

            Press(pad1.buttonSouth);
            yield return null;
            Release(pad1.buttonSouth);
            yield return null;
            Press(pad2.buttonSouth);
            yield return null;
            Release(pad2.buttonSouth);
            yield return null;

            var a = ((MonoBehaviour)joinManager.Roster.Slots[0].Input).GetComponent<PlayerInput>();
            var b = ((MonoBehaviour)joinManager.Roster.Slots[1].Input).GetComponent<PlayerInput>();
            Assert.AreNotSame(a.actions, b.actions,
                "each player needs its own InputActionAsset instance, or enabling one player's actions cross-enables the other's");

            // Only pad1 presses Start: exactly one player's handler may see it.
            Press(pad1.startButton);
            yield return null;
            int seen = 0;
            foreach (var slot in joinManager.Roster.Slots)
                if (slot.Input.IsStartJustPressed()) seen++;
            Assert.AreEqual(1, seen, "a press on one pad must reach only that pad's player");
        }
    }
}
