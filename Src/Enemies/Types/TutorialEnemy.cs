using Gamelab.Enemies.Core;
using Gamelab.Enemies.Slots;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Types;

public class TutorialEnemy(Vector2 spawnPosition, EnemyTrainSlot slot, EnemyAmmoDefinition ammoDefinition)
    : Enemy(spawnPosition, slot, ammoDefinition)
{
    protected override float ShootCooldown => GamelabGame.Instance.GameplayConfig.TutorialEnemyShootCooldown;
    protected override float StartingHealth => GamelabGame.Instance.GameplayConfig.TutorialEnemyHealth;
}
