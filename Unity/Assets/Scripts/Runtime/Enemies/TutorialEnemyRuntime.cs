namespace Gamelab.Enemies
{
    // Src/Enemies/Types/TutorialEnemy.cs: slower shots and lower health.
    public class TutorialEnemyRuntime : RifleEnemyRuntime
    {
        protected override float ShootCooldown => Tuning.TutorialEnemyShootCooldown;
        protected override void ApplyStartingHealth() => Health = Tuning.TutorialEnemyHealth;
    }
}
