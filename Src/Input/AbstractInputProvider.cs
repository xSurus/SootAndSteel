using Microsoft.Xna.Framework;

namespace Gamelab.Input;

public abstract class AbstractInputProvider : IInputProvider
{
    protected Vector2 currentMovement;
    protected Vector2 previousMovement;

    private float InputDirectionPressThreshold => GamelabGame.Instance.GameplayConfig.InputDirectionPressThreshold;
    private float InputInitialRepeatDelaySeconds => GamelabGame.Instance.GameplayConfig.InputInitialRepeatDelaySeconds;
    private float InputRepeatRateSeconds => GamelabGame.Instance.GameplayConfig.InputRepeatRateSeconds;
    private float InputMovementDeadzoneSquared => GamelabGame.Instance.GameplayConfig.InputMovementDeadzoneSquared;

    private float holdTimer;
    private float repeatTimer;

    public Vector2 GetMovement() => currentMovement;

    public abstract bool IsInteractJustPressed();
    public abstract bool IsInteractHeld();
    public abstract bool IsGrabJustPressed();
    public abstract bool IsGrabHeld();
    public abstract bool IsPickupJustPressed();
    public abstract bool IsPickupHeld();
    public abstract bool IsStartJustPressed();
    public abstract bool IsPauseJustPressed();

    public virtual bool IsDownJustPressed()
    {
        bool justPressed = currentMovement.Y > InputDirectionPressThreshold &&
                           previousMovement.Y <= InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.Y > InputDirectionPressThreshold);
    }

    public virtual bool IsUpJustPressed()
    {
        bool justPressed = currentMovement.Y < -InputDirectionPressThreshold &&
                           previousMovement.Y >= -InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.Y < -InputDirectionPressThreshold);
    }

    public virtual bool IsRightJustPressed()
    {
        bool justPressed = currentMovement.X > InputDirectionPressThreshold &&
                           previousMovement.X <= InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.X > InputDirectionPressThreshold);
    }

    public virtual bool IsLeftJustPressed()
    {
        bool justPressed = currentMovement.X < -InputDirectionPressThreshold &&
                           previousMovement.X >= -InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.X < -InputDirectionPressThreshold);
    }

    private bool CheckDirectionalRepeat(bool isDirectionHeld)
    {
        if (!isDirectionHeld) return false;
        if (holdTimer >= InputInitialRepeatDelaySeconds &&
            repeatTimer >= InputRepeatRateSeconds)
        {
            repeatTimer = 0f;
            return true;
        }

        return false;
    }

    public virtual void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (currentMovement.LengthSquared() > InputMovementDeadzoneSquared)
        {
            holdTimer += dt;
            repeatTimer += dt;
        }
        else
        {
            holdTimer = 0f;
            repeatTimer = 0f;
        }
    }
}