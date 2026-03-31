using Gamelab.Utils.Logging;

namespace Gamelab.Levels;

public static class LevelLoader
{
    private static readonly Logger logger = new("LevelLoader");

    public static LevelDefinition Load(int levelNumber)
    {
        string path = $"levels/level{levelNumber}.json";
        try
        {
            var def = GamelabGame.Instance.jsonLoader.LoadJson<LevelDefinition>(path);
            logger.Info($"Loaded level definition: {path}");
            return def;
        }
        catch
        {
            logger.Warning($"Could not load {path}, falling back to level 1 definition.");
            // load level 1 definition
            return Load(1);
        }
    }
}
