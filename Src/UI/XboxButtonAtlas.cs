using Microsoft.Xna.Framework;

namespace Gamelab.UI;

/// <summary>Crops in <see cref="SheetFile"/> — one row of 32×32 face buttons: A, B, X, Y, …</summary>
public static class XboxButtonAtlas
{
    public const string SheetFile = "xbox_buttons_spritesheet.png";

    public const int CellSize = 32;

    public enum Face
    {
        A = 0,
        B = 1,
        X = 2,
        Y = 3,
    }

    public static Rectangle GetSourceRect(Face face) =>
        new((int)face * CellSize, 0, CellSize, CellSize);
}
