using System;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public static class EnemyFactory
{
    public static AbstractEnemy Create(GameplayContext gameplayContext, EnemyDefinition definition, Vector2 spawnPosition,
        Random random, EnemyTrainSlot slot)
    {
        return definition.Type switch
        {
            EnemyType.Mounter => new MounterEnemy(gameplayContext, spawnPosition, slot),
            EnemyType.Rifle => new RifleEnemy(gameplayContext, spawnPosition, random, slot),
            EnemyType.Shield => new ShieldEnemy(gameplayContext, spawnPosition, random, slot),
            EnemyType.Anchor => throw new NotSupportedException("Anchor enemy is not implemented yet."),
            EnemyType.Molotov => throw new NotSupportedException("Molotov enemy is not implemented yet."),
            EnemyType.TarThrower => throw new NotSupportedException("Tar thrower enemy is not implemented yet."),
            _ => throw new ArgumentOutOfRangeException(nameof(definition), definition, "Unknown enemy definition.")
        };
    }
}
