using System;

namespace Gamelab.UI
{
    /// <summary>
    /// State of the hint and decision bubble from Src DialogueOverlay (ShowPassive, ShowDecision, Hide).
    /// The typewriter reveal, world anchor, tail and Show(line) belong to the deferred tutorial dialogue slice.
    /// </summary>
    public class DialogBubbleModel
    {
        public enum BubbleMode { Hidden, Passive, Decision }

        public BubbleMode Mode { get; private set; }
        public string Speaker { get; private set; }
        public string Text { get; private set; }
        public string LeftLabel { get; private set; }
        public string RightLabel { get; private set; }

        public event Action Changed;

        public void ShowPassive(string speaker, string text) => Set(BubbleMode.Passive, speaker, text, null, null);

        public void ShowDecision(string speaker, string text, string leftLabel, string rightLabel) =>
            Set(BubbleMode.Decision, speaker, text, leftLabel, rightLabel);

        public void Hide() => Set(BubbleMode.Hidden, null, null, null, null);

        private void Set(BubbleMode mode, string speaker, string text, string left, string right)
        {
            if (Mode == mode && Speaker == speaker && Text == text && LeftLabel == left && RightLabel == right)
                return;
            Mode = mode;
            Speaker = speaker;
            Text = text;
            LeftLabel = left;
            RightLabel = right;
            Changed?.Invoke();
        }
    }
}
