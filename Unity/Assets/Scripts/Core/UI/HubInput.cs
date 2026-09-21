using System;
using System.Collections.Generic;
using Gamelab.Players;

namespace Gamelab.UI
{
    /// <summary>Polls all player slots for the hub. The caller skips Tick while paused, as Src does.</summary>
    public class HubInput
    {
        private readonly Func<IReadOnlyList<PlayerSlot>> players;
        private readonly List<int> joined = new List<int>();

        public HubInput(Func<IReadOnlyList<PlayerSlot>> players)
        {
            this.players = players;
        }

        public void Tick(float dt, HubDepartureModel departure, CraftingHelpModel help, int pendingOffBoardCount)
        {
            IReadOnlyList<PlayerSlot> list = players();
            bool back = false, interact = false, grab = false;
            joined.Clear();
            foreach (PlayerSlot slot in list)
            {
                back |= slot.Input.IsBackButtonJustPressed();
                interact |= slot.Input.IsInteractJustPressed();
                grab |= slot.Input.IsGrabJustPressed();
                joined.Add(slot.PlayerIndex);
            }

            if (back) help.Toggle();
            departure.Update(dt, joined, pendingOffBoardCount, interact, grab);
        }
    }
}
