using Gamelab.Assets;
using Gamelab.Particles.Modifiers;
using Gamelab.Particles.Profiles;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles;

public static class ParticleFactory
{
    public static ParticleEmitter CreateSnowstorm(Point area = default)
    {
        if (area == default)
            area = new Point(1920, 1080);

        var emitter = new ParticleEmitter(1000, AssetManager.SparkTexture)
        {
            Position = new Vector2(area.X / 2f, area.Y / 2f),
            AutoTrigger = true,
            AutoTriggerFrequency = 0.05f,
            Profile = new BoxProfile(area.X * 1.5f, area.Y, Vector2.UnitY),

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

    public static ParticleEmitter CreateNeckBleed(Vector2 position)
    {
        var emitter = new ParticleEmitter(200, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = true,
            AutoTriggerFrequency = 0.05f,
            Profile = new ConeProfile(new Vector2(0f, -1f), MathHelper.PiOver4),
            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 2, MaxQuantity = 4,
                MinSpeed = 30f, MaxSpeed = 80f,
                MinAge = 0.3f, MaxAge = 0.6f,
                MinSize = 0.5f, MaxSize = 1.0f,
                Color = Color.DarkRed
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.4f));
        emitter.Modifiers.Add(new DirectionalForceModifier(Vector2.UnitY, 500f));

        return emitter;
    }

    public static ParticleEmitter CreateBloodHit(Vector2 position)
    {
        var emitter = new ParticleEmitter(50, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = false,
            Profile = new CircleProfile(radius: 3f, onlyRing: false, radiateOutward: true),
            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 3, MaxQuantity = 6,
                MinSpeed = 60f, MaxSpeed = 180f,
                MinAge = 0.2f, MaxAge = 0.5f,
                MinSize = 0.06f, MaxSize = 0.22f,
                Color = Color.DarkRed
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.3f));
        emitter.Modifiers.Add(new DirectionalForceModifier(Vector2.UnitY, 120f));

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

    public static ParticleEmitter CreateBuyParticles(Vector2 position)
    {
        var emitter = new ParticleEmitter(64, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = false,
            Profile = new ConeProfile(new Vector2(0, -1), MathHelper.Pi / 2f),

            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 18, MaxQuantity = 28,
                MinSpeed = 180f, MaxSpeed = 420f,
                MinAge = 0.5f, MaxAge = 1.1f,
                MinSize = 0.14f, MaxSize = 0.32f,
                Color = new Color(60, 220, 80)
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.5f));
        emitter.Modifiers.Add(new DirectionalForceModifier(Vector2.UnitY, 100f));

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

    public static ParticleEmitter CreateWorkbenchSpark(Vector2 position)
    {
        var emitter = new ParticleEmitter(32, AssetManager.SparkTexture)
        {
            Position = position,
            AutoTrigger = false,
            Profile = new CircleProfile(radius: 4f, onlyRing: false, radiateOutward: true),

            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 2, MaxQuantity = 4,
                MinSpeed = 80f, MaxSpeed = 240f,
                MinAge = 0.1f, MaxAge = 0.4f,
                MinSize = 0.18f, MaxSize = 0.50f,
                Color = new Color(255, 200, 80)
            }
        };

        emitter.Modifiers.Add(new FadeOutModifier(0.3f));
        emitter.Modifiers.Add(new DirectionalForceModifier(Vector2.UnitY * -1, 90f));

        return emitter;
    }
}