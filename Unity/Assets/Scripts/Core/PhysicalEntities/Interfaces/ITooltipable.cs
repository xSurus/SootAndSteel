using System.Numerics;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface ITooltipable
    {
        Vector2 Position { get; }
        string GetTitle();
        string GetDescription();
        bool IsVisible { get; }
        string CategoryName => null;
        string FunctionalityName => null;
        SpriteRect? IconSourceRect => null;
        int? Cost => null;
    }
}
