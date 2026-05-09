using Gamelab.Config;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Bullets;

public record struct BulletStats
{
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float Pierce { get; set; }
    public float Size { get; set; }
    public float Spread { get; set; }
    public float Lifetime { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public Color Color { get; set; }

    public BulletStats(GameplayConfig config, Vector2 position, Vector2 direction)
    {
        Speed = config.CannonProjectileSpeed;
        Damage = config.CannonProjectileDamage;
        Pierce = config.CannonProjectilePierce;
        Size = config.CannonProjectileSize;
        Spread = config.CannonProjectileSpread;
        Lifetime = config.CannonProjectileLifetime;

        Position = position;
        Direction = direction;
        Color = Color.Black;
    }
}