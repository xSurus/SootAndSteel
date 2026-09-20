using System;

namespace Gamelab.Levels
{
    /// <summary>
    /// Port of GameplayConfig.GetThreatScaleForPlayerCount / GetEnemySpawnSpacingScaleForPlayerCount.
    /// Values are the effective Src ones: GameplayConfig defaults overlaid by Src/Content/Data/gameplay.json
    /// (ThreatScalePerExtraPlayer 0.45, ThreatScaleExponent 0.85, EnemySpawnPacingScaleStrength 0.25).
    /// </summary>
    public static class PlayerCountScaling
    {
        private const int MaxSupportedPlayers = 4;
        private const float ThreatScalePerExtraPlayer = 0.45f;
        private const float ThreatScaleExponent = 0.85f;
        private const float EnemySpawnPacingScaleStrength = 0.25f;

        public static float GetThreatScale(int playerCount)
        {
            int extraPlayers = Math.Min(MaxSupportedPlayers, Math.Max(1, playerCount)) - 1;
            if (extraPlayers <= 0)
            {
                return 1f;
            }

            float exponent = Math.Max(0.01f, ThreatScaleExponent);
            float additionalThreat = ThreatScalePerExtraPlayer * MathF.Pow(extraPlayers, exponent);
            return Math.Max(1f, 1f + additionalThreat);
        }

        public static float GetEnemySpawnSpacingScale(int playerCount)
        {
            float threatScale = GetThreatScale(playerCount);
            float pacingStrength = Math.Min(1f, Math.Max(0f, EnemySpawnPacingScaleStrength));
            float blendedThreat = 1f + (threatScale - 1f) * pacingStrength;
            return Math.Min(1f, Math.Max(0.4f, 1f / blendedThreat));
        }
    }
}
