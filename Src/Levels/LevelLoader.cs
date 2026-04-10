using Gamelab.Utils.Logging;

namespace Gamelab.Levels;

public static class LevelLoader
{
    private static readonly Logger logger = new("LevelLoader");

    public static bool TryLoad(int levelNumber, out LevelDefinition definition)
    {
        string path = $"levels/level{levelNumber}.json";
        try
        {
            definition = GamelabGame.Instance.jsonLoader.LoadJson<LevelDefinition>(path);
            logger.Info($"Loaded level definition: {path}");
            return true;
        }
        catch
        {
            definition = null;
            logger.Warning($"Could not load level definition: {path}");
            return false;
        }
    }

    public static LevelDefinition Load(int levelNumber)
    {
        if (TryLoad(levelNumber, out LevelDefinition definition))
        {
            return definition;
        }

        if (levelNumber != 1 && TryLoad(1, out definition))
        {
            logger.Warning($"Falling back to level 1 definition.");
            return definition;
        }

        logger.Warning("No level file could be loaded. Returning empty fallback level definition.");
        return new LevelDefinition();
    }
}
