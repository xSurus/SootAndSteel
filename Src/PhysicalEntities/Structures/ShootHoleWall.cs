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
    private int variation;

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

        variation = Random.Shared.Next(1, 4);

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
    }

    public void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (CurrentHealth >= MaxHealth) return;

        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + HealthRestoredPerSecond * dt);

        if (CurrentHealth >= MaxHealth && isBreached)
        {
            isBreached = false;
            variation = Random.Shared.Next(1, 4);
            gameplayContext.Events.FireWallRepaired();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float damagePercent = (1f - CurrentHealth / MaxHealth) * 100f;
        
        string textureName;

        if (isTop)
        {
            if (isBreached)
            {
                textureName = $"WallTileTopBroken4_Variation{variation}";
            }
            else if (damagePercent >= 75f)
            {
                textureName = $"WallTileTopBroken3_Variation{variation}";
            }
            else if (damagePercent >= 50f)
            {
                textureName = $"WallTileTopBroken2_Variation{variation}";
            }
            else if (damagePercent >= 25f)
            {
                textureName = $"WallTileTopBroken1_Variation{variation}";
            }
            else
            {
                textureName = "WallTileTop";
            }
        }
        else
        {
            if (isBreached)
            {
                textureName = "WallTileBottomBroken3";
            }
            else if (damagePercent >= 66f)
            {
                textureName = "WallTileBottomBroken2";
            }
            else if (damagePercent >= 33f)
            {
                textureName = "WallTileBottomBroken1";
            }
            else
            {
                textureName = "WallTileBottom";
            }
        }

        Texture2D wallTex = AssetManager.GetWallTexture(textureName);

        float scale = tileSize / (float)AssetManager.GetWallTexture(textureName).Width;
        Vector2 origin = new Vector2(wallTex.Width / 2f, wallTex.Height);

        Vector2 feetPosition;
        if (isTop)
        {
            feetPosition = Position + new Vector2(0, dimensionsPixels.Y / 2f);
        }
        else
        {
            feetPosition = Position + new Vector2(0, -dimensionsPixels.Y / 2f);
        }

        float depth = RenderUtility.CalculateDepth(feetPosition.Y);

        spriteBatch.DrawWithHighlight(wallTex, feetPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None,
            depth + RenderUtility.Eps, isHighlighted: IsHighlighted);
    }

    public void DrawLightBatch(SpriteBatch spriteBatch)
    {
        float damagePercent = (1f - CurrentHealth / MaxHealth) * 100f;
        if (damagePercent <= 0f) return;

        Texture2D lightTex = AssetManager.GetDecorationTexture("BlueLight");
        Vector2 lightOrigin = new Vector2(lightTex.Width / 2f, lightTex.Height / 2f);
        float lightScale = dimensionsPixels.X * 3f / lightTex.Width;
        float intensity = Math.Clamp(damagePercent / 100f, 0f, 1f);

        Vector2 lightPos = Position + new Vector2(0, -dimensionsPixels.Y);

        spriteBatch.Draw(lightTex, lightPos, null, Color.White * (intensity * 0.5f),
            0f, lightOrigin, lightScale, SpriteEffects.None, 0f);

        if (damagePercent >= 66f)
        {
            float bigIntensity = Math.Clamp((damagePercent - 66f) / 34f, 0f, 1f);
            spriteBatch.Draw(lightTex, lightPos, null, Color.White * (bigIntensity * 0.5f),
                0f, lightOrigin, 1.5f * lightScale, SpriteEffects.None, 0f);
        }
    }
}