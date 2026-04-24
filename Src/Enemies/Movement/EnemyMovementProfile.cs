namespace Gamelab.Enemies.Movement;

public readonly record struct EnemyMovementProfile(
    float MaxForwardSpeed,
    float MaxReverseSpeed,
    float MaxLateralSpeed,
    float Acceleration,
    float Deceleration,
    float ArrivalRadius,
    float BrakeRadius)
{
    public static EnemyMovementProfile CreateDefault(float maxForwardSpeed)
    {
        return new EnemyMovementProfile(
            MaxForwardSpeed: maxForwardSpeed,
            MaxReverseSpeed: maxForwardSpeed * 0.35f,
            MaxLateralSpeed: maxForwardSpeed * 0.8f,
            Acceleration: maxForwardSpeed * 2.4f,
            Deceleration: maxForwardSpeed * 3.2f,
            ArrivalRadius: 18f,
            BrakeRadius: 120f);
    }
}
