using System;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Items;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Players;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Cannon;

public class CannonStation : AbstractStation
{
    private readonly GameplayConfig config;
    private float cooldownTimer;

    public CannonAimingBar AimingBar { get; private set; }

    public CannonStation(Vector2 position)
        : base("Cannon", Color.DarkRed, position)
    {
        config = GamelabGame.Instance.GameplayConfig;
        cooldownTimer = 0f;
        AimingBar = new CannonAimingBar(PhysicsBody, position.ToMeters());
    }

    public override void Update(float dt)
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= dt;
        }
    }

    public override void OnInteract(Player interactingPlayer)
    {
        if (cooldownTimer > 0f || HeldItem == null)
        {
            return;
        }

        Vector2 direction = new Vector2(
            (float)Math.Cos(AimingBar.PhysicsBody.Rotation),
            (float)Math.Sin(AimingBar.PhysicsBody.Rotation)
        );

        FireCannon(direction, HeldItem);
        HeldItem = null;
    }

    public override void OnPickup(Player interactingPlayer)
    {
        if (HeldItem != null || interactingPlayer.HeldItem == null)
        {
            return;
        }

        if (!IsBullet(interactingPlayer.HeldItem))
        {
            return;
        }

        HeldItem = interactingPlayer.HeldItem;
        interactingPlayer.HeldItem = null;
    }

    private void FireCannon(Vector2 direction, Item ammo)
    {
        Vector2 cannonPosition = DrawPosition + new Vector2(config.TrainTileSize / 2f);
        float barrelLength = config.TrainTileSize * 0.5f;
        Vector2 projectileSpawn = cannonPosition + direction * barrelLength;
        float damage = GetBulletDamage(ammo);

        CannonProjectile projectile = new CannonProjectile(
            gameplayContext.PhysicsWorld,
            projectileSpawn,
            direction * config.CannonProjectileSpeed,
            damage,
            config.CannonProjectileLifetime,
            config.CannonProjectileSize
        );

        gameplayContext.Events.FireCannonProjectile(projectile);
        GamelabGame.Instance.Services.GetService<IVfxService>()
            .EmitBurst(ParticleFactory.CreateCannonMuzzleFlash(projectileSpawn, direction));
        cooldownTimer = config.CannonCooldown;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (AimingBar?.PhysicsBody == null)
        {
            base.Draw(spriteBatch);
            return;
        }

        Vector2 direction = new Vector2(
            (float)Math.Cos(AimingBar.PhysicsBody.Rotation),
            (float)Math.Sin(AimingBar.PhysicsBody.Rotation)
        );
        float lineLength = config.TrainTileSize * 4f;
        Vector2 lineEnd = Position + direction * lineLength;

        base.Draw(spriteBatch);
        DrawAimLine(spriteBatch, Position, lineEnd);
        AimingBar?.Draw(spriteBatch);
    }

    private static void DrawAimLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            AssetManager.BlankTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 2),
            null,
            Color.White,
            angle,
            new Vector2(0f, 1f),
            SpriteEffects.None,
            0f
        );
    }

    private static bool IsBullet(Item item)
    {
        return item.Definition is BulletDefinition;
    }

    private float GetBulletDamage(Item ammo)
    {
        if (ammo.Definition is BulletDefinition bulletDefinition)
        {
            return bulletDefinition.Damage;
        }

        return config.CannonProjectileDamage;
    }
}