using System;

namespace Gamelab.Enemies.Core;

public readonly record struct EnemyDefinition(EnemyType Type)
{
    public string Id => Type switch
    {
        EnemyType.Mounter => EnemyIds.Mounter,
        EnemyType.Rifle => EnemyIds.Rifle,
        EnemyType.Shield => EnemyIds.Shield,
        EnemyType.Anchor => EnemyIds.Anchor,
        EnemyType.Molotov => EnemyIds.Molotov,
        EnemyType.TarThrower => EnemyIds.TarThrower,
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unknown enemy type.")
    };

    public static EnemyDefinition Parse(string id)
    {
        return id switch
        {
            EnemyIds.Mounter => new EnemyDefinition(EnemyType.Mounter),
            EnemyIds.Rifle => new EnemyDefinition(EnemyType.Rifle),
            EnemyIds.Shield => new EnemyDefinition(EnemyType.Shield),
            EnemyIds.Anchor => new EnemyDefinition(EnemyType.Anchor),
            EnemyIds.Molotov => new EnemyDefinition(EnemyType.Molotov),
            EnemyIds.TarThrower => new EnemyDefinition(EnemyType.TarThrower),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown enemy id.")
        };
    }
}
