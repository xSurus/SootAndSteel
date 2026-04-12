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
}
