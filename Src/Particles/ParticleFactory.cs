using System;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Particles.Modifiers;
using Gamelab.Particles.Profiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles;

public static class ParticleFactory
{
    public static ParticleEmitter CreateSnowstorm(GameplayContext gameplayContext, Random random)
    {
        var emitter = new ParticleEmitter(10000, AssetManager.SparkTexture, random)
        {
            Position = new Vector2(gameplayContext.ScreenWidth / 2f, gameplayContext.ScreenHeight / 2f),
            AutoTrigger = true,
            AutoTriggerFrequency = 0.05f,
            Profile = new BoxProfile(gameplayContext.ScreenWidth * 1.5f, gameplayContext.ScreenHeight, Vector2.UnitY),

            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 20, MaxQuantity = 40,
                MinSpeed = 10f, MaxSpeed = 30f,
                MinAge = 2.0f, MaxAge = 4.0f,
                MinSize = 0.3f, MaxSize = 0.8f,
                Color = Color.White
            }
        };

        emitter.Modifiers.Add(new FadeInModifier(1.0f));
        emitter.Modifiers.Add(new FadeOutModifier(1.0f));
        emitter.Modifiers.Add(new TrainWindModifier(gameplayContext));
        emitter.Modifiers.Add(new GravityModifier(10f));

        return emitter;
    }

    public static ParticleEmitter CreateCannonballTrail(GameplayContext gameplayContext, Random random)
    {
        var emitter = new ParticleEmitter(2000, AssetManager.SparkTexture, random)
        {
            Position = Vector2.Zero,
            AutoTrigger = true,
            AutoTriggerFrequency = 0.015f,
            Profile = new CircleProfile(radius: 6f, onlyRing: false, radiateOutward: false),
            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 1, MaxQuantity = 3,
                MinSpeed = 0f, MaxSpeed = 0f,
                MinAge = 0.4f, MaxAge = 0.8f,
                MinSize = 0.5f, MaxSize = 1.0f,
                Color = Color.Red
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.4f));
        emitter.Modifiers.Add(new TrainWindModifier(gameplayContext));

        return emitter;
    }
}