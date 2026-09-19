using System.Numerics;

namespace Gamelab.Input
{
    /// <summary>
    /// Ports the directional-repeat timing from Src/Input/AbstractInputProvider.cs
    /// (IsUpJustPressed/IsDownJustPressed/IsLeftJustPressed/IsRightJustPressed +
    /// CheckDirectionalRepeat) as an engine-free, unit-testable helper. A Runtime
    /// IInputActions implementation calls Tick() once per frame with the movement
    /// vector it reads from Unity's Input System.
    /// </summary>
    public class DirectionalInputRepeater
    {
        private readonly float pressThreshold;
        private readonly float initialRepeatDelaySeconds;
        private readonly float repeatRateSeconds;
        private readonly float movementDeadzoneSquared;

        private Vector2 previousMovement;
        private float holdTimer;
        private float repeatTimer;

        public bool IsUpJustPressed { get; private set; }
        public bool IsDownJustPressed { get; private set; }
        public bool IsLeftJustPressed { get; private set; }
        public bool IsRightJustPressed { get; private set; }

        public DirectionalInputRepeater(
            float pressThreshold = 0.5f,
            float initialRepeatDelaySeconds = 0.3f,
            float repeatRateSeconds = 0.1f,
            float movementDeadzoneSquared = 0.25f)
        {
            this.pressThreshold = pressThreshold;
            this.initialRepeatDelaySeconds = initialRepeatDelaySeconds;
            this.repeatRateSeconds = repeatRateSeconds;
            this.movementDeadzoneSquared = movementDeadzoneSquared;
        }

        public void Tick(Vector2 currentMovement, float dt)
        {
            // Increment timers first for repeat check to see updated values
            if (currentMovement.LengthSquared() > movementDeadzoneSquared)
            {
                holdTimer += dt;
                repeatTimer += dt;
            }
            else
            {
                holdTimer = 0f;
                repeatTimer = 0f;
            }

            IsDownJustPressed = ComputeJustPressed(currentMovement.Y, previousMovement.Y, positive: true);
            IsUpJustPressed = ComputeJustPressed(currentMovement.Y, previousMovement.Y, positive: false);
            IsRightJustPressed = ComputeJustPressed(currentMovement.X, previousMovement.X, positive: true);
            IsLeftJustPressed = ComputeJustPressed(currentMovement.X, previousMovement.X, positive: false);

            previousMovement = currentMovement;
        }

        private bool ComputeJustPressed(float current, float previous, bool positive)
        {
            bool held = positive ? current > pressThreshold : current < -pressThreshold;
            bool wasHeld = positive ? previous > pressThreshold : previous < -pressThreshold;
            bool justPressed = held && !wasHeld;
            // wasHeld short-circuits like the original's ||, so CheckDirectionalRepeat's
            // repeatTimer side effect only runs when the direction was already held.
            bool repeat = wasHeld && CheckDirectionalRepeat(held);
            return justPressed || repeat;
        }

        private bool CheckDirectionalRepeat(bool isDirectionHeld)
        {
            if (!isDirectionHeld) return false;
            if (holdTimer >= initialRepeatDelaySeconds && repeatTimer >= repeatRateSeconds)
            {
                repeatTimer = 0f;
                return true;
            }

            return false;
        }
    }
}
