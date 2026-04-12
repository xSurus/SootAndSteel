namespace Gamelab.Levels;

public class ProgressiveRunLevelProvider : IRunLevelProvider
{
    private readonly ProceduralLevelGenerator generator;

    public ProgressiveRunLevelProvider(RunDifficultyConfig config = null)
    {
        generator = new ProceduralLevelGenerator(config ?? new RunDifficultyConfig());
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
