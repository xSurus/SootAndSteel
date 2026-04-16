using Gamelab.Serialization;

namespace Gamelab.Levels;

public class ProgressiveRunLevelProvider : IRunLevelProvider
{
    private readonly ProceduralLevelGenerator generator;

    public ProgressiveRunLevelProvider(int playerCount, RunSession session, RunDifficultyConfig config = null)
    {
        config ??= new RunDifficultyConfig();
        config.RandomSeed = session.RunSeed;

        float threatScale = GamelabGame.Instance.GameplayConfig.GetThreatScaleForPlayerCount(playerCount);
        float spawnSpacingScale =
            GamelabGame.Instance.GameplayConfig.GetEnemySpawnSpacingScaleForPlayerCount(playerCount);

        generator = new ProceduralLevelGenerator(config, threatScale, spawnSpacingScale);
    }

    public LevelDefinition GetLevel(int levelNumber) => generator.Generate(levelNumber);
}