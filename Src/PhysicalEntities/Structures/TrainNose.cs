using System;
using Gamelab.Assets;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Structures;

public class TrainNose : AbstractPhysicalEntity, IPickable, IUpdatable
{
    private readonly float maxFuel;
    private float currentFuel;
    private float BurnRate => GamelabGame.Instance.GameplayConfig.CoalOvenBurnRate;
    private float RefuelAmount => GamelabGame.Instance.GameplayConfig.CoalOvenRefuelAmount;
    private float LowFuelThreshold => GamelabGame.Instance.GameplayConfig.CoalOvenLowFuelThreshold;
    private ParticleEmitter smokeEmitter;
    private ParticleEmitter chimneyEmitter;
    private const float ChimneyOffsetX = 650f;
    private const float ChimneyOffsetY = -500f;
    private const float NoseDrawOffsetX = -5f;
    private const float NoseDrawOffsetY = 15f;
    private float heightPixels;
    private float widthPixels;

    private float glowTimer;
    private const float GlowFrequency = 0.5f;
    private const float GlowMin = 0.3f;
    private const float GlowMax = 1.0f;

    private ISoundService soundService;

    public TrainNose(Vector2 position)
    {
        maxFuel = GamelabGame.Instance.GameplayConfig.CoalOvenMaxFuel;
        currentFuel = maxFuel;

        int trainHeightTiles = GamelabGame.Instance.GameplayConfig.TrainHeight;
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;

        widthPixels = tileSize;
        heightPixels = trainHeightTiles * tileSize;

        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(widthPixels.ToMeters(), heightPixels.ToMeters(), 1f,
            position.ToMeters());
        smokeEmitter = ParticleFactory.CreateOvenSmoke(position + new Vector2(60, 0));
        GamelabGame.Instance.Services.GetService<IVfxService>()?.AddContinuous(smokeEmitter);

        chimneyEmitter = ParticleFactory.CreateChimneySmoke();
        GamelabGame.Instance.Services.GetService<IVfxService>()?.AddContinuous(chimneyEmitter);

        soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.ShovelDown);
    }

    public void Update(float dt)
    {
        TrainMap map = gameplayContext.Map;
        if (map != null)
            chimneyEmitter.Position = map.GetTileTopLeftPixels(map.Width, map.Height - 1)
                                      + new Vector2(0, map.TileSize)
                                      + new Vector2(ChimneyOffsetX, ChimneyOffsetY);

        if (!gameplayContext.State.FuelBurningEnabled)
        {
            currentFuel = maxFuel;
            gameplayContext.State.IsCoalOvenBurning = currentFuel > 0f;
            smokeEmitter.AutoTrigger = true;
            chimneyEmitter.AutoTrigger = false;
        }

        if (currentFuel > 0)
        {
            gameplayContext.State.IsCoalOvenBurning = true;
            float speedMultiplier = GetInterpolatedBurnMultiplier(gameplayContext.State.actualSpeed);
            currentFuel -= BurnRate * gameplayContext.State.MaintenanceScale * speedMultiplier * dt;
            smokeEmitter.AutoTrigger = true;
            float fuelRatio = currentFuel / maxFuel;
            if (fuelRatio > 0.25f)
            {
                // fire phase (100% -> 25%)
                float t = (fuelRatio - 0.25f) / 0.75f + 0.25f;
                smokeEmitter.Parameters.Color = Color.DarkOrange;
                smokeEmitter.Parameters.MaxQuantity = (int)Math.Max(0, Math.Round(5 * t));
                smokeEmitter.Parameters.MinQuantity = (int)Math.Max(0, smokeEmitter.Parameters.MaxQuantity - 1);
            }
            else
            {
                // smoke phase (25% -> 0%)
                float t = fuelRatio / 0.25f;
                smokeEmitter.Parameters.Color = new Color(50, 50, 50, 200);
                smokeEmitter.Parameters.MaxQuantity = (int)Math.Max(0, Math.Round(3 * t));
                smokeEmitter.Parameters.MinQuantity = (int)Math.Max(0, smokeEmitter.Parameters.MaxQuantity - 1);
            }

            bool chimneyActive = gameplayContext.State.actualSpeed > 0f;
            chimneyEmitter.AutoTrigger = chimneyActive;
            if (chimneyActive)
            {
                float maxSpeed = TrainSpeedSetting.Fast.TargetSpeed;
                float speedT = MathHelper.Clamp(gameplayContext.State.actualSpeed / maxSpeed, 0f, 1f);
                chimneyEmitter.AutoTriggerFrequency = MathHelper.Lerp(0.30f, 0.04f, speedT);
            }
        }
        else
        {
            gameplayContext.State.IsCoalOvenBurning = false;
            gameplayContext.State.SlowDownIfRunning();
            smokeEmitter.AutoTrigger = false;
            chimneyEmitter.AutoTrigger = false;
        }

        glowTimer += dt;
    }

    private float GetInterpolatedBurnMultiplier(float actualSpeed)
    {
        var allSpeeds = TrainSpeedSetting.All;
        if (allSpeeds == null || allSpeeds.Count == 0) return 1f;
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

    public void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            currentFuel += RefuelAmount;
            currentFuel = Math.Min(currentFuel, maxFuel);
            interactingPlayer.HeldItem = null;
            soundService.PlayOnce(Sounds.ShovelDown);
        }
    }

    public void SetFuelLevel(float fuel)
    {
        currentFuel = MathHelper.Clamp(fuel, 0f, maxFuel);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        // Match TrainMap.DrawTrainTiles scaling so cab/nose art shares the same pixel density as train tiles.
        float tileScale = tileSize / (float)AssetManager.TileTexture[0].Width;

        TrainMap map = gameplayContext.Map;
        Vector2 feetAnchor = map != null
            ? map.GetTileTopLeftPixels(map.Width, map.Height - 1) + new Vector2(0, map.TileSize)
            : Position + new Vector2(0, heightPixels);

        float fuelFactor = Math.Min(Math.Max(0f, (currentFuel / maxFuel) * 2), 1);
        Texture2D tex = AssetManager.GetStructureTexture("TrainNoseOff");
        Texture2D texOn = AssetManager.GetStructureTexture("TrainNose");
        Texture2D texTop = AssetManager.GetStructureTexture("TrainNoseTop");
        Vector2 origin = new Vector2(0f, tex.Height);
        spriteBatch.Draw(tex, feetAnchor + new Vector2(NoseDrawOffsetX, NoseDrawOffsetY), null, Color.White, 0f, origin,
            tileScale, SpriteEffects.None, RenderUtility.FloorLayer + RenderUtility.Eps);
        spriteBatch.Draw(texOn, feetAnchor + new Vector2(NoseDrawOffsetX, NoseDrawOffsetY), null,
            Color.White * fuelFactor, 0f, origin, tileScale, SpriteEffects.None,
            RenderUtility.FloorLayer + 2 * RenderUtility.Eps);
        spriteBatch.Draw(texTop, feetAnchor + new Vector2(NoseDrawOffsetX, NoseDrawOffsetY), null,
            Color.White * fuelFactor, 0f, origin, tileScale, SpriteEffects.None, RenderUtility.TopEntityLayer);
    }

    public void DrawLightBatch(SpriteBatch spriteBatch)
    {
        float fuelFactor = Math.Min(Math.Max(0f, (currentFuel / maxFuel) * 2), 1);
        spriteBatch.Draw(
            AssetManager.GetDecorationTexture("FurnaceLight"),
            Position - new Vector2(340, 390),
            null,
            Color.White * fuelFactor,
            0f,
            Vector2.Zero,
            0.4f,
            SpriteEffects.None,
            0f
        );

        spriteBatch.Draw(
            AssetManager.GetDecorationTexture("FurnaceLight"),
            Position,
            null,
            Color.White * fuelFactor,
            0f,
            new Vector2(1024, 1024),
            0.7f,
            SpriteEffects.None,
            0f
        );

        float glow = MathHelper.Lerp(GlowMin, GlowMax,
            (MathF.Sin(glowTimer * GlowFrequency * MathHelper.TwoPi) + 1f) / 2f);

        glow *= fuelFactor;
        spriteBatch.Draw(
            AssetManager.GetDecorationTexture("FurnaceLight"),
            Position,
            null,
            Color.White * glow,
            0f,
            new Vector2(1024, 1024),
            0.7f,
            SpriteEffects.None,
            0f
        );
    }
}