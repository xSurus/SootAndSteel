using System;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Players;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public class CoalOven : AbstractStation
{
    private readonly float maxFuel;
    private float currentFuel;
    private float BurnRate => GamelabGame.Instance.GameplayConfig.CoalOvenBurnRate;
    private float RefuelAmount => GamelabGame.Instance.GameplayConfig.CoalOvenRefuelAmount;
    private float LowFuelThreshold => GamelabGame.Instance.GameplayConfig.CoalOvenLowFuelThreshold;
    private ParticleEmitter smokeEmitter;

    public CoalOven(Vector2 position)
        : base("CoalOven", Color.DarkRed, position)
    {
        maxFuel = GamelabGame.Instance.GameplayConfig.CoalOvenMaxFuel;
        currentFuel = maxFuel;
        smokeEmitter = ParticleFactory.CreateOvenSmoke(Position - new Vector2(0, 10f));
        GamelabGame.Instance.Services.GetService<IVfxService>()?.AddContinuous(smokeEmitter);
    }

    public override void Update(float dt)
    {
        if (currentFuel > 0)
        {
            gameplayContext.State.IsCoalOvenBurning = true;
            float speedMultiplier = GetInterpolatedBurnMultiplier(gameplayContext.State.actualSpeed);
            currentFuel -= BurnRate * speedMultiplier * dt;
            smokeEmitter.AutoTrigger = true;
        }
        else
        {
            gameplayContext.State.IsCoalOvenBurning = false;
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
            smokeEmitter.AutoTrigger = false;
        }
    }

    private float GetInterpolatedBurnMultiplier(float actualSpeed)
    {
        var allSpeeds = TrainSpeedSetting.All;
        if (allSpeeds == null || allSpeeds.Count == 0) return 1f;

        // check for lowest and highest speeds
        if (actualSpeed <= allSpeeds[0].TargetSpeed) return allSpeeds[0].BurnMultiplier;
        if (actualSpeed >= allSpeeds[^1].TargetSpeed) return allSpeeds[^1].BurnMultiplier;

        for (int i = 0; i < allSpeeds.Count - 1; i++)
        {
            var lowerBound = allSpeeds[i];
            var upperBound = allSpeeds[i + 1];

            if (actualSpeed >= lowerBound.TargetSpeed && actualSpeed <= upperBound.TargetSpeed)
            {
                if (Math.Abs(upperBound.TargetSpeed - lowerBound.TargetSpeed) < 0.001) return lowerBound.BurnMultiplier;
                float t = (actualSpeed - lowerBound.TargetSpeed) / (upperBound.TargetSpeed - lowerBound.TargetSpeed);
                t = MathHelper.Clamp(t, 0f, 1f);
                return MathHelper.Lerp(lowerBound.BurnMultiplier, upperBound.BurnMultiplier, t);
            }
        }

        return 0f;
    }

    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            currentFuel += RefuelAmount;
            currentFuel = Math.Min(currentFuel, maxFuel);
            interactingPlayer.HeldItem = null;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        var position = Position - new Vector2(tileSize / 2f);
        int barWidth = tileSize - 20;
        int barHeight = 8;
        Vector2 barPos = new Vector2(position.X + 10, position.Y + tileSize - 15);

        spriteBatch.Draw(AssetManager.BlankTexture, barPos, null, Color.Black, 0f, Vector2.Zero,
            new Vector2(barWidth, barHeight), SpriteEffects.None, 0f);

        float fuelPercentage = currentFuel / maxFuel;
        float currentBarWidthFloat = barWidth * fuelPercentage;

        Color barColor = fuelPercentage < LowFuelThreshold ? Color.Red : Color.Orange;
        spriteBatch.Draw(AssetManager.BlankTexture, barPos, null, barColor, 0f, Vector2.Zero,
            new Vector2(currentBarWidthFloat, barHeight), SpriteEffects.None, 0f);
    }
}