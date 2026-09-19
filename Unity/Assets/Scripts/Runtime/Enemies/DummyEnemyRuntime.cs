namespace Gamelab.Enemies
{
    // Vertical-slice concrete enemy: Src/Enemies/Types/DummyEnemy.cs has no sound/
    // animation/VFX coupling (unlike Enemy.cs's horse+rider state machine), so it's
    // the one this agent ports behaviorally. Visual representation (the original draws
    // a red filled rectangle) is left to whichever agent wires up SpriteRenderer/prefabs.
    public class DummyEnemyRuntime : EnemyRuntime
    {
    }
}
