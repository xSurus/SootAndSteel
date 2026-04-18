using System;
using Microsoft.Xna.Framework;

namespace Gamelab.Particles.Profiles;

public class BoxProfile(float width, float height, Vector2 baseDirection) : IParticleProfile
{
    public float Width { get; set; } = width;
    public float Height { get; set; } = height;
    private Vector2 BaseDirection { get; set; } = baseDirection;

    public void GetOffsetAndDirection(Random random, out Vector2 offset, out Vector2 direction)
    {
        // random point in box
        float offsetX = ((float)random.NextDouble() - 0.5f) * Width;
        float offsetY = ((float)random.NextDouble() - 0.5f) * Height;
        offset = new Vector2(offsetX, offsetY);

        // add some randomness to direction
        float driftX = ((float)random.NextDouble() - 0.5f) * 0.5f;
        direction = Vector2.Normalize(BaseDirection + new Vector2(driftX, 0));
    }
}