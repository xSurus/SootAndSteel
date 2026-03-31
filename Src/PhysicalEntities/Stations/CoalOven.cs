using System;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Utils;
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

    public CoalOven(Vector2 position, GameplayContext gameplayContext)
        : base("CoalOven", Color.DarkRed, position, gameplayContext)
    {
        maxFuel = GamelabGame.Instance.GameplayConfig.CoalOvenMaxFuel;
        currentFuel = maxFuel;
    }

    public override void Update(float dt, GameplayContext gameplayContext)
    {
        if (currentFuel > 0)
        {
            gameplayContext.State.IsCoalOvenBurning = true;
            float speedMultiplier = gameplayContext.State.CurrentSpeed.BurnMultiplier;
            currentFuel -= BurnRate * speedMultiplier * dt;
        }
        else
        {
            gameplayContext.State.IsCoalOvenBurning = false;
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
        }
    }

    public override void OnPickup(Player interactingPlayer, GameplayContext gameplayContext)
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