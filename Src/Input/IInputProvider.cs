using Microsoft.Xna.Framework;

namespace Gamelab.Input;

public interface IInputProvider
{
    Vector2 GetMovement();
    bool IsInteractJustPressed();
    bool IsInteractHeld();
    bool IsGrabJustPressed();
    bool IsGrabHeld();
    bool IsStartJustPressed();
    bool IsUpJustPressed();
    bool IsDownJustPressed();
    bool IsLeftJustPressed();
    bool IsRightJustPressed();
    void Update(GameTime gameTime);
}