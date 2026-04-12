using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Particles.Modifiers;
using Gamelab.Particles.Profiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles;

public static class ParticleFactory
{
    public static ParticleEmitter CreateSnowstorm()
    {
        GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
        var emitter = new ParticleEmitter(1000, AssetManager.SparkTexture)
        {
            Position = new Vector2(gameplayContext.ScreenWidth / 2f, gameplayContext.ScreenHeight / 2f),
            AutoTrigger = true,
            AutoTriggerFrequency = 0.05f,
            Profile = new BoxProfile(gameplayContext.ScreenWidth * 1.5f, gameplayContext.ScreenHeight, Vector2.UnitY),

            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 10, MaxQuantity = 20,
                MinSpeed = 10f, MaxSpeed = 30f,
                MinAge = 2.0f, MaxAge = 4.0f,
                MinSize = 0.3f, MaxSize = 0.8f,
                Color = Color.White
            }
        };

        emitter.Modifiers.Add(new FadeInModifier(1.0f));
        emitter.Modifiers.Add(new FadeOutModifier(1.0f));
        emitter.Modifiers.Add(new TrainWindModifier());
        emitter.Modifiers.Add(new DirectionalForceModifier(Vector2.UnitY, 10f));

        return emitter;
    }

    public static ParticleEmitter CreateCannonballTrail()
    {
        var emitter = new ParticleEmitter(2000, AssetManager.SparkTexture)
        {
            Position = Vector2.Zero,
            AutoTrigger = true,
            AutoTriggerFrequency = 0.015f,
            Profile = new CircleProfile(radius: 6f, onlyRing: false, radiateOutward: false),
            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 0, MaxQuantity = 2,
                MinSpeed = 0f, MaxSpeed = 0f,
                MinAge = 0.2f, MaxAge = 0.4f,
                MinSize = 0.3f, MaxSize = 0.7f,
                Color = Color.SlateGray
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.4f));
        emitter.Modifiers.Add(new TrainWindModifier());

        return emitter;
    }

    public static ParticleEmitter CreateBloodSplatter(Vector2 position)
    {
        var emitter = new ParticleEmitter(100, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = false,
            Profile = new CircleProfile(radius: 5f, onlyRing: false, radiateOutward: true),
            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 15, MaxQuantity = 25,
                MinSpeed = 50f, MaxSpeed = 120f,
                MinAge = 0.3f, MaxAge = 0.7f,
                MinSize = 0.6f, MaxSize = 1.2f,
                Color = Color.DarkRed
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.3f));
        emitter.Modifiers.Add(new DirectionalForceModifier(Vector2.UnitY, 400f));

        return emitter;
    }

    public static ParticleEmitter CreateOvenSmoke(Vector2 position)
    {
        var emitter = new ParticleEmitter(200, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = false,
            AutoTriggerFrequency = 0.08f,
            Profile = new CircleProfile(radius: 6f, onlyRing: false, radiateOutward: true),

            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 1, MaxQuantity = 2,
                MinSpeed = 5f, MaxSpeed = 15f,
                MinAge = 1.0f, MaxAge = 2.0f,
                MinSize = 0.8f, MaxSize = 2.0f,
                Color = new Color(50, 50, 50, 200)
            }
        };

        emitter.Modifiers.Add(new FadeInModifier(0.2f));
        emitter.Modifiers.Add(new FadeOutModifier(0.8f));
        emitter.Modifiers.Add(new DirectionalForceModifier(new Vector2(0, -1), 30f));

        return emitter;
    }

    public static ParticleEmitter CreateCannonMuzzleFlash(Vector2 position, Vector2 direction)
    {
        var emitter = new ParticleEmitter(200, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = false,
            Profile = new ConeProfile(direction, MathHelper.PiOver4 * 0.5f),

            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 40, MaxQuantity = 60,
                MinSpeed = 100f, MaxSpeed = 300f,
                MinAge = 0.3f, MaxAge = 0.5f,
                MinSize = 0.5f, MaxSize = 2f,
                Color = Color.Orange
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.2f));

        return emitter;
    }
}