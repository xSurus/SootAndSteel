using System;
using Gamelab.Config;

namespace Gamelab.Screens;

public readonly record struct LevelRewardBreakdown(
    int DeliveryReward,
    int TimeAdjustment,
    int TotalCredits,
    string TimeLineLabel,
    string TimeLineDescription)
{
    public static LevelRewardBreakdown FromCompletion(float actualTime, float referenceTime, GameplayConfig config)
    {
        int baseReward = config.LevelBaseReward;
        int expectedBonus = config.LevelReferenceBonus;
        float rawBonus = expectedBonus * MathF.Sqrt(referenceTime / Math.Max(actualTime, 0.1f));
        int actualBonus = (int)MathF.Round(MathF.Max(0f, rawBonus));
        int deliveryReward = baseReward + expectedBonus;
        int timeAdjustment = actualBonus - expectedBonus;

        return new(
            deliveryReward,
            timeAdjustment,
            deliveryReward + timeAdjustment,
            GetTimeLineLabel(timeAdjustment),
            GetTimeLineDescription(timeAdjustment));
    }

    public static string FormatSignedAmount(int amount) => amount >= 0 ? $"+{amount}" : amount.ToString();

    private static string GetTimeLineLabel(int timeAdjustment) => timeAdjustment switch
    {
        > 0 => "Time Bonus",
        < 0 => "Time Penalty",
        _ => "On Time"
    };

    private static string GetTimeLineDescription(int timeAdjustment) => timeAdjustment switch
    {
        > 0 => "Arrived ahead of schedule",
        < 0 => "Arrived behind schedule",
        _ => "Met scheduled arrival"
    };
}
