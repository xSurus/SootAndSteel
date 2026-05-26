using System;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Cannon;

public class CannonStation : AbstractStation, IBulletEmitter, IInteractable, ICannonSeat
{
    private readonly GameplayConfig config;
    private float cooldownTimer;
    public Player SeatedPlayer { get; private set; }
    public SeatPosition SeatPosition => SeatPosition.Bottom;
    public Vector2 DrawOffset => new(0, 0);

    public CannonStation(Vector2 position)
        : base(StationIds.Cannon, position)
    {
        config = GamelabGame.Instance.GameplayConfig;
        cooldownTimer = 0f;
        soundService.LoadSound(Sounds.CannonLoad);
        soundService.LoadSound(Sounds.CannonFire);
    }

    public override void Update(float dt)
    {
        base.Update(dt);
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

        Vector2 ejectPosition = FindEjectPosition();
        SeatedPlayer.PhysicsBody.Position = ejectPosition.ToMeters();

        SeatedPlayer.UnseatFrom(this);
        SeatedPlayer = null;
    }

    private Vector2 FindEjectPosition()
    {
        var map = gameplayContext.Map;
        int tileSize = map.TileSize;

        Vector2[] offsets =
        {
            new Vector2(-tileSize, 0),
            new Vector2(tileSize, 0),
            new Vector2(0, -tileSize),
            new Vector2(0, tileSize),
            new Vector2(-tileSize, -tileSize),
            new Vector2(tileSize, -tileSize),
            new Vector2(-tileSize, tileSize),
            new Vector2(tileSize, tileSize),
        };

        foreach (Vector2 offset in offsets)
        {
            Vector2 candidate = Position + offset;
            if (IsTileFree(candidate, map))
                return candidate;
        }

        return Position;
    }

    private bool IsTileFree(Vector2 pixelPosition, TrainMap map)
    {
        Point candidateTile = map.GetTileIndexFromPixels(pixelPosition);

        if (candidateTile.X < 0 || candidateTile.X >= map.Width ||
            candidateTile.Y < 0 || candidateTile.Y >= map.Height)
            return false;

        foreach (var entity in map.MapObjects)
        {
            if (entity is not AbstractStation station) continue;
            if (ReferenceEquals(station, this)) continue;

            Point stationTile = map.GetTileIndexFromPixels(station.Position);
            if (stationTile == candidateTile)
                return false;
        }

        return true;
    }

    public void OnInteract(Player interactingPlayer)
    {
        if (cooldownTimer > 0f)
        {
            return;
        }

        ReloadCannon();
        FireCannon();
    }

    public Vector2 AimDirection => new(
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

    private void ReloadCannon()
    {
        foreach (GridDirection dir in Enum.GetValues(typeof(GridDirection)))
        {
            AbstractStation neighbor = gameplayContext.Map.GetAdjacentStation(Position, dir);

            if (neighbor is BulletRack provider)
            {
                if (provider.TryProvideItem(out Item bulletToFire))
                {
                    soundService.PlayOnce(Sounds.CannonLoad);
                    HeldItem = bulletToFire;
                }
            }
        }
    }

    public void FireCannon()
    {
        if (HeldItem == null)
        {
            return;
        }

        Vector2 direction = AimDirection;
        Vector2 cannonPosition = DrawPosition + new Vector2(config.TrainTileSize / 2f);
        float barrelLength = config.TrainTileSize * 0.5f;
        Vector2 position = cannonPosition + direction * barrelLength;

        direction.Normalize();
        GamelabGame.Instance.Services.GetService<IBulletService>()
            .EmitBullet((BulletItem)HeldItem, position, direction, this);
        GamelabGame.Instance.Services.GetService<IVfxService>()
            .EmitBurst(ParticleFactory.CreateCannonMuzzleFlash(position, direction));
        cooldownTimer = config.CannonCooldown;
        soundService.PlayOnce(Sounds.CannonFire);
        HeldItem = null;
        gameplayContext.Events.FireCannonFired();
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
}