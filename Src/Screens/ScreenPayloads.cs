namespace Gamelab.Screens;

public static class ScreenPayloads
{
    public struct PostLevelResults
    {
        public int CompletedLevelNumber;
        public int CoalRemaining;
    }

    public static PostLevelResults LastPostLevelResults;
}

