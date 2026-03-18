using Gamelab.Config;
using Microsoft.Xna.Framework;

namespace Gamelab.Input;

public abstract class AbstractInputProvider : IInputProvider
{
    protected Vector2 currentMovement;
    protected Vector2 previousMovement;
    protected readonly GameplayConfig gameplayConfig;

    private float holdTimer = 0f;
    private float repeatTimer = 0f;

    protected AbstractInputProvider(GameplayConfig gameplayConfig)
    {
        this.gameplayConfig = gameplayConfig;
    }

    public Vector2 GetMovement() => currentMovement;
    
    public abstract bool IsActionJustPressed();
    public abstract bool IsActionHeld();
    public abstract bool IsStartJustPressed();

    public virtual bool IsDownJustPressed() {
        bool justPressed = currentMovement.Y > gameplayConfig.InputDirectionPressThreshold &&
                           previousMovement.Y <= gameplayConfig.InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.Y > gameplayConfig.InputDirectionPressThreshold);
    }

    public virtual bool IsUpJustPressed() {
        bool justPressed = currentMovement.Y < -gameplayConfig.InputDirectionPressThreshold &&
                           previousMovement.Y >= -gameplayConfig.InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.Y < -gameplayConfig.InputDirectionPressThreshold);
    }

    public virtual bool IsRightJustPressed() {
        bool justPressed = currentMovement.X > gameplayConfig.InputDirectionPressThreshold &&
                           previousMovement.X <= gameplayConfig.InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.X > gameplayConfig.InputDirectionPressThreshold);
    }

    public virtual bool IsLeftJustPressed() {
        bool justPressed = currentMovement.X < -gameplayConfig.InputDirectionPressThreshold &&
                           previousMovement.X >= -gameplayConfig.InputDirectionPressThreshold;
        return justPressed || CheckDirectionalRepeat(currentMovement.X < -gameplayConfig.InputDirectionPressThreshold);
    }

    private bool CheckDirectionalRepeat(bool isDirectionHeld)
    {
        if (!isDirectionHeld) return false;
        if (holdTimer >= gameplayConfig.InputInitialRepeatDelaySeconds &&
            repeatTimer >= gameplayConfig.InputRepeatRateSeconds)
        {
            repeatTimer = 0f;
            return true;
        }
        return false;
    }

    public virtual void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (currentMovement.LengthSquared() > gameplayConfig.InputMovementDeadzoneSquared) {
            holdTimer += dt;
            repeatTimer += dt;
        }
        else {
            holdTimer = 0f;
            repeatTimer = 0f;
        }
    }
}