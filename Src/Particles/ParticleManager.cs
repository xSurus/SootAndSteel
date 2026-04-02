using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Particles;

public class ParticleManager
{
    private readonly List<ParticleEmitter> emitters = [];

    public void AddEmitter(ParticleEmitter emitter)
    {
        emitters.Add(emitter);
    }

    public void RemoveEmitter(ParticleEmitter emitter)
    {
        emitters.Remove(emitter);
    }

    public void Update(float dt)
    {
        for (int i = emitters.Count - 1; i >= 0; i--)
        {
            emitters[i].Update(dt);
            if (emitters[i].ShouldRemove && emitters[i].activeParticles == 0)
            {
                emitters.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var emitter in emitters)
        {
            emitter.Draw(spriteBatch);
        }
    }

    public void Clear()
    {
        emitters.Clear();
    }
}