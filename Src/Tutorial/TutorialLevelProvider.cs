using Gamelab.Enemies.Core;
using Gamelab.Levels;

namespace Gamelab.Tutorial;

public class TutorialLevelProvider : ILevelProvider
{
    public const float AmbushDistance = 400f;

    public LevelDefinition GetLevel(int levelNumber) => new LevelDefinition
    {
        LevelDistance = 1500f,
        SpawnEvents =
        [
            new SpawnEvent { Distance = AmbushDistance, Type = EnemyIds.TutorialRifle, Side = "Top" }
        ]
    };
}
