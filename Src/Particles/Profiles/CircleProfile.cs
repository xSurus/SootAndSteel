using System;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Profiles;

public class CircleProfile(float radius, bool onlyRing = false, bool radiateOutward = true)
    : IParticleProfile
{
    public float Radius { get; set; } = radius;

    // only emit from outer ring or from the whole disk
    public bool OnlyRing { get; set; } = onlyRing;

    // fly away from center or use base direction
    public bool RadiateOutward { get; set; } = radiateOutward;

    public Vector2 BaseDirection { get; set; } = -Vector2.UnitY;

    public void GetOffsetAndDirection(Random random, out Vector2 offset, out Vector2 direction)
    {
        float angle = (float)(random.NextDouble() * Math.PI * 2.0);
        float distance = OnlyRing ? Radius : Radius * (float)Math.Sqrt(random.NextDouble());
        offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
        if (RadiateOutward)
        {
            // normalize offset or use fallback if particle is at 0,0
            direction = offset == Vector2.Zero
                ? new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))
                : Vector2.Normalize(offset);
        }
        else
        {
            direction = BaseDirection;
        }
    }
}