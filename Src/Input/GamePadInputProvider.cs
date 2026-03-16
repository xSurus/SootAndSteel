using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gamelab.Input;

public class GamePadInputProvider(int controllerIndex) : IInputProvider
{
    private GamePadState currentGamePadState;
    private GamePadState previousGamePadState;
    public int ControllerIndex { get; } = controllerIndex;

    public Vector2 GetMovement() {
        Vector2 move = currentGamePadState.ThumbSticks.Left;
        move.Y = -move.Y;

        if (move == Vector2.Zero)
        {
            if (currentGamePadState.DPad.Up == ButtonState.Pressed) move.Y -= 1;
            if (currentGamePadState.DPad.Down == ButtonState.Pressed) move.Y += 1;
            if (currentGamePadState.DPad.Left == ButtonState.Pressed) move.X -= 1;
            if (currentGamePadState.DPad.Right == ButtonState.Pressed) move.X += 1;
        }

        if (move.LengthSquared() > 1f) move.Normalize();
        return move;
    }

    public bool IsActionJustPressed() => 
        currentGamePadState.Buttons.A == ButtonState.Pressed && previousGamePadState.Buttons.A == ButtonState.Released;

    public bool IsActionHeld() => currentGamePadState.Buttons.A == ButtonState.Pressed;
    
    public bool IsStartJustPressed() => currentGamePadState.Buttons.Start == ButtonState.Pressed && previousGamePadState.Buttons.Start == ButtonState.Released;

    public void Update() {
        previousGamePadState = currentGamePadState;
        currentGamePadState = GamePad.GetState(ControllerIndex);
    }
}