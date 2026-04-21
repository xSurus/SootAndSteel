namespace Gamelab.Levels;

public interface ILevelProvider
{
    LevelDefinition GetLevel(int levelNumber);
}
