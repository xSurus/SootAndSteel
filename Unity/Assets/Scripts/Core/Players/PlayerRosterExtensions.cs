using System.Collections.Generic;
using System.Linq;
using Gamelab.Input;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Screens/PlayerInputExtensions.cs.</summary>
    public static class PlayerRosterExtensions
    {
        public static bool AnyPressedMenuConfirm(this PlayerRoster roster) =>
            roster.Slots.Select(s => s.Input).AnyPressedMenuConfirm();

        public static bool AnyPressedMenuConfirm(this IEnumerable<IInputActions> inputs) =>
            inputs.Any(i => i.IsPickupJustPressed() || i.IsStartJustPressed());
    }
}
