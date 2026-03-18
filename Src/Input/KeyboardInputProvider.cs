using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gamelab.Input;

public class KeyboardInputProvider(Keys up, Keys down, Keys left, Keys right, Keys action) : AbstractInputProvider()
{
    private KeyboardState currentKeyboardState;
    private KeyboardState previousKeyboardState;

    public override void Update(GameTime gameTime)
    {
        previousKeyboardState = currentKeyboardState;
        previousMovement = currentMovement;

        currentKeyboardState = Keyboard.GetState();
        currentMovement = CalculateMovement();

        base.Update(gameTime);
    }

    private Vector2 CalculateMovement()
    {
        Vector2 move = Vector2.Zero;

        if (currentKeyboardState.IsKeyDown(up)) move.Y -= 1;
        if (currentKeyboardState.IsKeyDown(down)) move.Y += 1;
        if (currentKeyboardState.IsKeyDown(left)) move.X -= 1;
        if (currentKeyboardState.IsKeyDown(right)) move.X += 1;

        if (move != Vector2.Zero) move.Normalize();
        return move;
    }

    public override bool IsActionJustPressed() =>
        currentKeyboardState.IsKeyDown(action) && previousKeyboardState.IsKeyUp(action);

    public override bool IsActionHeld() => currentKeyboardState.IsKeyDown(action);

    public override bool IsStartJustPressed() =>
        currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter);
}