using Gamelab.Enemies.Movement;
using UnityEngine;

namespace Gamelab.Enemies
{
    // ponytail: train-frame drift (Src/Enemies/Movement/EnemyMovementController.cs's
    // GetTrainFrameDrift, from Gamelab.Map.Train.State) is dropped — Levels/Map is
    // Wave B (B1)'s subsystem and doesn't exist yet. Add it back once B1 exposes train
    // scroll speed.
    public class EnemyMovementController
    {
        private readonly Rigidbody2D physicsBody;
        public EnemyMovementProfile Profile { get; }

        public EnemyMovementController(Rigidbody2D physicsBody, EnemyMovementProfile profile)
        {
            this.physicsBody = physicsBody;
            Profile = profile;
        }

        public void UpdateTowardPoint(Vector2 targetPosition, float deltaTime)
        {
            Vector2 currentPosition = physicsBody.position;
            Vector2 toTarget = targetPosition - currentPosition;
            float distance = toTarget.magnitude;

            if (distance <= Profile.ArrivalRadius)
            {
                UpdateStop(deltaTime);
                return;
            }

            Vector2 direction = toTarget / distance;
            float desiredSpeed = Profile.MaxForwardSpeed;
            if (distance < Profile.BrakeRadius)
            {
                float t = Mathf.Clamp01(distance / Profile.BrakeRadius);
                desiredSpeed = Mathf.Lerp(0f, Profile.MaxForwardSpeed, t);
            }

            UpdateTowardDirection(direction, desiredSpeed, deltaTime);
        }

        public void UpdateTowardDirection(Vector2 direction, float desiredSpeed, float deltaTime)
        {
            if (direction == Vector2.zero || desiredSpeed <= 0f)
            {
                UpdateStop(deltaTime);
                return;
            }

            direction.Normalize();
            Vector2 desiredVelocity = direction * desiredSpeed;
            Vector2 nextVelocity = MoveVelocityTowards(desiredVelocity, deltaTime);
            physicsBody.linearVelocity = nextVelocity;
        }

        public void UpdateStop(float deltaTime)
        {
            Vector2 nextVelocity = MoveVelocityTowards(Vector2.zero, deltaTime);
            physicsBody.linearVelocity = nextVelocity;
        }

        private Vector2 MoveVelocityTowards(Vector2 desiredVelocity, float deltaTime)
        {
            Vector2 current = physicsBody.linearVelocity;
            float maxDelta = desiredVelocity.sqrMagnitude > current.sqrMagnitude
                ? Profile.Acceleration * deltaTime
                : Profile.Deceleration * deltaTime;

            Vector2 next = Vector2.MoveTowards(current, desiredVelocity, maxDelta);
            return Clamp(next);
        }

        private Vector2 Clamp(Vector2 velocity)
        {
            float x = Mathf.Clamp(velocity.x, -Profile.MaxReverseSpeed, Profile.MaxForwardSpeed);
            float y = Mathf.Clamp(velocity.y, -Profile.MaxLateralSpeed, Profile.MaxLateralSpeed);
            return new Vector2(x, y);
        }
    }
}
