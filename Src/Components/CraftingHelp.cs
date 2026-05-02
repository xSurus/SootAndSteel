using RenderingLibrary.Graphics;

namespace Gamelab.Components
{
    partial class CraftingHelp
    {
        partial void CustomInitialize()
        {
            if (ButtonWithIconInstance?.SpriteInstance is not { } icon)
                return;

            // Rotate around the center so the 180° flip does not shift the art.
            icon.XOrigin = HorizontalAlignment.Center;
            icon.YOrigin = VerticalAlignment.Center;
            icon.Rotation = 180f;
        }
    }
}
