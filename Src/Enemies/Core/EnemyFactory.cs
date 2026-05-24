using Gamelab.Enemies.Slots;
using Gamelab.Enemies.Types;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Core;

public static class EnemyFactory
{
    public static AbstractEnemy Create(
        Vector2 spawnPosition,
        EnemyTrainSlot slot,
        EnemyAmmoDefinition ammoDefinition,
        EnemyType type = EnemyType.Rifle)
    {
        return type == EnemyType.TutorialRifle
            ? new TutorialEnemy(spawnPosition, slot, ammoDefinition)
            : new Enemy(spawnPosition, slot, ammoDefinition);
    }
}
