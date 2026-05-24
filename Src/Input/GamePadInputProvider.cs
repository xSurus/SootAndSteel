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
    
    public override bool IsInteractJustReleased() => currentGamePadState.Buttons.X == ButtonState.Released &&
                                                     previousGamePadState.Buttons.X == ButtonState.Pressed;
    
    public override bool IsGrabJustPressed() => currentGamePadState.Buttons.Y == ButtonState.Pressed &&
                                                previousGamePadState.Buttons.Y == ButtonState.Released;

    public override bool IsGrabHeld() => currentGamePadState.Buttons.Y == ButtonState.Pressed;
    
    public override bool IsGrabJustReleased() => currentGamePadState.Buttons.Y == ButtonState.Released &&
                                                 previousGamePadState.Buttons.Y == ButtonState.Pressed;

    public override bool IsPickupJustPressed() => currentGamePadState.Buttons.A == ButtonState.Pressed &&
                                                  previousGamePadState.Buttons.A == ButtonState.Released;

    public override bool IsPickupHeld() => currentGamePadState.Buttons.A == ButtonState.Pressed;

    public override bool IsStartJustPressed() => currentGamePadState.Buttons.Start == ButtonState.Pressed &&
                                                 previousGamePadState.Buttons.Start == ButtonState.Released;

    public override bool IsStartHeld() => currentGamePadState.Buttons.Start == ButtonState.Pressed;

    // same as start for the controller
    public override bool IsPauseJustPressed() => currentGamePadState.Buttons.Start == ButtonState.Pressed &&
                                                 previousGamePadState.Buttons.Start == ButtonState.Released;

    public override bool IsBackButtonJustPressed() => currentGamePadState.Buttons.Back == ButtonState.Pressed &&
                                                      previousGamePadState.Buttons.Back == ButtonState.Released;
    
    public override bool IsBackButtonHeld() => currentGamePadState.Buttons.Back == ButtonState.Pressed;
}