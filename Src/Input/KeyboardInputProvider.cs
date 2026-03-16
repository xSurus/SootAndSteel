using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gamelab.Input;

public class KeyboardInputProvider(Keys up, Keys down, Keys left, Keys right, Keys action) : IInputProvider
{
    private KeyboardState currentKeyboardState;
    private KeyboardState previousKeyboardState;

    public Vector2 GetMovement() {
        Vector2 move = Vector2.Zero;
        
        if (currentKeyboardState.IsKeyDown(up)) move.Y -= 1;
        if (currentKeyboardState.IsKeyDown(down)) move.Y += 1;
        if (currentKeyboardState.IsKeyDown(left)) move.X -= 1;
        if (currentKeyboardState.IsKeyDown(right)) move.X += 1;

        if (move != Vector2.Zero) move.Normalize();
        return move;
    }

    public bool IsActionJustPressed() => currentKeyboardState.IsKeyDown(action) && previousKeyboardState.IsKeyUp(action);
    
    public bool IsActionHeld() => currentKeyboardState.IsKeyDown(action);

    public bool IsStartJustPressed() => currentKeyboardState.IsKeyDown(Keys.Enter) &&  previousKeyboardState.IsKeyUp(Keys.Enter);

    public void Update() {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();
    }
}