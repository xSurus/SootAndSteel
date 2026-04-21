using System;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public static class EnemyFactory
{
    public static AbstractEnemy Create(EnemyDefinition definition, Vector2 spawnPosition, EnemyTrainSlot slot)
    {
        return definition.Type switch
        {
            EnemyType.Mounter => new MounterEnemy(spawnPosition, slot),
            EnemyType.Rifle => new RifleEnemy(spawnPosition, slot),
            EnemyType.Shield => new ShieldEnemy(spawnPosition, slot),
            EnemyType.Anchor => new AnchorEnemy(spawnPosition, slot),
            EnemyType.Molotov => new MolotovEnemy(spawnPosition, slot),
            EnemyType.TarThrower => new TarThrowerEnemy(spawnPosition, slot),
            _ => throw new ArgumentOutOfRangeException(nameof(definition), definition, "Unknown enemy definition.")
        };
    }
}