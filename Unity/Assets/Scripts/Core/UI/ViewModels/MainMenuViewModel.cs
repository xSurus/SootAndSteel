using System;
using System.Collections.Generic;

namespace Gamelab.UI.ViewModels
{
    public class MainMenuViewModel
    {
        public class Entry
        {
            public string Label { get; }
            public Action OnSelected { get; }

            public Entry(string label, Action onSelected)
            {
                Label = label;
                OnSelected = onSelected;
            }
        }

        private readonly List<Entry> entries;
        private readonly Dictionary<int, int> playerSelections = new Dictionary<int, int>();

        public IReadOnlyList<Entry> Entries => entries;

        /// <summary>Maps playerIndex -> selected entry index.</summary>
        public IReadOnlyDictionary<int, int> PlayerSelections => playerSelections;

        public event Action<int, int> OnSelectionChanged;
        public event Action<int, Entry> OnEntryConfirmed;

        public MainMenuViewModel(IEnumerable<Entry> entries)
        {
            this.entries = new List<Entry>(entries);
            if (this.entries.Count == 0)
            {
                throw new ArgumentException("MainMenuViewModel requires at least one entry.", nameof(entries));
            }
        }

        /// <summary>Ensure a player is registered; returns their current selection index.</summary>
        public int EnsurePlayer(int playerIndex)
        {
            if (!playerSelections.ContainsKey(playerIndex))
            {
                playerSelections[playerIndex] = 0;
                OnSelectionChanged?.Invoke(playerIndex, 0);
            }
            return playerSelections[playerIndex];
        }

        public void MoveSelectionUp(int playerIndex)
        {
            EnsurePlayer(playerIndex);
            int next = (playerSelections[playerIndex] - 1 + entries.Count) % entries.Count;
            SetSelection(playerIndex, next);
        }

        public void MoveSelectionDown(int playerIndex)
        {
            EnsurePlayer(playerIndex);
            int next = (playerSelections[playerIndex] + 1) % entries.Count;
            SetSelection(playerIndex, next);
        }

        public void SetSelection(int playerIndex, int entryIndex)
        {
            if (entryIndex < 0 || entryIndex >= entries.Count) return;
            if (playerSelections.TryGetValue(playerIndex, out int current) && current == entryIndex)
                return;
            playerSelections[playerIndex] = entryIndex;
            OnSelectionChanged?.Invoke(playerIndex, entryIndex);
        }

        public void Confirm(int playerIndex)
        {
            int selection = EnsurePlayer(playerIndex);
            Entry entry = entries[selection];
            OnEntryConfirmed?.Invoke(playerIndex, entry);
            entry.OnSelected?.Invoke();
        }

        public bool IsEntrySelectedByAnyone(int entryIndex)
        {
            foreach (int selected in playerSelections.Values)
            {
                if (selected == entryIndex) return true;
            }
            return false;
        }
    }
}
