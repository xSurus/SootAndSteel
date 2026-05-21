using System.Collections.Generic;
using System.Linq;
using Gamelab.Players;

namespace Gamelab.Screens;

internal static class PlayerInputExtensions
{
    public static bool AnyPressedMenuConfirm(this IEnumerable<PlayerConfiguration> configs) =>
        configs.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed());
}
