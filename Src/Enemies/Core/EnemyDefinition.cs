using System;

namespace Gamelab.Enemies.Core;

public readonly record struct EnemyDefinition(EnemyType Type)
{
    public string Id => Type switch
    {
        EnemyType.Rifle => EnemyIds.Rifle,
        EnemyType.Dummy => EnemyIds.Dummy,
        EnemyType.TutorialRifle => EnemyIds.TutorialRifle,
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unknown enemy type.")
    };

    public static EnemyDefinition Parse(string id)
    {
        return id switch
        {
            EnemyIds.Rifle => new EnemyDefinition(EnemyType.Rifle),
            EnemyIds.Dummy => new EnemyDefinition(EnemyType.Dummy),
            EnemyIds.TutorialRifle => new EnemyDefinition(EnemyType.TutorialRifle),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown enemy id.")
        };
    }
}
