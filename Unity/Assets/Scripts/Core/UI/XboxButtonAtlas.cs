using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.UI
{
    public static class XboxButtonAtlas
    {
        public const string SheetFile = "xbox_buttons_spritesheet.png";

        public const int CellSize = 32;

        // Face value times CellSize is the sheet X; Y is always 0.
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

        public static SpriteRect GetSourceRect(Face face)
        {
            return new SpriteRect((int)face * CellSize, 0, CellSize, CellSize);
        }
    }
}
