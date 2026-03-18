namespace Gamelab.Config;

public class GameplayConfig
{
    public float PixelsPerMeter { get; set; } = 100f;
    public float FixedTimeStep { get; set; } = 1f / 60f;
    public float MaxAccumulatedDeltaSeconds { get; set; } = 0.25f;
    public int TrainTileSize { get; set; } = 80;
    public int TrainWidth { get; set; } = 8;
    public int TrainHeight { get; set; } = 6;
    public float TrainSpeed { get; set; } = 150f;
    public float SpawnOffsetPixels { get; set; } = 100f;
    public float PlayerRadiusPixels { get; set; } = 24f;
    public float PlayerDensity { get; set; } = 3f;
    public float PlayerLinearDamping { get; set; } = 20f;
    public float PlayerMaxVelocity { get; set; } = 5f;
    public float PlayerVelocityLerpFactor { get; set; } = 0.8f;
    public float PlayerInteractDistancePixels { get; set; } = 40f;
    public float PlayerHeldItemOffsetRadiusMultiplier { get; set; } = 1.5f;
    public float PlayerHeldItemSizeRadiusMultiplier { get; set; } = 0.8f;
    public float PlayerVisionConeLengthRadiusMultiplier { get; set; } = 2.5f;
    public float PlayerVisionConeAngleDegrees { get; set; } = 30f;
    public float InputInitialRepeatDelaySeconds { get; set; } = 0.3f;
    public float InputRepeatRateSeconds { get; set; } = 0.1f;
    public float InputDirectionPressThreshold { get; set; } = 0.5f;
    public float InputMovementDeadzoneSquared { get; set; } = 0.25f;
    public float MenuVolumeStep { get; set; } = 0.05f;
    public int MenuPanelBottomOffsetPixels { get; set; } = 330;
    public int MenuPanelHeightPixels { get; set; } = 250;
    public int MenuPanelFirstItemOffsetYPixels { get; set; } = 20;
    public int MenuPanelItemSpacingPixels { get; set; } = 85;
    public int MenuPlayerMarkerNearOffsetPixels { get; set; } = 60;
    public int MenuPlayerMarkerFarOffsetPixels { get; set; } = 140;
    public float WorldScrollerDefaultSpeed { get; set; } = 100f;
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
}
