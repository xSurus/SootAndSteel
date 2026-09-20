namespace Gamelab.Map
{
    // Effective Src GameplayConfig camera zoom limits. gameplay.json has no camera zoom keys
    // (only cameraLerpFactor), so the class defaults 0.4 and 0.8 apply.
    public static class CameraTuning
    {
        public const float MinZoom = 0.4f;
        public const float MaxZoom = 0.8f;
    }
}
