namespace Gamelab.Screens
{
    /// <summary>Port of Src Screens/StageNaming.cs.</summary>
    public static class StageNaming
    {
        public static string GetStageTitle(int stageNumber)
        {
            if (stageNumber >= 7)
            {
                int exclamationCount = stageNumber - 6;
                return $"FULL STEAM{new string('!', exclamationCount)}";
            }

            switch (stageNumber)
            {
                case 1: return "First Departure";
                case 2: return "Unstable Schedule";
                case 3: return "Signal Failure";
                case 4: return "Critical Junction";
                case 5: return "Runaway Line";
                case 6: return "Full Steam";
                default: return "Full Steam";
            }
        }
    }
}
