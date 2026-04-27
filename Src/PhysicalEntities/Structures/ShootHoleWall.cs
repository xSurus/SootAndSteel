using System;
using Gamelab.Assets;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Structures;

public class ShootHoleWall : AbstractPhysicalEntity, IInteractable, IDamageable, IPickable
{
    public float MaxHealth => GamelabGame.Instance.GameplayConfig.WallMaxHealth;

    private float HealthRestoredPerSecond => GamelabGame.Instance.GameplayConfig.WallHealthRestoredPerSecond;
    public float CurrentHealth { get; private set; }
    public bool IsBroken => CurrentHealth <= 0f;
    private bool isBreached;
    private readonly Vector2 dimensionsPixels;
    private bool isTop;

    private ISoundService soundService;

    public ShootHoleWall(Vector2 dimensionsPixels, Vector2 positionPixels, bool isTop)
    {
        this.dimensionsPixels = dimensionsPixels;
        CurrentHealth = MaxHealth;
        PhysicsBody = this.gameplayContext.PhysicsWorld.CreateRectangle(dimensionsPixels.X.ToMeters(),
            dimensionsPixels.Y.ToMeters(), 1f,
            positionPixels.ToMeters(), 0f, BodyType.Static);
        PhysicsBody.Tag = this;
        this.isTop = isTop;

        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.WallHit);
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsBroken) return;
        CurrentHealth = Math.Max(0f, CurrentHealth - damageAmount);
        if (IsBroken && !isBreached)
        {
            isBreached = true;
            gameplayContext.Events.FireWallBreached();
        }

        soundService.PlayOnce(Sounds.WallHit);
    }

    public bool OnHit(BulletEntity bullet)
    {
        if (bullet.InitialShooter is AbstractEnemy && !IsBroken)
        {
            TakeDamage(bullet.Stats.Damage);
            return true;
        }

        return false;
    }

    public void OnPickup(Player interactingPlayer)
    {
        // TODO Only for debugging until damage from enemies is implemented
        TakeDamage(10);
    }

    public void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (CurrentHealth >= MaxHealth) return;

        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + HealthRestoredPerSecond * dt);

        if (CurrentHealth >= MaxHealth && isBreached)
        {
            isBreached = false;
            gameplayContext.Events.FireWallRepaired();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Texture2D wallTex =
            isTop ? AssetManager.GetWallTexture("WallTileTop") : AssetManager.GetWallTexture("WallTileBottom");

        float scale = tileSize / (float)AssetManager.GetWallTexture("WallTileTop").Width;
        Vector2 origin = new Vector2(wallTex.Width / 2f, wallTex.Height);

        Vector2 feetPosition;
        if (isTop)
        {
            feetPosition = Position + new Vector2(0, dimensionsPixels.Y / 2f);
        }
        else
        {
            float visualHeight = wallTex.Height * scale;
            feetPosition = Position + new Vector2(0, -dimensionsPixels.Y / 2f);
        }

        float depth = RenderUtility.CalculateDepth(feetPosition.Y);

        spriteBatch.Draw(wallTex, feetPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None,
            depth + RenderUtility.Eps);

        if (CurrentHealth < MaxHealth)
        {
            int barWidth = (int)dimensionsPixels.X - 10;
            int barHeight = 6;
            float healthPercentage = CurrentHealth / MaxHealth;

            Vector2 barPos = Position + new Vector2(-barWidth / 2f, -dimensionsPixels.Y / 2f - 15f);

            Rectangle bgBar = new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight);
            Rectangle fillBar =
                new Rectangle(bgBar.X, bgBar.Y, (int)MathF.Round(barWidth * healthPercentage), barHeight);
            Color healthBarColor = IsBroken ? Color.Red : Color.LimeGreen;

            spriteBatch.Draw(AssetManager.BlankTexture, bgBar, null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None,
                depth + 2 * RenderUtility.Eps);
            spriteBatch.Draw(AssetManager.BlankTexture, fillBar, null, healthBarColor, 0f, Vector2.Zero,
                SpriteEffects.None, depth + 3 * RenderUtility.Eps);
        }
    }
}