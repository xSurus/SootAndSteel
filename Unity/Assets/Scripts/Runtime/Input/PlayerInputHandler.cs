using UnityEngine;
using UnityEngine.InputSystem;
using Gamelab.Input;

namespace Gamelab.Input.Runtime
{
    /// <summary>
    /// Unity Input System-backed implementation of IInputActions. PlayerInputManager
    /// (see Gamelab.Players.Runtime.PlayerJoinManager) puts this on the prefab it
    /// instantiates per joined device; SetActions() wires it to that instance's
    /// sibling PlayerInput component.
    ///
    /// Note: the interface's Vector2 is System.Numerics.Vector2 (Gamelab.Core is
    /// engine-agnostic); this file also touches UnityEngine.Vector2 when reading raw
    /// input values, so GetMovement()'s return type is fully qualified and the
    /// Vector2Interop helper (Vector2Raw/ToSystemVector2) converts between the two
    /// without an ambiguous unqualified "Vector2" anywhere in this file.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour, IInputActions
    {
        private PlayerInput playerInput;
        private InputAction move, interact, grab, pickup, start, pause, back;
        private readonly DirectionalInputRepeater directionalRepeater = new DirectionalInputRepeater();

        public void SetActions(PlayerInput input)
        {
            playerInput = input;
            move = input.actions["Player/Move"];
            interact = input.actions["Player/Interact"];
            grab = input.actions["Player/Grab"];
            pickup = input.actions["Player/Pickup"];
            start = input.actions["Player/Start"];
            pause = input.actions["Player/Pause"];
            back = input.actions["Player/Back"];

            // PlayerInput only auto-enables its default map's actions from its own
            // OnEnable, which has already run by the time a caller assigns .actions
            // here (AddComponent<PlayerInput>() then setting .actions afterward, as
            // both this handler's Awake() auto-wiring and the tests do). Enable the
            // actions directly rather than depending on that timing.
            move.Enable();
            interact.Enable();
            grab.Enable();
            pickup.Enable();
            start.Enable();
            pause.Enable();
            back.Enable();
        }

        private void Awake()
        {
            if (playerInput == null && TryGetComponent(out PlayerInput autoWired))
                SetActions(autoWired);
        }

        private void Update()
        {
            if (move == null) return;
            directionalRepeater.Tick(GetMovement(), Time.deltaTime);
        }

        public System.Numerics.Vector2 GetMovement()
        {
            if (move == null) return System.Numerics.Vector2.Zero;
            Vector2Raw raw = move.ReadValue<UnityEngine.Vector2>().ToSystemVector2();
            return raw.Value;
        }

        public bool IsInteractJustPressed() => interact != null && interact.WasPressedThisFrame();
        public bool IsInteractHeld() => interact != null && interact.IsPressed();
        public bool IsInteractJustReleased() => interact != null && interact.WasReleasedThisFrame();
        public bool IsGrabJustPressed() => grab != null && grab.WasPressedThisFrame();
        public bool IsGrabHeld() => grab != null && grab.IsPressed();
        public bool IsGrabJustReleased() => grab != null && grab.WasReleasedThisFrame();
        public bool IsPickupJustPressed() => pickup != null && pickup.WasPressedThisFrame();
        public bool IsPickupHeld() => pickup != null && pickup.IsPressed();
        public bool IsStartJustPressed() => start != null && start.WasPressedThisFrame();
        public bool IsStartHeld() => start != null && start.IsPressed();
        public bool IsPauseJustPressed() => pause != null && pause.WasPressedThisFrame();
        public bool IsBackButtonJustPressed() => back != null && back.WasPressedThisFrame();
        public bool IsBackButtonHeld() => back != null && back.IsPressed();
        public bool IsUpJustPressed() => directionalRepeater.IsUpJustPressed;
        public bool IsDownJustPressed() => directionalRepeater.IsDownJustPressed;
        public bool IsLeftJustPressed() => directionalRepeater.IsLeftJustPressed;
        public bool IsRightJustPressed() => directionalRepeater.IsRightJustPressed;
    }
}
