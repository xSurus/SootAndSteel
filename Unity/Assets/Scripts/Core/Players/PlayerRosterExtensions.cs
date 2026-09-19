using System.Linq;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Screens/PlayerInputExtensions.cs.</summary>
    public static class PlayerRosterExtensions
    {
        public static bool AnyPressedMenuConfirm(this PlayerRoster roster) =>
            roster.Slots.Any(s => s.Input.IsPickupJustPressed() || s.Input.IsStartJustPressed());
    }
}
