using System.Collections.Generic;
using Gamelab.Input;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Players/PlayerManager.cs.</summary>
    public class PlayerRoster
    {
        public const int MaxPlayers = 4;

        private readonly List<PlayerSlot> slots = new List<PlayerSlot>();
        public IReadOnlyList<PlayerSlot> Slots => slots;

        public bool JoinPlayer(IInputActions input)
        {
            if (slots.Count >= MaxPlayers) return false;

            slots.Add(new PlayerSlot(slots.Count, input));
            return true;
        }

        public void Reset()
        {
            slots.Clear();
        }
    }
}
