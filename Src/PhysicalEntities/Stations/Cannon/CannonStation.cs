using System;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.Players;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Cannon;

public class CannonStation : AbstractStation, IBulletEmitter
{
    private readonly GameplayConfig config;
    private float cooldownTimer;
    
    private ISoundService soundService;

    public Player SeatedPlayer { get; private set; }

    public CannonStation(Vector2 position)
        : base(StationIds.Cannon, position)
    {
        config = GamelabGame.Instance.GameplayConfig;
        cooldownTimer = 0f;
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.CannonLoad);
        soundService.LoadSound(Sounds.CannonFire);
    }

    public override void Update(float dt)
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= dt;
        }

        UpdateAim(dt);
    }

    public override bool OnGrab(Player interactingPlayer, Vector2 grabPointWorldMeters)
    {
        if (SeatedPlayer != null) return false;
        SeatedPlayer = interactingPlayer;
        interactingPlayer.SeatAt(this);
        return true;
    }

    public override void OnRelease(Player interactingPlayer)
    {
        if (SeatedPlayer != interactingPlayer) return;
        SeatedPlayer.UnseatFrom(this);
        SeatedPlayer = null;
    }

    public override void OnInteract(Player interactingPlayer)
    {
        if (cooldownTimer > 0f || HeldItem == null)
        {
            return;
        }

        Vector2 direction = AimDirection;
        FireCannon(direction, (BulletItem)HeldItem);
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
        soundService.PlayOnce(Sounds.CannonLoad);
    }

    private Vector2 AimDirection => new(
        (float)Math.Cos(PhysicsBody.Rotation),
        (float)Math.Sin(PhysicsBody.Rotation)
    );

    private void UpdateAim(float dt)
    {
        if (SeatedPlayer == null) return;

        Vector2 input = SeatedPlayer.PlayerConfiguration.Input.GetMovement();
        if (input.LengthSquared() <= GamelabGame.Instance.GameplayConfig.InputMovementDeadzoneSquared)
        {
            return;
        }

        float targetAngle = (float)Math.Atan2(input.Y, input.X);
        float currentAngle = PhysicsBody.Rotation;
        float diff = MathHelper.WrapAngle(targetAngle - currentAngle);
        float maxStep = config.CannonRotationSpeed * dt;
        float step = Math.Clamp(diff, -maxStep, maxStep);
        PhysicsBody.Rotation = currentAngle + step;
    }

    private void FireCannon(Vector2 direction, BulletItem ammo)
    {
        Vector2 cannonPosition = DrawPosition + new Vector2(config.TrainTileSize / 2f);
        float barrelLength = config.TrainTileSize * 0.5f;
        Vector2 position = cannonPosition + direction * barrelLength;

        direction.Normalize();
        GamelabGame.Instance.Services.GetService<IBulletService>().EmitBullet(ammo, position, direction, this);
        GamelabGame.Instance.Services.GetService<IVfxService>()
            .EmitBurst(ParticleFactory.CreateCannonMuzzleFlash(position, direction));
        cooldownTimer = config.CannonCooldown;
        
        soundService.PlayOnce(Sounds.CannonFire);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        float lineLength = config.TrainTileSize * 4f;
        Vector2 lineEnd = Position + AimDirection * lineLength;
        Color lineColor = SeatedPlayer != null ? Color.OrangeRed : Color.White;
        DrawAimLine(spriteBatch, Position, lineEnd, lineColor);
    }

    private static void DrawAimLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            AssetManager.BlankTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 2),
            null,
            color,
            angle,
            new Vector2(0f, 1f),
            SpriteEffects.None,
            RenderUtility.TopEntityLayer
        );
    }

    private static bool IsBullet(Item item)
    {
        return item.Definition.Id == "Bullet" && ((BulletItem)item).Type == EComponentType.Bullet;
    }
}
