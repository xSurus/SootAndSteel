namespace Gamelab.Screens;

public static class StageNaming
{
    public static string GetStageTitle(int stageNumber)
    {
        if (stageNumber >= 7)
        {
            int exclamationCount = stageNumber - 6;
            return $"FULL STEAM{new string('!', exclamationCount)}";
        }

        return stageNumber switch
        {
            1 => "First Departure",
            2 => "Unstable Schedule",
            3 => "Signal Failure",
            4 => "Critical Junction",
            5 => "Runaway Line",
            6 => "Full Steam",
            _ => "Full Steam"
        };
    }
}
