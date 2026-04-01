using System;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Profiles;

public class ConeProfile(Vector2 direction, float spreadRadians) : IParticleProfile
{
    private Vector2 Direction = direction;
    private float SpreadRadians = spreadRadians;

    public void GetOffsetAndDirection(Random random, out Vector2 offset, out Vector2 direction)
    {
        offset = Vector2.Zero;
        float angle = (float)Math.Atan2(Direction.Y, Direction.X);
        float randomAngle = angle + ((float)random.NextDouble() - 0.5f) * SpreadRadians;
        direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
    }
}