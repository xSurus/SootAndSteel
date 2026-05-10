using Gamelab.Components;
using Microsoft.Xna.Framework;

namespace Gamelab.UI;

/// <summary>
/// Sets a <see cref="ButtonWithIcon"/> sprite to a glyph crop from <see cref="XboxButtonAtlas"/> (spritesheet row).
/// Shared by world tooltips and dialogue prompts.
/// </summary>
internal static class XboxButtonGlyphs
{
    internal static void ApplyFaceButton(ButtonWithIcon button, XboxButtonAtlas.Face face)
    {
        Rectangle rect = XboxButtonAtlas.GetSourceRect(face);
        var sprite = button.SpriteInstance;
        sprite.SourceFileName = XboxButtonAtlas.SheetFile;
        sprite.TextureAddress = Gum.Managers.TextureAddress.Custom;
        sprite.TextureLeft = rect.X;
        sprite.TextureTop = rect.Y;
        sprite.TextureWidth = rect.Width;
        sprite.TextureHeight = rect.Height;
    }
}