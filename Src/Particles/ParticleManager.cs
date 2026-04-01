using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Particles;

public class ParticleManager
{
    private readonly List<ParticleEmitter> emitters = [];

    public void AddEmitter(ParticleEmitter effect)
    {
        emitters.Add(effect);
    }

    public void RemoveEmitter(ParticleEmitter effect)
    {
        emitters.Remove(effect);
    }

    public void Update(float dt)
    {
        foreach (var effect in emitters)
        {
            effect.Update(dt);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var effect in emitters)
        {
            effect.Draw(spriteBatch);
        }
    }
}