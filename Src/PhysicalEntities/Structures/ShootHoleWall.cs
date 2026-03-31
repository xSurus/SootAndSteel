using System;
using Gamelab.Assets;
using Gamelab.Entities;
using Gamelab.Events;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Projectiles;
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
    private readonly GameEvents gameEvents;

    public ShootHoleWall(World physicsWorld, GameEvents gameEvents, Vector2 dimensionsPixels, Vector2 positionPixels)
    {
        this.dimensionsPixels = dimensionsPixels;
        this.gameEvents = gameEvents;
        CurrentHealth = MaxHealth;
        PhysicsBody = physicsWorld.CreateRectangle(dimensionsPixels.X.ToMeters(), dimensionsPixels.Y.ToMeters(), 1f,
            positionPixels.ToMeters(), 0f, BodyType.Static);
        PhysicsBody.Tag = this;
    }

    public void AttachPhysics(Body body)
    {
        PhysicsBody = body;
        PhysicsBody.Tag = this;
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsBroken) return;
        bool wasBroken = IsBroken;
        CurrentHealth = Math.Max(0f, CurrentHealth - damageAmount);
        if (!wasBroken && IsBroken) gameEvents.FireWallBreached();
    }

    public void OnHit(AbstractProjectile projectile)
    {
        if (projectile is not EnemyProjectile || IsBroken)
        {
            return;
        }

        bool wasBroken = IsBroken;
        TakeDamage(projectile.Damage);

        if (!wasBroken && IsBroken && gameEvents != null)
        {
            gameEvents.FireWallBreached();
        }

        projectile.Deactivate();
    }

    public void OnPickup(Player interactingPlayer, GameplayContext context)
    {
        // TODO Only for debugging until damage from enemies is implemented
        TakeDamage(10);
    }

    public void OnInteractHeld(Player interactingPlayer, GameplayContext context, float dt)
    {
        if (CurrentHealth >= MaxHealth) return;

        float previousHealth = CurrentHealth;
        CurrentHealth += HealthRestoredPerSecond * dt;

        if (CurrentHealth >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
            if (previousHealth < MaxHealth) gameEvents.FireWallRepaired();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 topLeft = Position - (dimensionsPixels / 2f);

        Rectangle rect = new Rectangle(
            (int)topLeft.X,
            (int)topLeft.Y,
            (int)dimensionsPixels.X,
            (int)dimensionsPixels.Y
        );
        Color wallColor = IsBroken ? Color.DarkRed : Color.DarkSlateGray;
        spriteBatch.Draw(AssetManager.BlankTexture, rect, wallColor);

        if (CurrentHealth < MaxHealth)
        {
            int barWidth = (int)dimensionsPixels.X - 10;
            float healthPercentage = CurrentHealth / MaxHealth;

            // Background of the bar (black)
            Rectangle bgBar = new Rectangle(rect.X + 5, rect.Y + rect.Height / 2 - 3, barWidth, 6);

            // The filled portion of the bar
            Rectangle fillBar = new Rectangle(rect.X + 5, rect.Y + rect.Height / 2 - 3,
                (int)(barWidth * healthPercentage), 6);

            // Change color based on status (Red if completely broken, LimeGreen if just damaged)
            Color barColor = IsBroken ? Color.Red : Color.LimeGreen;

            spriteBatch.Draw(AssetManager.BlankTexture, bgBar, Color.Black);
            spriteBatch.Draw(AssetManager.BlankTexture, fillBar, barColor);
        }
    }
}