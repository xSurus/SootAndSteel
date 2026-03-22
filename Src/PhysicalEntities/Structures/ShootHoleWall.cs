using Gamelab.Assets;
using Gamelab.Entities;
using Gamelab.Map.Train.State;
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

    public ShootHoleWall(Vector2 dimensionsPixels)
    {
        this.dimensionsPixels = dimensionsPixels;
        CurrentHealth = MaxHealth;
    }

    public void AttachPhysics(Body body)
    {
        PhysicsBody = body;
        PhysicsBody.Tag = this;
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsBroken) return;

        CurrentHealth -= damageAmount;
    }

    public void OnPickup(Player interactingPlayer, TrainContext context)
    {
        // TODO Only for debugging until damage from enemies is implemented
        TakeDamage(10);
    }

    public void OnInteractHeld(Player interactingPlayer, TrainContext context, float dt)
    {
        if (CurrentHealth >= MaxHealth) return;

        CurrentHealth += HealthRestoredPerSecond * dt;

        if (CurrentHealth >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 centerPixels = PhysicsBody.Position.ToPixels();
        Vector2 topLeft = centerPixels - (dimensionsPixels / 2f);
        Rectangle rect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)dimensionsPixels.X,
            (int)dimensionsPixels.Y);
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