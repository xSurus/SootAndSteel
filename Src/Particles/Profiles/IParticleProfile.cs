using System;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Profiles;

public interface IParticleProfile
{
    void GetOffsetAndDirection(Random random, out Vector2 offset, out Vector2 direction);
}