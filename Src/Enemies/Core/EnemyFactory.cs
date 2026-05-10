using Gamelab.Enemies.Slots;
using Gamelab.Enemies.Types;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Core;

public static class EnemyFactory
{
    public static AbstractEnemy Create(Vector2 spawnPosition, EnemyTrainSlot slot, EnemyType type = EnemyType.Rifle)
    {
        return type == EnemyType.TutorialRifle
            ? new TutorialEnemy(spawnPosition, slot)
            : new Enemy(spawnPosition, slot);
    }
}
