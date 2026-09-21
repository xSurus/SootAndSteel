using System.Numerics;
using Gamelab.Input;

namespace Gamelab.Tests.EditMode.UI
{
    /// <summary>Menu-relevant flags are set by tests and stay true until Clear.</summary>
    public class FakeInputActions : IInputActions
    {
        public bool Up, Down, Left, Right, Pickup, Pause;

        public void Clear() { Up = Down = Left = Right = Pickup = Pause = false; }

        public bool IsUpJustPressed() => Up;
        public bool IsDownJustPressed() => Down;
        public bool IsLeftJustPressed() => Left;
        public bool IsRightJustPressed() => Right;
        public bool IsPickupJustPressed() => Pickup;
        public bool IsPauseJustPressed() => Pause;

        public Vector2 GetMovement() => Vector2.Zero;
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
}
