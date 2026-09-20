namespace Gamelab.Config
{
    // Enemy-related GameplayConfig values (Src/Config/GameplayConfig.cs), pixels/seconds.
    // Src loads Src/Content/Data/gameplay.json over the defaults. The json only repeats
    // enemySize 72, rifleMaxSpeed 700, riflePreferredDistance 30, enemyShootCooldown 4 and
    // enemyShootSpread 0.2, all equal to the class defaults. The rest (EnemyHealth,
    // TutorialEnemy*, EnemyFleeDelay, TrainTileSize) come from the class defaults only.
    // Wave B may build another instance.
    public sealed class EnemyTuning
    {
        public static readonly EnemyTuning Default = new EnemyTuning(
            100f, 72f, 700f, 30f, 4f, 6f, 1f, 0.2f, 0.75f, 80);

        public float EnemyHealth { get; }
        public float EnemySize { get; }
        public float RifleMaxSpeed { get; }
        public float RiflePreferredDistance { get; }
        public float EnemyShootCooldown { get; }
        public float TutorialEnemyShootCooldown { get; }
        public float TutorialEnemyHealth { get; }
        public float EnemyShootSpread { get; }
        public float EnemyFleeDelay { get; }
        public int TrainTileSize { get; }

        public EnemyTuning(float enemyHealth, float enemySize, float rifleMaxSpeed, float riflePreferredDistance,
            float enemyShootCooldown, float tutorialEnemyShootCooldown, float tutorialEnemyHealth,
            float enemyShootSpread, float enemyFleeDelay, int trainTileSize)
        {
            EnemyHealth = enemyHealth;
            EnemySize = enemySize;
            RifleMaxSpeed = rifleMaxSpeed;
            RiflePreferredDistance = riflePreferredDistance;
            EnemyShootCooldown = enemyShootCooldown;
            TutorialEnemyShootCooldown = tutorialEnemyShootCooldown;
            TutorialEnemyHealth = tutorialEnemyHealth;
            EnemyShootSpread = enemyShootSpread;
            EnemyFleeDelay = enemyFleeDelay;
            TrainTileSize = trainTileSize;
        }
    }
}
