using Gamelab.Serialization;

namespace Gamelab.Levels;

public class ProceduralLevelProvider : ILevelProvider
{
    private readonly ProceduralLevelGenerator generator;

    public ProceduralLevelProvider(int playerCount, RunSession session, LevelGenerationConfig config = null)
    {
        config ??= new LevelGenerationConfig();
        config.RandomSeed = session.RunSeed;

        float threatScale = GamelabGame.Instance.GameplayConfig.GetThreatScaleForPlayerCount(playerCount);
        float spawnSpacingScale =
            GamelabGame.Instance.GameplayConfig.GetEnemySpawnSpacingScaleForPlayerCount(playerCount);

        generator = new ProceduralLevelGenerator(config, threatScale, spawnSpacingScale);
    }

    public LevelDefinition GetLevel(int levelNumber) => generator.Generate(levelNumber);
}
