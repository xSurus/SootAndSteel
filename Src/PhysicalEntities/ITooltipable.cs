using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities;

public interface ITooltipable
{
    Vector2 Position { get; }
    string GetTooltipTitle();
    string GetTooltipDescription();
    Color GetTooltipTextColor();
    bool IsTooltipVisible { get; }
}