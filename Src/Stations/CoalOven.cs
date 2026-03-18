using System;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Map;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Stations;

public class CoalOven : AbstractStation
{
    public float MaxFuel { get; }
    public float CurrentFuel { get; private set; }
    private readonly float burnRate;
    private readonly float refuelAmount;
    private readonly float lowFuelThreshold;

    public CoalOven(GameplayConfig gameplayConfig) : base("CoalOven", Color.DarkRed)
    {
        MaxFuel = gameplayConfig.CoalOvenMaxFuel;
        burnRate = gameplayConfig.CoalOvenBurnRate;
        refuelAmount = gameplayConfig.CoalOvenRefuelAmount;
        lowFuelThreshold = gameplayConfig.CoalOvenLowFuelThreshold;
        CurrentFuel = MaxFuel;
    }

    public override void Update(float deltaTime)
    {
        if (CurrentFuel > 0)
        {
            CurrentFuel -= burnRate * deltaTime;
            CurrentFuel = Math.Max(0, CurrentFuel);
        }
    }

    public override void Interact(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            CurrentFuel += refuelAmount;
            CurrentFuel = Math.Min(CurrentFuel, MaxFuel);
            interactingPlayer.HeldItem = null;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, int tileSize)
    {
        base.Draw(spriteBatch, position, tileSize);

        int barWidth = tileSize - 20;
        int barHeight = 8;
        Vector2 barPos = new Vector2(position.X + 10, position.Y + tileSize - 15);

        spriteBatch.Draw(AssetManager.BlankTexture, barPos, null, Color.Black, 0f, Vector2.Zero,
            new Vector2(barWidth, barHeight), SpriteEffects.None, 0f);

        float fuelPercentage = CurrentFuel / MaxFuel;
        float currentBarWidthFloat = barWidth * fuelPercentage;

        Color barColor = fuelPercentage < lowFuelThreshold ? Color.Red : Color.Orange;
        spriteBatch.Draw(AssetManager.BlankTexture, barPos, null, barColor, 0f, Vector2.Zero,
            new Vector2(currentBarWidthFloat, barHeight), SpriteEffects.None, 0f);
    }
}