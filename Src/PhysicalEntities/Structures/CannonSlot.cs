using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.Players;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Structures;

public class CannonSlot : ICannonSeat, IBulletEmitter, IHighlightable, IGrabbable
{
    private readonly ISoundService soundService;
    private readonly IBulletService bulletService;
    private readonly IVfxService vfxService;
    private readonly float arcMin;
    private readonly float arcMax;
    private readonly List<BulletItem> ammoRack = new();
    private const int MaxAmmo = 4;
    private float cooldownTimer;
    private int highlighterCount;
    private BulletRack pairedRack;
    private Vector2 barrelPivotOffset;
    private float barrelLength;
    private readonly string textureName;
    private readonly Vector2 exitOffsetMeters;

    public Vector2 DrawOffset { get; set; } = Vector2.Zero;
    public Player SeatedPlayer { get; private set; }
    public float AimAngle { get; private set; }
    public IReadOnlyList<BulletItem> AmmoRack => ammoRack;
    public Body PhysicsBody { get; }

    public Vector2 Position
    {
        get => PhysicsBody.Position.ToPixels();
        set => PhysicsBody.Position = value.ToMeters();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 center = PhysicsBody.Position.ToPixels();
        float depth = RenderUtility.CalculateDepth(center.Y);

        if (textureName == null) return;

        Texture2D tex = AssetManager.GetStationTexture(textureName);
        float scale = tileSize / (float)tex.Width * 2f;
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
        Color idleLight = new(190, 190, 190, 44);
        Color highlightLight = new(255, 255, 255, 44);
        spriteBatch.DrawWithLightBoost(tex, center + new Vector2(0, tileSize * 0.3f), null, Color.White, idleLight,
            highlightLight, 0f, origin, scale, SpriteEffects.None, depth, IsHighlighted);
    }

    public bool IsHighlighted => highlighterCount > 0;

    public void OnHighlight(Player player) { highlighterCount++; }

    public void OnHighlightRemoved(Player player)
    {
        highlighterCount = Math.Max(0, highlighterCount - 1);
    }

    public CannonSlot(Vector2 seatPositionPixels, float arcMin, float arcMax, float defaultAngle, string textureName = null, Vector2 exitOffsetPixels = default)
    {
        this.textureName = textureName;
        exitOffsetMeters = exitOffsetPixels.ToMeters();
        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        bulletService = GamelabGame.Instance.Services.GetService<IBulletService>();
        vfxService = GamelabGame.Instance.Services.GetService<IVfxService>();
        soundService.LoadSound(Sounds.CannonLoad);
        soundService.LoadSound(Sounds.CannonFire);

        this.arcMin = arcMin;
        this.arcMax = arcMax;
        AimAngle = defaultAngle;

        var ctx = GamelabGame.Instance.Services.GetService<GameplayContext>();
        float tileMeters = GamelabGame.Instance.GameplayConfig.TrainTileSize.ToMeters();
        PhysicsBody = ctx.PhysicsWorld.CreateRectangle(
            tileMeters, tileMeters, 0f, seatPositionPixels.ToMeters());
        PhysicsBody.BodyType = BodyType.Static;
        PhysicsBody.Tag = this;
    }

    public bool OnGrab(Player player, Vector2 grabPointWorldMeters)
    {
        if (SeatedPlayer != null) return false;
        SeatedPlayer = player;
        player.SeatAt(this);
        return true;
    }

    public void OnRelease(Player player)
    {
        if (SeatedPlayer != player) return;
        player.UnseatFrom(this);
        if (exitOffsetMeters != Vector2.Zero)
            player.PhysicsBody.Position += exitOffsetMeters;
        SeatedPlayer = null;
    }

    public void OnInteract(Player player)
    {
        if (ammoRack.Count == 0 && pairedRack != null && pairedRack.TryProvideItem(out Item item) && item is BulletItem bullet)
            ammoRack.Add(bullet);
        if (cooldownTimer > 0f || ammoRack.Count == 0) return;
        FireCannon();
    }

    public void SetPairedRack(BulletRack rack) => pairedRack = rack;

    public void SetBarrelParams(Vector2 pivotOffset, float barrelLengthPixels)
    {
        barrelPivotOffset = pivotOffset;
        barrelLength = barrelLengthPixels;
    }

    public bool TryLoadAmmo(Item item)
    {
        if (ammoRack.Count >= MaxAmmo || item is not BulletItem bullet) return false;
        ammoRack.Add(bullet);
        soundService.PlayOnce(Sounds.CannonLoad);
        return true;
    }

    public void Update(float dt)
    {
        cooldownTimer = MathF.Max(0f, cooldownTimer - dt);
        if (SeatedPlayer != null) UpdateAim(dt);
    }

    private void UpdateAim(float dt)
    {
        Vector2 input = SeatedPlayer.PlayerConfiguration.Input.GetMovement();
        if (input.LengthSquared() <= GamelabGame.Instance.GameplayConfig.InputMovementDeadzoneSquared) return;

        float targetAngle = MathF.Atan2(input.Y, input.X);
        float diff = MathHelper.WrapAngle(targetAngle - AimAngle);
        float maxStep = GamelabGame.Instance.GameplayConfig.CannonRotationSpeed * dt;
        AimAngle = Math.Clamp(AimAngle + Math.Clamp(diff, -maxStep, maxStep), arcMin, arcMax);
    }

    private void FireCannon()
    {
        var config = GamelabGame.Instance.GameplayConfig;
        BulletItem bullet = ammoRack[0];
        ammoRack.RemoveAt(0);

        Vector2 direction = new(MathF.Cos(AimAngle), MathF.Sin(AimAngle));
        Vector2 seatPositionPixels = PhysicsBody.Position.ToPixels();
        Vector2 barrelPivot = seatPositionPixels + barrelPivotOffset;
        Vector2 barrelTip = barrelPivot + direction * barrelLength;

        bulletService.EmitBullet(bullet, barrelTip, direction, this);
        vfxService.EmitBurst(ParticleFactory.CreateCannonMuzzleFlash(barrelTip, direction));

        cooldownTimer = config.CannonCooldown;
        soundService.PlayOnce(Sounds.CannonFire);
    }
}
