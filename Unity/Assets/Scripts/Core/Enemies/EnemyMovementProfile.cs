namespace Gamelab.Enemies.Movement
{
    public readonly struct EnemyMovementProfile
    {
        public float MaxForwardSpeed { get; }
        public float MaxReverseSpeed { get; }
        public float MaxLateralSpeed { get; }
        public float Acceleration { get; }
        public float Deceleration { get; }
        public float ArrivalRadius { get; }
        public float BrakeRadius { get; }

        public EnemyMovementProfile(
            float maxForwardSpeed,
            float maxReverseSpeed,
            float maxLateralSpeed,
            float acceleration,
            float deceleration,
            float arrivalRadius,
            float brakeRadius)
        {
            MaxForwardSpeed = maxForwardSpeed;
            MaxReverseSpeed = maxReverseSpeed;
            MaxLateralSpeed = maxLateralSpeed;
            Acceleration = acceleration;
            Deceleration = deceleration;
            ArrivalRadius = arrivalRadius;
            BrakeRadius = brakeRadius;
        }

        /// <summary>All seven fields are pixel-based; returns the same profile in meters.</summary>
        public EnemyMovementProfile ToMeters()
        {
            return new EnemyMovementProfile(
                WorldUnits.ToMeters(MaxForwardSpeed),
                WorldUnits.ToMeters(MaxReverseSpeed),
                WorldUnits.ToMeters(MaxLateralSpeed),
                WorldUnits.ToMeters(Acceleration),
                WorldUnits.ToMeters(Deceleration),
                WorldUnits.ToMeters(ArrivalRadius),
                WorldUnits.ToMeters(BrakeRadius));
        }

        public static EnemyMovementProfile CreateDefault(float maxForwardSpeed)
        {
            return new EnemyMovementProfile(
                maxForwardSpeed: maxForwardSpeed,
                maxReverseSpeed: maxForwardSpeed * 0.35f,
                maxLateralSpeed: maxForwardSpeed * 0.8f,
                acceleration: maxForwardSpeed * 2.4f,
                deceleration: maxForwardSpeed * 3.2f,
                arrivalRadius: 18f,
                brakeRadius: 120f);
        }
    }
}
