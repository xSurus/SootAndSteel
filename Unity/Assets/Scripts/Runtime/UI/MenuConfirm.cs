using System;
using System.Collections.Generic;
using Gamelab.Input;

namespace Gamelab.UI.Runtime
{
    /// <summary>Src AnyPressedMenuConfirm: any player pressed Pickup or Start this frame.</summary>
    internal static class MenuConfirm
    {
        public static bool Any(Func<IReadOnlyList<IInputActions>> players)
        {
            var list = players?.Invoke();
            if (list == null) return false;
            foreach (var input in list)
                if (input.IsPickupJustPressed() || input.IsStartJustPressed()) return true;
            return false;
        }
    }
}
