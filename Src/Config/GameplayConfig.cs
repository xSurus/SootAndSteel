using System;

namespace Gamelab.Config;

public class GameplayConfig
{
    private const int MaxSupportedPlayers = 4;

    public float PixelsPerMeter { get; set; } = 100f;
    public float FixedTimeStep { get; set; } = 1f / 60f;
    public float MaxAccumulatedDeltaSeconds { get; set; } = 0.25f;
    public int TrainTileSize { get; set; } = 80;
    public int TrainWidth { get; set; } = 8;
    public int TrainHeight { get; set; } = 6;
    public float TrainSpeedSlow { get; set; } = 150f;
    public float TrainSpeedDefault { get; set; } = 300f;
    public float TrainSpeedFast { get; set; } = 600f;
    public float TrainAccelerationRate { get; set; } = 50f;
    public float TrainBurnMultiplierSlow { get; set; } = 1f;
    public float TrainBurnMultiplierDefault { get; set; } = 1f;
    public float TrainBurnMultiplierFast { get; set; } = 1f;
    public float PlayerRadiusPixels { get; set; } = 10f;
    public float PlayerDensity { get; set; } = 3f;
    public float PlayerLinearDamping { get; set; } = 20f;
    public float PlayerMaxVelocity { get; set; } = 5f;
    public float PlayerVelocityLerpFactor { get; set; } = 0.8f;
    public float PlayerInteractDistancePixels { get; set; } = 60f;
    public float PlayerHeldItemOffsetRadiusMultiplier { get; set; } = 1.5f;
    public float PlayerHeldItemSizeRadiusMultiplier { get; set; } = 0.8f;
    public float PlayerStunDurationSeconds { get; set; } = 4f;
    public float PlayerReviveDurationSeconds { get; set; } = 1.5f;
    public float AllPlayersStunnedFailDelaySeconds { get; set; } = 1.5f;
    public float FireStunDurationSeconds { get; set; } = 2f;
    public float InputInitialRepeatDelaySeconds { get; set; } = 0.3f;
    public float InputRepeatRateSeconds { get; set; } = 0.1f;
    public float InputDirectionPressThreshold { get; set; } = 0.5f;
    public float InputMovementDeadzoneSquared { get; set; } = 0.25f;
    public float MenuVolumeStep { get; set; } = 0.05f;
    public int ScrollerWorldTextureWidth { get; set; } = 480;
    public float CoalOvenMaxFuel { get; set; } = 30f;
    public float CoalOvenBurnRate { get; set; } = 1f;
    public float CoalOvenRefuelAmount { get; set; } = 10f;
    public float CoalOvenLowFuelThreshold { get; set; } = 0.25f;
    public float WallMaxHealth { get; set; } = 100f;
    public float WallHealthRestoredPerSecond { get; set; } = 40f;
    public float GrabbableLinearDamping { get; set; } = 100f;
    public float GrabbableRotationalResistance { get; set; } = 40f;
    public float DepartHoldSeconds { get; set; } = 0.75f;

    // Enemy configuration (general)
    public float EnemyHealth { get; set; } = 100f;
    public float EnemySize { get; set; } = 72f;
    public float EnemySpawnOffsetX { get; set; } = 100f;

    // Rifle enemy configuration
    public float RifleMaxSpeed { get; set; } = 250f;
    public float RiflePreferredDistance { get; set; } = 150f;
    public float EnemyShootCooldown { get; set; } = 2f;
    public float EnemyShootSpread { get; set; } = 0.2f;

    // Shield enemy configuration
    public float ShieldMaxSpeed { get; set; } = 220f;
    public float ShieldPreferredDistance { get; set; } = 150f;
    public float ShieldDurationSeconds { get; set; } = 1.2f;
    public float ShieldAimDurationSeconds { get; set; } = 0.8f;
    public float ShieldRecoverDurationSeconds { get; set; } = 0.8f;
    public float ShieldBlockArcDegrees { get; set; } = 120f;

    // Anchor enemy configuration
    public float AnchorMaxSpeed { get; set; } = 220f;
    public float AnchorPreferredDistance { get; set; } = 120f;
    public float AnchorDeployDurationSeconds { get; set; } = 1f;
    public float AnchorRetreatSpeed { get; set; } = 320f;
    public float AnchorCutDurationSeconds { get; set; } = 1.75f;
    public float AnchorSpeedMultiplierPerActiveAnchor { get; set; } = 0.5f;
    public float AnchorMinimumSpeedMultiplier { get; set; } = 0.1f;

    // Molotov enemy configuration
    public float MolotovMaxSpeed { get; set; } = 210f;
    public float MolotovPreferredDistance { get; set; } = 160f;
    public float MolotovAimDurationSeconds { get; set; } = 0.8f;
    public float MolotovRecoverDurationSeconds { get; set; } = 1f;
    public float MolotovProjectileSpeed { get; set; } = 260f;
    public float MolotovProjectileLifetime { get; set; } = 0.8f;
    public float MolotovProjectileSize { get; set; } = 14f;
    public float FireZoneRadius { get; set; } = 90f;
    public float FireZoneDurationSeconds { get; set; } = 5f;

    // Tar thrower configuration
    public float TarThrowerMaxSpeed { get; set; } = 210f;
    public float TarThrowerPreferredDistance { get; set; } = 160f;
    public float TarThrowerAimDurationSeconds { get; set; } = 0.9f;
    public float TarThrowerRecoverDurationSeconds { get; set; } = 1f;
    public float TarProjectileSpeed { get; set; } = 240f;
    public float TarProjectileLifetime { get; set; } = 0.85f;
    public float TarProjectileSize { get; set; } = 14f;
    public float TarCleanDurationSeconds { get; set; } = 1.75f;

    // Mounter enemy configuration
    public float MounterApproachSpeed { get; set; } = 200f;
    public float MounterFleeSpeed { get; set; } = 350f;
    public float MounterMountDuration { get; set; } = 0.3f;
    public float MounterStealDuration { get; set; } = 1.5f;
    public int MounterCoalAmount { get; set; } = 5;

    // Train temperature configuration
    public float TrainMaxTemperature { get; set; } = 100f;
    public float TrainTemperatureDecreasePerSecondPerBreachedWall { get; set; } = 3f;
    public float TrainTemperatureDecreasePerSecondEngineOff { get; set; } = 3f;
    public float TrainTemperatureIncreasePerSecond { get; set; } = 3f;

    // Per-player run scaling. Threat scales harder than maintenance to keep co-op challenging without
    // turning fuel/temperature management into pure busywork at high player counts.
    public float ThreatScalePerExtraPlayer { get; set; } = 0.75f;
    public float ThreatScaleExponent { get; set; } = 0.85f;
    public float EnemySpawnPacingScaleStrength { get; set; } = 0.6f;
    public float MaintenanceScalePerExtraPlayer { get; set; } = 0.35f;

    // Screen shake configuration
    public float ScreenShakeIntensity { get; set; } = 8f;
    public float ScreenShakeDuration { get; set; } = 0.15f;
    public float ScreenShakeDecay { get; set; } = 5f;

    // Cannon configuration
    public float CannonProjectileSpeed { get; set; } = 800f;
    public float CannonProjectileDamage { get; set; } = 50f;
    public float CannonProjectileLifetime { get; set; } = 3f;
    public float CannonProjectileSize { get; set; } = 12f;
    public float CannonProjectilePierce { get; set; } = 1f;
    public float CannonProjectileSpread { get; set; } = 0.2f;
    public float CannonCooldown { get; set; } = 0.5f;
    public float CannonRotationSpeed { get; set; } = 6f;

    // Camera configuration
    public float CameraLerpFactor { get; set; } = 4.0f;

    public float GetThreatScaleForPlayerCount(int playerCount)
    {
        int clampedCount = Math.Clamp(playerCount, 1, MaxSupportedPlayers);
        int extraPlayers = clampedCount - 1;
        if (extraPlayers <= 0)
        {
            return 1f;
        }

        float exponent = Math.Max(0.01f, ThreatScaleExponent);
        float additionalThreat = ThreatScalePerExtraPlayer * MathF.Pow(extraPlayers, exponent);
        return Math.Max(1f, 1f + additionalThreat);
    }

    public float GetEnemySpawnSpacingScaleForPlayerCount(int playerCount)
    {
        float threatScale = GetThreatScaleForPlayerCount(playerCount);
        float pacingStrength = Math.Clamp(EnemySpawnPacingScaleStrength, 0f, 1f);
        float blendedThreat = Lerp(1f, threatScale, pacingStrength);
        return Math.Clamp(1f / blendedThreat, 0.4f, 1f);
    }

    public float GetMaintenanceScaleForPlayerCount(int playerCount)
    {
        int clampedCount = Math.Clamp(playerCount, 1, MaxSupportedPlayers);
        int extraPlayers = clampedCount - 1;
        return Math.Max(1f, 1f + extraPlayers * MaintenanceScalePerExtraPlayer);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}