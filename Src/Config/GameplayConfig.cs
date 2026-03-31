namespace Gamelab.Config;

public class GameplayConfig
{
    public float PixelsPerMeter { get; set; } = 100f;
    public float FixedTimeStep { get; set; } = 1f / 60f;
    public float MaxAccumulatedDeltaSeconds { get; set; } = 0.25f;
    public int TrainTileSize { get; set; } = 80;
    public int TrainWidth { get; set; } = 8;
    public int TrainHeight { get; set; } = 6;
    public float TrainSpeedStopped { get; set; } = 0f;
    public float TrainSpeedDefault { get; set; } = 150f;
    public float TrainSpeedDouble { get; set; } = 300f;
    public float TrainSpeedQuadruple { get; set; } = 600f;
    public float TrainBurnMultiplierStopped { get; set; } = 1f;
    public float TrainBurnMultiplierDefault { get; set; } = 1f;
    public float TrainBurnMultiplierDouble { get; set; } = 1f;
    public float TrainBurnMultiplierQuadruple { get; set; } = 4f;
    public float TrainAccelerationRate { get; set; } = 50f;
    public int TrainInitialCoalAmount { get; set; } = 50;
    public float SpawnOffsetPixels { get; set; } = 100f;
    public float PlayerRadiusPixels { get; set; } = 24f;
    public float PlayerDensity { get; set; } = 3f;
    public float PlayerLinearDamping { get; set; } = 20f;
    public float PlayerMaxVelocity { get; set; } = 5f;
    public float PlayerVelocityLerpFactor { get; set; } = 0.8f;
    public float PlayerInteractDistancePixels { get; set; } = 60f;
    public float PlayerHeldItemOffsetRadiusMultiplier { get; set; } = 1.5f;
    public float PlayerHeldItemSizeRadiusMultiplier { get; set; } = 0.8f;
    public float PlayerForceMultiplier { get; set; } = 50f;
    public float InputInitialRepeatDelaySeconds { get; set; } = 0.3f;
    public float InputRepeatRateSeconds { get; set; } = 0.1f;
    public float InputDirectionPressThreshold { get; set; } = 0.5f;
    public float InputMovementDeadzoneSquared { get; set; } = 0.25f;
    public float MenuVolumeStep { get; set; } = 0.05f;
    public int WorldScrollerPatternWidthPixels { get; set; } = 480;
    public int WorldScrollerStripeSpacingPixels { get; set; } = 80;
    public int WorldScrollerStripeThicknessPixels { get; set; } = 4;
    public byte WorldScrollerBaseColorR { get; set; } = 40;
    public byte WorldScrollerBaseColorG { get; set; } = 40;
    public byte WorldScrollerBaseColorB { get; set; } = 45;
    public byte WorldScrollerStripeColorR { get; set; } = 50;
    public byte WorldScrollerStripeColorG { get; set; } = 50;
    public byte WorldScrollerStripeColorB { get; set; } = 55;
    public float CoalOvenMaxFuel { get; set; } = 30f;
    public float CoalOvenBurnRate { get; set; } = 1f;
    public float CoalOvenRefuelAmount { get; set; } = 10f;
    public float CoalOvenLowFuelThreshold { get; set; } = 0.25f;
    public float WallMaxHealth { get; set; } = 100f;
    public float WallHealthRestoredPerSecond { get; set; } = 40f;
    public float GrabbableLinearDamping { get; set; } = 100f;
    public float GrabbableRotationalResistance { get; set; } = 40f;
    
    // Enemy configuration (general)
    public float EnemyHealth { get; set; } = 100f;
    public float EnemySpeed { get; set; } = 0f;
    public float EnemySize { get; set; } = 72f;
    public float EnemySpawnIntervalBase { get; set; } = 12f;
    public float EnemySpawnIntervalVariance { get; set; } = 5f;
    public float EnemySpawnOffsetX { get; set; } = 100f;
    public float EnemySpawnMarginY { get; set; } = 100f;
    public float ShooterSpawnChance { get; set; } = 0.9f;
    
    // Shooter enemy configuration
    public float ShooterMaxSpeed { get; set; } = 250f;
    public float ShooterPreferredDistance { get; set; } = 150f;
    public float EnemyShootCooldown { get; set; } = 2f;
    public float EnemyShootSpread { get; set; } = 0.2f;
    
    // Thief enemy configuration
    public float ThiefApproachSpeed { get; set; } = 200f;
    public float ThiefFleeSpeed { get; set; } = 350f;
    public float ThiefStealDuration { get; set; } = 1.5f;
    public int ThiefCoalAmount { get; set; } = 5;
    
    // Projectile configuration
    public float ProjectileSpeed { get; set; } = 300f;
    public float ProjectileDamage { get; set; } = 10f;
    public float ProjectileLifetime { get; set; } = 5f;
    public float ProjectileSize { get; set; } = 8f;
    
    // Train temperature configuration
    public float TrainMaxTemperature { get; set; } = 100f;
    public float TrainTemperatureDecreasePerSecondPerBreachedWall { get; set; } = 3f;
    public float TrainTemperatureIncreasePerSecond { get; set; } = 3f;
    
    // Screen shake configuration
    public float ScreenShakeIntensity { get; set; } = 8f;
    public float ScreenShakeDuration { get; set; } = 0.15f;
    public float ScreenShakeDecay { get; set; } = 5f;
    
    // Cannon configuration
    public float CannonProjectileSpeed { get; set; } = 500f;
    public float CannonProjectileDamage { get; set; } = 50f;
    public float CannonProjectileLifetime { get; set; } = 3f;
    public float CannonProjectileSize { get; set; } = 12f;
    public float CannonCooldown { get; set; } = 0.5f;
    
    // Enemy movement behavior configuration
    public float WobbleAmplitude { get; set; } = 8f;
    public float WobbleFrequency { get; set; } = 3f;
    public float ChaseFallbackMultiplier { get; set; } = 5f;
    public float PatrolSpeedRatio { get; set; } = 0.5f;
    public float TrainCollisionDeathThreshold { get; set; } = 0.5f;
    public float TrainAvoidanceStrength { get; set; } = 50f;
}
