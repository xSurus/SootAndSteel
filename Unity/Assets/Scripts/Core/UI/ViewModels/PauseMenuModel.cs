using System;

namespace Gamelab.UI.ViewModels
{
    public enum PauseItem
    {
        Continue,
        Options,
        Controls,
        Exit,
    }

    public class PauseMenuModel
    {
        private static readonly PauseItem[] ItemList =
            { PauseItem.Continue, PauseItem.Options, PauseItem.Controls, PauseItem.Exit };

        public PauseMenuModel(OptionsViewModel options)
        {
            Options = options;
        }

        public OptionsViewModel Options { get; }
        public PauseItem[] Items => ItemList;
        public bool IsPaused { get; private set; }
        public bool ControlsOpen { get; private set; }
        public int SelectionIndex { get; private set; }

        public event Action ExitRequested;

        public void MoveUp() => SelectionIndex = (SelectionIndex - 1 + ItemList.Length) % ItemList.Length;

        public void MoveDown() => SelectionIndex = (SelectionIndex + 1) % ItemList.Length;

        public void Toggle()
        {
            IsPaused = !IsPaused;
            SelectionIndex = 0;
            if (!IsPaused)
            {
                Options.Close();
                ControlsOpen = false;
            }
        }

        /// <summary>A pause press toggles unless the options menu is open (it uses pause to close itself).</summary>
        public bool IsToggleRequested(bool anyPausePressed) => anyPausePressed && !Options.IsOpen;

        public void Confirm()
        {
            switch (ItemList[SelectionIndex])
            {
                case PauseItem.Continue: Toggle(); break;
                case PauseItem.Options: Options.Open(); break;
                case PauseItem.Controls: ControlsOpen = true; break;
                case PauseItem.Exit: ExitRequested?.Invoke(); break;
            }
        }

        public void CloseControls() => ControlsOpen = false;
    }
}
