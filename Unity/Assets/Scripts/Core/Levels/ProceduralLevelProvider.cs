namespace Gamelab.Levels
{
    public class ProceduralLevelProvider : ILevelProvider
    {
        private readonly ProceduralLevelGenerator generator;

        public ProceduralLevelProvider(int playerCount, int runSeed, LevelGenerationConfig config = null)
        {
            config = config ?? new LevelGenerationConfig();
            config.RandomSeed = runSeed;

            generator = new ProceduralLevelGenerator(
                config,
                PlayerCountScaling.GetThreatScale(playerCount),
                PlayerCountScaling.GetEnemySpawnSpacingScale(playerCount));
        }

        public LevelDefinition GetLevel(int levelNumber) => generator.Generate(levelNumber);
    }
}
