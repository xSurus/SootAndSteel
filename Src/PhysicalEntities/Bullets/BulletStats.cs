using Gamelab.Config;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Bullets;

public class BulletStats(GameplayConfig config, Vector2 position, Vector2 direction)
{
    public float Speed { get; set; } = config.CannonProjectileSpeed;
    public float Damage { get; set; } = config.CannonProjectileDamage;
    public float Pierce { get; set; } = config.CannonProjectilePierce;
    public float Size { get; set; } = config.CannonProjectileSize;
    public float Spread { get; set; } = config.CannonProjectileSpread;

    public float Lifetime { get; set; } = config.CannonProjectileLifetime;
    public Vector2 Position { get; set; } = position;
    public Vector2 Direction { get; set; } = direction;
    public Color Color { get; set; } = Color.Cyan;
}