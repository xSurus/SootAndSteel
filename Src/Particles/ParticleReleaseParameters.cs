using Microsoft.Xna.Framework;

namespace Gamelab.Particles;

public class ParticleReleaseParameters
{
    public int MinQuantity = 1;
    public int MaxQuantity = 1;
    public float MinSpeed = 10f;
    public float MaxSpeed = 50f;
    public float MinAge = 1f;
    public float MaxAge = 2f;
    public float MinSize = 1f;
    public float MaxSize = 1f;
    public Color Color = Color.White;
}