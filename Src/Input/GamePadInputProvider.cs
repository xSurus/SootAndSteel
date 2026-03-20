using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gamelab.Input;

public class GamePadInputProvider(int controllerIndex) : AbstractInputProvider()
{
    private GamePadState currentGamePadState;
    private GamePadState previousGamePadState;
    public int ControllerIndex { get; } = controllerIndex;

    public override void Update(GameTime gameTime)
    {
        previousGamePadState = currentGamePadState;
        previousMovement = currentMovement;

        currentGamePadState = GamePad.GetState(ControllerIndex);
        currentMovement = CalculateMovement();

        base.Update(gameTime);
    }

    private Vector2 CalculateMovement()
    {
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

    public override bool IsInteractJustPressed() => currentGamePadState.Buttons.X == ButtonState.Pressed &&
                                                    previousGamePadState.Buttons.X == ButtonState.Released;

    public override bool IsInteractHeld() => currentGamePadState.Buttons.X == ButtonState.Pressed;

    public override bool IsGrabJustPressed() => currentGamePadState.Buttons.A == ButtonState.Pressed &&
                                                previousGamePadState.Buttons.A == ButtonState.Released;

    public override bool IsGrabHeld() => currentGamePadState.Buttons.A == ButtonState.Pressed;

    public override bool IsStartJustPressed() => currentGamePadState.Buttons.Start == ButtonState.Pressed &&
                                                 previousGamePadState.Buttons.Start == ButtonState.Released;
}