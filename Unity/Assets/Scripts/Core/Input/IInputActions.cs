using System.Numerics;

namespace Gamelab.Input
{
    /// <summary>
    /// Engine-free input query surface. Mirrors Src/Input/IInputProvider.cs from the
    /// MonoGame source 1:1 so Player.cs's eventual port can consume it unchanged.
    /// Gamelab.Runtime supplies the Unity Input System-backed implementation
    /// (see Gamelab.Input.Runtime.PlayerInputHandler).
    /// </summary>
    public interface IInputActions
    {
        Vector2 GetMovement();
        bool IsInteractJustPressed();
        bool IsInteractHeld();
        bool IsInteractJustReleased();
        bool IsGrabJustPressed();
        bool IsGrabHeld();
        bool IsGrabJustReleased();
        bool IsPickupJustPressed();
        bool IsPickupHeld();
        bool IsStartJustPressed();
        bool IsStartHeld();
        bool IsPauseJustPressed();
        bool IsBackButtonJustPressed();
        bool IsBackButtonHeld();
        bool IsUpJustPressed();
        bool IsDownJustPressed();
        bool IsLeftJustPressed();
        bool IsRightJustPressed();
    }
}
