namespace Gamelab.Config
{
    // Cannon-related GameplayConfig values (Src/Config/GameplayConfig.cs), pixels/seconds.
    // None of these four appear in Src/Content/Data/gameplay.json except trainTileSize (80) and
    // inputMovementDeadzoneSquared (0.25), both equal to the class defaults.
    public sealed class CannonTuning
    {
        public static readonly CannonTuning Default = new CannonTuning(0.5f, 6f, 80, 0.25f);

        public float CannonCooldown { get; }
        public float CannonRotationSpeed { get; }
        public int TrainTileSize { get; }
        public float InputMovementDeadzoneSquared { get; }

        public CannonTuning(float cannonCooldown, float cannonRotationSpeed, int trainTileSize,
            float inputMovementDeadzoneSquared)
        {
            CannonCooldown = cannonCooldown;
            CannonRotationSpeed = cannonRotationSpeed;
            TrainTileSize = trainTileSize;
            InputMovementDeadzoneSquared = inputMovementDeadzoneSquared;
        }
    }
}
