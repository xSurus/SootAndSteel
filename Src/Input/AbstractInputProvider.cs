using Microsoft.Xna.Framework;

namespace Gamelab.Input;

public abstract class AbstractInputProvider : IInputProvider
{
    protected Vector2 currentMovement;
    protected Vector2 previousMovement;

    private float holdTimer = 0f;
    private float repeatTimer = 0f;
    private const float InitialDelay = 0.3f;
    private const float RepeatRate = 0.1f;

    public Vector2 GetMovement() => currentMovement;
    
    public abstract bool IsActionJustPressed();
    public abstract bool IsActionHeld();
    public abstract bool IsStartJustPressed();

    public virtual bool IsDownJustPressed() {
        bool justPressed = currentMovement.Y > 0.5f && previousMovement.Y <= 0.5f;
        return justPressed || CheckDirectionalRepeat(currentMovement.Y > 0.5f);
    }

    public virtual bool IsUpJustPressed() {
        bool justPressed = currentMovement.Y < -0.5f && previousMovement.Y >= -0.5f;
        return justPressed || CheckDirectionalRepeat(currentMovement.Y < -0.5f);
    }

    public virtual bool IsRightJustPressed() {
        bool justPressed = currentMovement.X > 0.5f && previousMovement.X <= 0.5f;
        return justPressed || CheckDirectionalRepeat(currentMovement.X > 0.5f);
    }

    public virtual bool IsLeftJustPressed() {
        bool justPressed = currentMovement.X < -0.5f && previousMovement.X >= -0.5f;
        return justPressed || CheckDirectionalRepeat(currentMovement.X < -0.5f);
    }

    private bool CheckDirectionalRepeat(bool isDirectionHeld)
    {
        if (!isDirectionHeld) return false;
        if (holdTimer >= InitialDelay && repeatTimer >= RepeatRate)
        {
            repeatTimer = 0f;
            return true;
        }
        return false;
    }

    public virtual void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (currentMovement.LengthSquared() > 0.25f) {
            holdTimer += dt;
            repeatTimer += dt;
        }
        else {
            holdTimer = 0f;
            repeatTimer = 0f;
        }
    }
}