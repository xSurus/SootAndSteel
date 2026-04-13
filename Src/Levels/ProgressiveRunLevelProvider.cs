namespace Gamelab.Levels;

public class ProgressiveRunLevelProvider : IRunLevelProvider
{
    private readonly ProceduralLevelGenerator generator;

    public ProgressiveRunLevelProvider(int playerCount, RunDifficultyConfig config = null)
    {
        float threatScale = GamelabGame.Instance.GameplayConfig.GetThreatScaleForPlayerCount(playerCount);
        float spawnSpacingScale = GamelabGame.Instance.GameplayConfig.GetEnemySpawnSpacingScaleForPlayerCount(playerCount);
        generator = new ProceduralLevelGenerator(config ?? new RunDifficultyConfig(), threatScale, spawnSpacingScale);
    }

    public LevelDefinition GetLevel(int levelNumber)
    {
        if (LevelLoader.TryLoad(levelNumber, out LevelDefinition authoredLevel))
        {
            return authoredLevel;
        }

        return generator.Generate(levelNumber);
    }
}
