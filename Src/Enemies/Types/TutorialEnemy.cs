using Gamelab.Enemies.Slots;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Types;

public class TutorialEnemy(Vector2 spawnPosition, EnemyTrainSlot slot) : Enemy(spawnPosition, slot)
{
    protected override float ShootCooldown => GamelabGame.Instance.GameplayConfig.TutorialEnemyShootCooldown;
    protected override float StartingHealth => GamelabGame.Instance.GameplayConfig.TutorialEnemyHealth;
}
