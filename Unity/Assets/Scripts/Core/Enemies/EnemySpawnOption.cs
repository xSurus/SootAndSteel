namespace Gamelab.Enemies.Core
{
    public readonly struct EnemySpawnOption
    {
        public EnemyType Type { get; }
        public EnemyAmmoDefinition AmmoDefinition { get; }

        public EnemySpawnOption(EnemyType type, EnemyAmmoDefinition ammoDefinition)
        {
            Type = type;
            AmmoDefinition = ammoDefinition;
        }
    }
}
