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
    private const float ChimneyOffsetY = -520f;
    private const float NoseDrawOffsetX = -10f;
    private const float NoseDrawOffsetY = 15f;
    private float heightPixels;
    private float widthPixels;
    
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
        smokeEmitter = ParticleFactory.CreateOvenSmoke(position);
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
            gameplayContext.State.IsCoalOvenBurning = currentFuel > 0f;
            smokeEmitter.AutoTrigger = false;
            chimneyEmitter.AutoTrigger = false;
            return;
        }

        if (currentFuel > 0)
        {
            gameplayContext.State.IsCoalOvenBurning = true;
            float speedMultiplier = GetInterpolatedBurnMultiplier(gameplayContext.State.actualSpeed);
            currentFuel -= BurnRate * gameplayContext.State.MaintenanceScale * speedMultiplier * dt;
            smokeEmitter.AutoTrigger = true;

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
            : Position + new Vector2(0, heightPixels / 2f);

        float depth = RenderUtility.CalculateDepth(feetAnchor.Y);

        Texture2D tex = AssetManager.GetStructureTexture("TrainNose");
        if (tex != AssetManager.BlankTexture)
        {
            // Pivot at bottom-left of the texture so the back of the nose lines up with the cab seam (column Width).
            Vector2 origin = new Vector2(0f, tex.Height);
            spriteBatch.Draw(tex, feetAnchor + new Vector2(NoseDrawOffsetX, NoseDrawOffsetY), null, Color.White, 0f, origin, tileScale, SpriteEffects.None, depth);
        }
        else
        {
            Vector2 centerBottom = Position + new Vector2(0, heightPixels / 2f);
            Vector2 placeholderOrigin = new Vector2(0.5f, 1f);
            spriteBatch.Draw(AssetManager.BlankTexture, centerBottom, null, Color.DarkGray, 0f, placeholderOrigin,
                new Vector2(widthPixels, heightPixels), SpriteEffects.None, depth);
        }

        float barDepth = depth + RenderUtility.Eps;
        int barMaxWidth = (int)(widthPixels * 0.8f);
        int barHeight = 12;
        // Keep fuel UI near the top of the car so it is not confused with wheels / tracks.
        float barTopY = Position.Y - heightPixels / 2f + tileSize * 0.35f;
        Vector2 barPos = new Vector2(Position.X - barMaxWidth / 2f, barTopY);

        float fuelRatio = Math.Max(0f, currentFuel / maxFuel);
        int currentBarWidth = (int)(barMaxWidth * fuelRatio);

        spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle((int)barPos.X, (int)barPos.Y, barMaxWidth, barHeight),
            null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, barDepth);

        Color barColor = currentFuel <= LowFuelThreshold ? Color.Red : Color.DarkOrange;
        spriteBatch.Draw(AssetManager.BlankTexture,
            new Rectangle((int)barPos.X, (int)barPos.Y, currentBarWidth, barHeight), null, barColor, 0f, Vector2.Zero,
            SpriteEffects.None, barDepth + RenderUtility.Eps);
    }
}