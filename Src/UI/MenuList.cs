using System;
using System.Collections.Generic;
using Gamelab.Players;

namespace Gamelab.UI;

public class MenuList
{
    public class MenuEntry(string label, Action onSelected)
    {
        public string Label { get; } = label;
        public Action OnSelected { get; } = onSelected;
    }

    private readonly List<MenuEntry> entries;

    public Dictionary<int, int> PlayerSelections { get; } = new();

    public MenuList(IEnumerable<MenuEntry> entries)
    {
        this.entries = [.. entries];
        if (this.entries.Count == 0)
        {
            throw new ArgumentException("MenuList requires at least one menu entry.", nameof(entries));
        }
    }

    public void Update(IEnumerable<PlayerConfiguration> players)
    {
        foreach (PlayerConfiguration player in players)
        {
            int pIndex = player.PlayerIndex;
            var input = player.Input;

            if (!PlayerSelections.ContainsKey(pIndex))
            {
                PlayerSelections[pIndex] = 0;
            }

            if (input.IsUpJustPressed())
            {
                PlayerSelections[pIndex] = (PlayerSelections[pIndex] - 1 + entries.Count) % entries.Count;
            }
            else if (input.IsDownJustPressed())
            {
                PlayerSelections[pIndex] = (PlayerSelections[pIndex] + 1) % entries.Count;
            }

            if (input.IsPickupJustPressed())
            {
                entries[PlayerSelections[pIndex]].OnSelected.Invoke();
            }
        }
    }
}