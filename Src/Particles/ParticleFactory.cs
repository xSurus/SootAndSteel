using System;
using Gamelab.Assets;
using Gamelab.Particles.Modifiers;
using Gamelab.Particles.Profiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles;

public static class ParticleFactory
{
    public static ParticleEmitter CreateSnowstorm(Random random, Point screenSize = default)
    {
        if (screenSize == default)
            screenSize = new Point(1920, 1080);

        var emitter = new ParticleEmitter(1000, AssetManager.SparkTexture, random)
        {
            Position = new Vector2(screenSize.X / 2f, screenSize.Y / 2f),
            AutoTrigger = true,
            AutoTriggerFrequency = 0.05f,
            Profile = new BoxProfile(screenSize.X * 1.5f, screenSize.Y, Vector2.UnitY),

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

    public static ParticleEmitter CreateCannonballTrail(Random random)
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
        emitter.Modifiers.Add(new TrainWindModifier());

        return emitter;
    }

    public static ParticleEmitter CreateBloodSplatter(Vector2 position, Random random)
    {
        var emitter = new ParticleEmitter(100, AssetManager.SparkTexture, random)
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
}