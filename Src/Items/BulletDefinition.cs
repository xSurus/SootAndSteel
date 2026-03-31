using Microsoft.Xna.Framework;

namespace Gamelab.Items;

public class BulletDefinition(string id, Color color, float damage) : ItemDefinition(id, color)
{
    public float Damage { get; } = damage;
}