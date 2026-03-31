using Microsoft.Xna.Framework;

namespace Gamelab.Items;

public class ItemDefinition(string id, Color color)
{
    public string Id { get; } = id;
    public Color Color { get; } = color;
}