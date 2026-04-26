using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface ITooltipable
{
    Vector2 Position { get; }
    string GetTitle();
    string GetDescription();
    bool IsVisible { get; }
    string CategoryName => null;
    string FunctionalityName => null;
    Rectangle? IconSourceRect => null;
    int? Cost => null;
}
