using System;
using System.Collections.Generic;
using Gamelab.Particles.Modifiers;
using Gamelab.Particles.Profiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Particles;

public class ParticleEmitter
{
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }

    public bool AutoTrigger { get; set; }
    private bool shouldRemove;

    public bool ShouldRemove
    {
        get => shouldRemove;
        set
        {
            shouldRemove = value;
            if (shouldRemove)
            {
                AutoTrigger = false;
            }
        }
    }

    public float AutoTriggerFrequency { get; set; }
    private float triggerTimer;

    public IParticleProfile Profile { get; set; }
    public ParticleReleaseParameters Parameters { get; set; } = new();
    public List<IParticleModifier> Modifiers { get; } = new();

    private readonly Particle[] particles;
    public int activeParticles = 0;
    private readonly Random random = Random.Shared;
    private Vector2 origin;

    public ParticleEmitter(int capacity, Texture2D texture)
    {
        particles = new Particle[capacity];
        Texture = texture;
        origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
    }

    public void Emit()
    {
        int quantity = random.Next(Parameters.MinQuantity, Parameters.MaxQuantity + 1);

        for (int i = 0; i < quantity; i++)
        {
            if (activeParticles >= particles.Length) return;

            Profile.GetOffsetAndDirection(random, out Vector2 offset, out Vector2 direction);

            float speed = Parameters.MinSpeed +
                          (float)random.NextDouble() * (Parameters.MaxSpeed - Parameters.MinSpeed);
            float maxAge = Parameters.MinAge + (float)random.NextDouble() * (Parameters.MaxAge - Parameters.MinAge);
            float size = Parameters.MinSize + (float)random.NextDouble() * (Parameters.MaxSize - Parameters.MinSize);

            particles[activeParticles].Position = Position + offset;
            particles[activeParticles].Velocity = direction * speed;
            particles[activeParticles].Age = 0;
            particles[activeParticles].MaxAge = maxAge;
            particles[activeParticles].Size = size;
            particles[activeParticles].InitialColor = Parameters.Color;
            particles[activeParticles].Color = Parameters.Color;

            activeParticles++;
        }
    }

    public void Update(float dt)
    {
        if (AutoTrigger && !ShouldRemove)
        {
            triggerTimer += dt;
            while (triggerTimer >= AutoTriggerFrequency)
            {
                triggerTimer -= AutoTriggerFrequency;
                Emit();
            }
        }

        for (int i = 0; i < activeParticles; i++)
        {
            particles[i].Age += dt;

            if (particles[i].Age >= particles[i].MaxAge)
            {
                // swap the dead particle with the last particle in the array
                // ensures that we fill holes of dead particles with still active ones (which we also check for deadness)
                particles[i] = particles[activeParticles - 1];
                activeParticles--;
                i--;
                continue;
            }

            particles[i].Color = particles[i].InitialColor;

            foreach (IParticleModifier t in Modifiers)
            {
                t.Update(dt, ref particles[i]);
            }

            particles[i].Position += particles[i].Velocity * dt;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < activeParticles; i++)
        {
            spriteBatch.Draw(Texture, particles[i].Position, null, particles[i].Color,
                particles[i].Rotation, origin, particles[i].Size, SpriteEffects.None, 0f);
        }
    }
}