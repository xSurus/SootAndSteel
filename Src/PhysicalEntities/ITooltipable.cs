using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities;

public interface ITooltipable
{
    Vector2 WorldPosition { get; }
    string GetTitle();
    string GetDescription();
    Color GetTextColor();
    bool IsActive { get; }
}