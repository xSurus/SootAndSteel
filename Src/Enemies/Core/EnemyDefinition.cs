using System;

namespace Gamelab.Enemies.Core;

public readonly record struct EnemyDefinition(EnemyType Type)
{
    public string Id
    {
        get
        {
            if (Type != EnemyType.Rifle)
            {
                throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unknown enemy type.");
            }

            return EnemyIds.Rifle;
        }
    }

    public static EnemyDefinition Parse(string id)
    {
        return id == EnemyIds.Rifle
            ? new EnemyDefinition(EnemyType.Rifle)
            : throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown enemy id.");
    }
}
