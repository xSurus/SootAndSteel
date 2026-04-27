using Gamelab.Enemies.Slots;
using EnemyEntity = Gamelab.Enemies.Types.Enemy;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Core;

public static class EnemyFactory
{
    public static AbstractEnemy Create(Vector2 spawnPosition, EnemyTrainSlot slot)
    {
        return new EnemyEntity(spawnPosition, slot);
    }
}
