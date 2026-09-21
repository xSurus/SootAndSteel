using System;

namespace Gamelab.UI
{
    /// <summary>
    /// Display state of Src JoinScreen, derived from a joined-count function (the roster size).
    /// Joining and advancing are not here: PlayerJoinManager joins players (A on a pad, Space on
    /// keyboard) and JoinFlowController raises OnReadyToAdvance when a joined player presses Start.
    /// Slot i is joined when i is below the count, the same rule as JoinFlowController.IsSlotJoined.
    /// Gaps between Src JoinScreen and that seam:
    /// Src calls playerManager.Reset() in Initialize. The seam has no reset, so the join screen
    /// controller does not clear the roster.
    /// Src Start switches to MainMenuScreen. The seam only raises OnReadyToAdvance, the scene
    /// switch belongs to whoever subscribes.
    /// Src refreshes each frame and compares previousSlotJoined. Refresh does the same and raises
    /// Changed when the title or any slot state changed.
    /// </summary>
    public class JoinScreenModel
    {
        public const int SlotCount = 4;
        public const string TitleEmpty = "Enter the train";
        public const string TitleJoined = "Press start to advance";
        public const string SilhouetteFigure = "Silhouette";

        // Src JoinPlayerComponent states joined1..joined4 in the Gum project, in slot order.
        private static readonly string[] JoinedFigures = { "IdleA0", "IdleA3", "IdleA1", "IdleA2" };

        private readonly Func<int> joinedCount;
        private readonly bool[] joined = new bool[SlotCount];
        private string title = TitleEmpty;

        public event Action Changed;

        public JoinScreenModel(Func<int> joinedCount)
        {
            this.joinedCount = joinedCount;
        }

        public string Title => title;

        public bool IsJoined(int slot) => joined[slot];
        public string FigureId(int slot) => joined[slot] ? JoinedFigures[slot] : SilhouetteFigure;
        public string ButtonText(int slot) => joined[slot] ? "Joined" : "Join";
        public XboxButtonAtlas.Face ButtonFace(int slot) =>
            joined[slot] ? XboxButtonAtlas.Face.Start : XboxButtonAtlas.Face.A;

        public void Refresh()
        {
            int count = joinedCount();
            bool changed = false;
            for (int i = 0; i < SlotCount; i++)
            {
                bool now = i < count;
                if (now == joined[i]) continue;
                joined[i] = now;
                changed = true;
            }

            string newTitle = count > 0 ? TitleJoined : TitleEmpty;
            if (newTitle != title)
            {
                title = newTitle;
                changed = true;
            }

            if (changed) Changed?.Invoke();
        }
    }
}
