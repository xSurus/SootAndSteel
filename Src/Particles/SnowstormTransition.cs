using System;
using System.Linq;
using Gamelab.Particles.Modifiers;

namespace Gamelab.Particles;

public static class SnowstormTransition
{
    public readonly struct Baseline
    {
        public readonly float AutoTriggerFrequency;
        public readonly int MinQuantity;
        public readonly int MaxQuantity;
        public readonly float MinSpeed;
        public readonly float MaxSpeed;
        public readonly float MinSize;
        public readonly float MaxSize;
        public readonly float DownwardForceStrength;

        public Baseline(
            float autoTriggerFrequency,
            int minQuantity,
            int maxQuantity,
            float minSpeed,
            float maxSpeed,
            float minSize,
            float maxSize,
            float downwardForceStrength)
        {
            AutoTriggerFrequency = autoTriggerFrequency;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
            MinSize = minSize;
            MaxSize = maxSize;
            DownwardForceStrength = downwardForceStrength;
        }
    }

    public static Baseline Capture(ParticleEmitter emitter)
    {
        var grav = emitter.Modifiers.OfType<DirectionalForceModifier>().FirstOrDefault();
        float gravStrength = grav?.Strength ?? 0f;
        return new Baseline(
            emitter.AutoTriggerFrequency,
            emitter.Parameters.MinQuantity,
            emitter.Parameters.MaxQuantity,
            emitter.Parameters.MinSpeed,
            emitter.Parameters.MaxSpeed,
            emitter.Parameters.MinSize,
            emitter.Parameters.MaxSize,
            gravStrength);
    }

    public static void ApplyBlizzardIntensity(ParticleEmitter emitter, Baseline b, float whiteOpacity01)
    {
        float t = Math.Clamp(whiteOpacity01, 0f, 1f);
        DirectionalForceModifier grav = emitter.Modifiers.OfType<DirectionalForceModifier>().FirstOrDefault();

        if (t <= 0.0001f)
        {
            emitter.AutoTriggerFrequency = b.AutoTriggerFrequency;
            emitter.Parameters.MinQuantity = b.MinQuantity;
            emitter.Parameters.MaxQuantity = b.MaxQuantity;
            emitter.Parameters.MinSpeed = b.MinSpeed;
            emitter.Parameters.MaxSpeed = b.MaxSpeed;
            emitter.Parameters.MinSize = b.MinSize;
            emitter.Parameters.MaxSize = b.MaxSize;
            emitter.Capacity = 1000; // Reset to baseline capacity
            if (grav != null)
            {
                grav.Strength = b.DownwardForceStrength;
            }

            return;
        }
        float amp = 1f + 8 * t;
        emitter.AutoTriggerFrequency = b.AutoTriggerFrequency / amp;
        emitter.Parameters.MinQuantity = (int)Math.Round(b.MinQuantity * amp);
        emitter.Parameters.MaxQuantity = (int)Math.Round(b.MaxQuantity * amp);
        emitter.Parameters.MinSpeed = b.MinSpeed * (1f + 0.85f * t);
        emitter.Parameters.MaxSpeed = b.MaxSpeed * (1f + 1.1f * t);
        emitter.Parameters.MinSize = b.MinSize * (1f + 0.35f * t);
        emitter.Parameters.MaxSize = b.MaxSize * (1f + 5 * t);
        emitter.Capacity = 20000;


        if (grav != null)
        {
            grav.Strength = b.DownwardForceStrength * (1f + 1.85f * t);        
        }
    }
}
