using Microsoft.Xna.Framework;

namespace Gamelab.UI;
public static class XboxButtonAtlas
{
    public const string SheetFile = "xbox_buttons_spritesheet.png";

    public const int CellSize = 32;

    /// <summary>Spritesheet column index × <see cref="CellSize"/> for <c>TextureLeft</c>; Y is always 0.</summary>
    public enum Face
    {
        A = 0,
        B = 1,
        X = 2,
        Y = 3,
        Start = 4,
        Back = 5,
        Lb = 6,
        Rb = 7,
        Lt = 8,
        Rt = 9,
    }


    public static Rectangle GetSourceRect(Face face) =>
        new((int)face * CellSize, 0, CellSize, CellSize);
}
