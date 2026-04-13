using Gamelab.Enemies;
using Gamelab.Entities;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public class SpeedLever(Vector2 position)
    : AbstractStation("SpeedLever", Color.LightGreen, position), IRepairable
{
    private float RepairPerSecond => GamelabGame.Instance.GameplayConfig.RepairableSpeedLeverRepairPerSecond;
    private readonly RepairState repairState = new(GamelabGame.Instance.GameplayConfig.RepairableSpeedLeverMaxHealth);

    public bool IsBroken => repairState.IsBroken;
    public float CurrentHealth => repairState.CurrentHealth;
    public float MaxHealth => repairState.MaxHealth;

    public override void OnInteract(Player interactingPlayer)
    {
        if (IsBroken)
        {
            return;
        }

        if (gameplayContext.State.VictoryLapActive)
        {
            return;
        }

        if (!gameplayContext.State.IsCoalOvenBurning)
        {
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(gameplayContext.State.CurrentSpeed);
        int nextIndex = (currentIndex + 1) % allSpeeds.Count;

        gameplayContext.State.CurrentSpeed = allSpeeds[nextIndex];
    }

    public override void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (!IsBroken)
        {
            return;
        }

        Repair(RepairPerSecond * dt);
    }

    public void Repair(float amount)
    {
        repairState.Repair(amount);
    }

    public void TakeDamage(float damageAmount)
    {
        repairState.ApplyDamage(damageAmount);
    }

    public bool OnHit(BulletEntity bullet)
    {
        if (bullet.Owner is not AbstractEnemy || IsBroken)
        {
            return false;
        }
        TakeDamage(bullet.Stats.Damage);
        return true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Color previousColor = DisplayColor;
        if (IsBroken)
        {
            DisplayColor = Color.DarkOliveGreen;
        }

        base.Draw(spriteBatch);
        DisplayColor = previousColor;
        DrawHealthBar(spriteBatch);
    }

    private void DrawHealthBar(SpriteBatch spriteBatch)
    {
        if (CurrentHealth >= MaxHealth)
        {
            return;
        }

        int width = 48;
        int height = 6;
        Rectangle bg = new((int)(Position.X - width / 2f), (int)(Position.Y + 28f), width, height);
        Rectangle fill = new(bg.X, bg.Y, (int)(width * (CurrentHealth / MaxHealth)), height);
        spriteBatch.Draw(Assets.AssetManager.BlankTexture, bg, Color.Black);
        spriteBatch.Draw(Assets.AssetManager.BlankTexture, fill, IsBroken ? Color.OrangeRed : Color.LimeGreen);
    }
}
