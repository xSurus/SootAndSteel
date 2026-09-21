using System;
using System.Globalization;

namespace Gamelab.Screens
{
    /// <summary>
    /// Port of Src Screens/LevelRewardBreakdown.cs. The Unity port has no GameplayConfig, so the
    /// two config values are parameters with the Src defaults (LevelBaseReward 25, LevelReferenceBonus 20).
    /// Src rounds with MathF.Round (banker's rounding), kept as is.
    /// </summary>
    public readonly struct LevelRewardBreakdown
    {
        public const int DefaultBaseReward = 25;
        public const int DefaultReferenceBonus = 20;

        public int DeliveryReward { get; }
        public int TimeAdjustment { get; }
        public int TotalCredits { get; }
        public string TimeLineLabel { get; }
        public string TimeLineDescription { get; }

        public LevelRewardBreakdown(int deliveryReward, int timeAdjustment, int totalCredits,
            string timeLineLabel, string timeLineDescription)
        {
            DeliveryReward = deliveryReward;
            TimeAdjustment = timeAdjustment;
            TotalCredits = totalCredits;
            TimeLineLabel = timeLineLabel;
            TimeLineDescription = timeLineDescription;
        }

        public static LevelRewardBreakdown FromCompletion(float actualTime, float referenceTime,
            int baseReward = DefaultBaseReward, int referenceBonus = DefaultReferenceBonus)
        {
            int expectedBonus = referenceBonus;
            float rawBonus = expectedBonus * MathF.Sqrt(referenceTime / Math.Max(actualTime, 0.1f));
            int actualBonus = (int)MathF.Round(MathF.Max(0f, rawBonus));
            int deliveryReward = baseReward + expectedBonus;
            int timeAdjustment = actualBonus - expectedBonus;

            return new LevelRewardBreakdown(
                deliveryReward,
                timeAdjustment,
                deliveryReward + timeAdjustment,
                timeAdjustment > 0 ? "Time Bonus" : timeAdjustment < 0 ? "Time Penalty" : "On Time",
                timeAdjustment > 0 ? "Arrived ahead of schedule"
                    : timeAdjustment < 0 ? "Arrived behind schedule" : "Met scheduled arrival");
        }

        public static string FormatSignedAmount(int amount) => amount >= 0
            ? "+" + amount.ToString(CultureInfo.InvariantCulture)
            : amount.ToString(CultureInfo.InvariantCulture);
    }
}
