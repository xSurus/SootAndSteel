namespace Gamelab.Levels;

public class RunDifficultyConfig
{
    public float BaseDistance { get; set; } = 3000f;
    public float DistanceGrowthPerLevel { get; set; } = 300f;

    public float BaseEnemyBudget { get; set; } = 8f;
    public float BudgetGrowthPerLevel { get; set; } = 2.5f;
    public float BudgetMultiplierPerLevel { get; set; } = 1.12f;

    public int ShooterCost { get; set; } = 3;
    public int ThiefCost { get; set; } = 2;

    public float BaseShooterChance { get; set; } = 0.45f;
    public float ShooterChanceIncreasePerLevel { get; set; } = 0.02f;
    public float MaxShooterChance { get; set; } = 0.85f;

    public float MinSpawnSpacing { get; set; } = 120f;
    public float MaxSpawnSpacing { get; set; } = 450f;
    public int RandomSeed { get; set; } = 1337;
}
