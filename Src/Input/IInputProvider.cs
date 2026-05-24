using Microsoft.Xna.Framework;

namespace Gamelab.Input;

public interface IInputProvider
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
    void Update(GameTime gameTime);
}