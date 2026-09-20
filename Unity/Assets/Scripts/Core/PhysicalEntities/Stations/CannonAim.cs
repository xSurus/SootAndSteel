using System;
using System.Numerics;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src CannonStation.UpdateAim (the angle maths only).
    public static class CannonAim
    {
        // MonoGame MathHelper.WrapAngle: result in (-pi, pi].
        public static float WrapAngle(float angle)
        {
            // Float constants: (float)Math.PI rounds up, and callers pass float pi as the upper edge.
            const float pi = (float)Math.PI;
            const float twoPi = pi * 2f;
            if (angle > -pi && angle <= pi) return angle;
            angle %= twoPi;
            if (angle <= -pi) return angle + twoPi;
            if (angle > pi) return angle - twoPi;
            return angle;
        }

        public static float Step(float currentAngle, Vector2 input, float dt, float rotationSpeed, float deadzoneSquared)
        {
            if (input.LengthSquared() <= deadzoneSquared) return currentAngle;

            float targetAngle = (float)Math.Atan2(input.Y, input.X);
            float diff = WrapAngle(targetAngle - currentAngle);
            float maxStep = rotationSpeed * dt;
            float step = Math.Max(-maxStep, Math.Min(maxStep, diff));
            return currentAngle + step;
        }
    }
}
