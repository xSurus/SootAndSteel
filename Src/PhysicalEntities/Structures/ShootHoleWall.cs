using System;
using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
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
    private readonly Vector2 dimensionsPixels;
    private bool isTop;

    public ShootHoleWall(Vector2 dimensionsPixels, Vector2 positionPixels, bool isTop)
    {
        this.dimensionsPixels = dimensionsPixels;
        CurrentHealth = MaxHealth;
        PhysicsBody = this.gameplayContext.PhysicsWorld.CreateRectangle(dimensionsPixels.X.ToMeters(),
            dimensionsPixels.Y.ToMeters(), 1f,
            positionPixels.ToMeters(), 0f, BodyType.Static);
        PhysicsBody.Tag = this;
        this.isTop = isTop;
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsBroken) return;
        float before = CurrentHealth;
        CurrentHealth = Math.Max(0f, CurrentHealth - damageAmount);
        if (before > 0f && CurrentHealth <= 0f)
        {
            gameplayContext.Events.FireWallBreached();
        }
    }

    public bool OnHit(BulletEntity bullet)
    {
        if (bullet.Owner is AbstractEnemy && !IsBroken)
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

        float previousHealth = CurrentHealth;
        CurrentHealth += HealthRestoredPerSecond * dt;

        if (CurrentHealth >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
            if (previousHealth < MaxHealth) gameplayContext.Events.FireWallRepaired();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 origin = new Vector2(dimensionsPixels.X / 2f, dimensionsPixels.Y / 2f);
        Rectangle sourceRect = new Rectangle(0, 0, (int)dimensionsPixels.X, (int)dimensionsPixels.Y);

        Color wallColor = IsBroken ? Color.DarkRed : Color.DarkSlateGray;
        Vector2 snappedPosition = new Vector2(MathF.Round(Position.X), MathF.Round(Position.Y));

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float originalSize = AssetManager.GetWallTexture("WallTileTop").Width;
        float scale = tileSize / originalSize;

        if (isTop)
        {
            Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.75f);
            spriteBatch.Draw(AssetManager.GetWallTexture("WallTileTop"), drawingPos, null, Color.White,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        else
        {
            Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1f);
            spriteBatch.Draw(AssetManager.GetWallTexture("WallTileBottom"), drawingPos, null, Color.White,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }


        // spriteBatch.Draw(
        //     texture: AssetManager.BlankTexture,
        //     position: snappedPosition,
        //     sourceRectangle: sourceRect,
        //     color: wallColor,
        //     rotation: PhysicsBody.Rotation,
        //     origin: origin,
        //     scale: 1f,
        //     effects: SpriteEffects.None,
        //     layerDepth: 0f
        // );

        if (CurrentHealth < MaxHealth)
        {
            int barWidth = (int)dimensionsPixels.X - 10;
            int barHeight = 6;
            float healthPercentage = CurrentHealth / MaxHealth;

            Rectangle bgBar = new Rectangle(
                (int)(snappedPosition.X - barWidth / 2f),
                (int)(snappedPosition.Y - barHeight / 2f),
                barWidth,
                barHeight
            );

            Rectangle fillBar = new Rectangle(
                bgBar.X,
                bgBar.Y,
                (int)MathF.Round(barWidth * healthPercentage),
                barHeight
            );

            Color healthBarColor = IsBroken ? Color.Red : Color.LimeGreen;

            spriteBatch.Draw(AssetManager.BlankTexture, bgBar, Color.Black);
            spriteBatch.Draw(AssetManager.BlankTexture, fillBar, healthBarColor);
        }
    }
}