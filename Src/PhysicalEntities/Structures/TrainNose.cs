using System;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Players;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Structures;

public class TrainNose : AbstractPhysicalEntity, IPickable, IUpdatable
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly float maxFuel;
    private float currentFuel;
    private float BurnRate => GamelabGame.Instance.GameplayConfig.CoalOvenBurnRate;
    private float RefuelAmount => GamelabGame.Instance.GameplayConfig.CoalOvenRefuelAmount;
    private float LowFuelThreshold => GamelabGame.Instance.GameplayConfig.CoalOvenLowFuelThreshold;
    private ParticleEmitter smokeEmitter;

    public TrainNose(Vector2 position)
    {
        maxFuel = GamelabGame.Instance.GameplayConfig.CoalOvenMaxFuel;
        currentFuel = maxFuel;

        int trainHeightTiles = GamelabGame.Instance.GameplayConfig.TrainHeight;
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        float widthPixels = tileSize;
        float heightPixels = trainHeightTiles * tileSize;

        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(widthPixels.ToMeters(), heightPixels.ToMeters(), 1f,
            position.ToMeters());
        smokeEmitter = ParticleFactory.CreateOvenSmoke(position);
        GamelabGame.Instance.Services.GetService<IVfxService>()?.AddContinuous(smokeEmitter);
    }

    public void Update(float dt)
    {
        if (currentFuel > 0)
        {
            gameplayContext.State.IsCoalOvenBurning = true;
            float speedMultiplier = gameplayContext.State.CurrentSpeed.BurnMultiplier;
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

    public void OnPickup(Player interactingPlayer)
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
        int trainHeightTiles = GamelabGame.Instance.GameplayConfig.TrainHeight;
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        float widthPixels = tileSize;
        float heightPixels = trainHeightTiles * tileSize;

        Vector2 centerPixels = PhysicsBody.Position.ToPixels();

        Rectangle boxRect = new Rectangle(
            (int)(centerPixels.X - widthPixels / 2f),
            (int)(centerPixels.Y - heightPixels / 2f),
            (int)widthPixels,
            (int)heightPixels
        );
        spriteBatch.Draw(AssetManager.BlankTexture, boxRect, Color.DarkSlateGray);

        int barMaxWidth = (int)(widthPixels * 0.8f);
        int barHeight = 12;
        int barX = (int)(centerPixels.X - barMaxWidth / 2f);
        int barY = (int)(centerPixels.Y - barHeight / 2f);

        float fuelRatio = Math.Max(0f, currentFuel / maxFuel);
        int currentBarWidth = (int)(barMaxWidth * fuelRatio);

        spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(barX, barY, barMaxWidth, barHeight), Color.Black);

        Color barColor = currentFuel <= LowFuelThreshold ? Color.Red : Color.DarkOrange;
        spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(barX, barY, currentBarWidth, barHeight), barColor);
    }
}