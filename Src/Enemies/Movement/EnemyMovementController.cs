using Gamelab.Map.Train.State;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Enemies.Movement;

public class EnemyMovementController(Body physicsBody, EnemyMovementProfile profile)
{
    public EnemyMovementProfile Profile { get; } = profile;
    protected readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    public void UpdateTowardPoint(Vector2 targetPosition, float deltaTime, bool includeTrainDrift = true)
    {
        Vector2 currentPosition = physicsBody.Position.ToPixels();
        Vector2 toTarget = targetPosition - currentPosition;
        float distance = toTarget.Length();

        if (distance <= Profile.ArrivalRadius)
        {
            UpdateStop(deltaTime, includeTrainDrift);
            return;
        }

        Vector2 direction = toTarget / distance;
        float desiredSpeed = Profile.MaxForwardSpeed;
        if (distance < Profile.BrakeRadius)
        {
            float t = MathHelper.Clamp(distance / Profile.BrakeRadius, 0f, 1f);
            desiredSpeed = MathHelper.Lerp(0f, Profile.MaxForwardSpeed, t);
        }

        UpdateTowardDirection(direction, desiredSpeed, deltaTime, includeTrainDrift);
    }

    public void UpdateTowardDirection(Vector2 direction, float desiredSpeed, float deltaTime,
        bool includeTrainDrift = true)
    {
        if (direction == Vector2.Zero || desiredSpeed <= 0f)
        {
            UpdateStop(deltaTime, includeTrainDrift);
            return;
        }

        direction.Normalize();
        Vector2 desiredVelocity = direction * desiredSpeed;
        Vector2 nextSelfVelocity = MoveSelfVelocityTowards(desiredVelocity, deltaTime, includeTrainDrift);
        Apply(nextSelfVelocity, includeTrainDrift);
    }

    public void UpdateHoldPosition(Vector2 anchor, float deltaTime, bool includeTrainDrift = true)
    {
        UpdateTowardPoint(anchor, deltaTime, includeTrainDrift);
    }

    public void UpdateStop(float deltaTime, bool includeTrainDrift = true)
    {
        Vector2 nextSelfVelocity = MoveSelfVelocityTowards(Vector2.Zero, deltaTime, includeTrainDrift);
        Apply(nextSelfVelocity, includeTrainDrift);
    }

    private Vector2 MoveSelfVelocityTowards(Vector2 desiredVelocity, float deltaTime, bool includeTrainDrift)
    {
        Vector2 currentSelfVelocity = GetCurrentSelfVelocity(includeTrainDrift);
        float maxDelta = desiredVelocity.LengthSquared() > currentSelfVelocity.LengthSquared()
            ? Profile.Acceleration * deltaTime
            : Profile.Deceleration * deltaTime;

        Vector2 nextSelfVelocity = MoveTowards(currentSelfVelocity, desiredVelocity, maxDelta);
        return Clamp(nextSelfVelocity);
    }

    private Vector2 GetCurrentSelfVelocity(bool includeTrainDrift)
    {
        Vector2 totalVelocity = physicsBody.LinearVelocity.ToPixels();
        return includeTrainDrift ? totalVelocity - GetTrainFrameDrift() : totalVelocity;
    }

    private void Apply(Vector2 selfVelocity, bool includeTrainDrift)
    {
        Vector2 totalVelocity = selfVelocity;
        if (includeTrainDrift)
        {
            totalVelocity += GetTrainFrameDrift();
        }

        physicsBody.LinearVelocity = totalVelocity.ToMeters();
    }

    private Vector2 GetTrainFrameDrift()
    {
        return new Vector2(-gameplayContext.State.actualSpeed, 0f);
    }

    private Vector2 Clamp(Vector2 velocity)
    {
        float x = MathHelper.Clamp(velocity.X, -Profile.MaxReverseSpeed, Profile.MaxForwardSpeed);
        float y = MathHelper.Clamp(velocity.Y, -Profile.MaxLateralSpeed, Profile.MaxLateralSpeed);
        return new Vector2(x, y);
    }

    private static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDelta)
    {
        Vector2 delta = target - current;
        float distance = delta.Length();
        if (distance <= maxDelta || distance <= 0.0001f)
        {
            return target;
        }

        return current + delta / distance * maxDelta;
    }
}
