namespace Gamelab.Levels;

public interface IRunLevelProvider
{
    LevelDefinition GetLevel(int levelNumber);
}
