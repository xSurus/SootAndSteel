using System;
using System.Globalization;

namespace Gamelab.Screens
{
    public readonly struct PostDeathStatsSnapshot
    {
        public int EnemiesDefeated { get; }
        public float DistanceTravelledMeters { get; }
        public int StagesDefeated { get; }
        public int UpgradesBought { get; }

        public PostDeathStatsSnapshot(int enemiesDefeated, float distanceTravelledMeters, int stagesDefeated, int upgradesBought)
        {
            EnemiesDefeated = enemiesDefeated;
            DistanceTravelledMeters = distanceTravelledMeters;
            StagesDefeated = stagesDefeated;
            UpgradesBought = upgradesBought;
        }
    }

    /// <summary>Value strings and incident line of Src FailScreen. Math.Round is banker's rounding, as in Src.</summary>
    public static class PostDeathStatsText
    {
        public static string EnemiesDefeated(PostDeathStatsSnapshot s) => s.EnemiesDefeated.ToString(CultureInfo.InvariantCulture);

        public static string Distance(PostDeathStatsSnapshot s)
            => $"{((int)Math.Round(s.DistanceTravelledMeters)).ToString(CultureInfo.InvariantCulture)} m";

        public static string StagesDefeated(PostDeathStatsSnapshot s) => Math.Max(0, s.StagesDefeated).ToString(CultureInfo.InvariantCulture);

        public static string UpgradesBought(PostDeathStatsSnapshot s) => Math.Max(0, s.UpgradesBought).ToString(CultureInfo.InvariantCulture);

        public static string BuildIncidentLine(int levelNumber, DateTime now)
        {
            string levelTag = $"N. {Math.Max(1, levelNumber).ToString("00", CultureInfo.InvariantCulture)} / F";
            string dateTag = $"FILED {now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}".ToUpperInvariant();
            string timeTag = now.ToString("HH:mm", CultureInfo.InvariantCulture);
            return $"{levelTag} · {dateTag} · {timeTag}";
        }
    }
}
