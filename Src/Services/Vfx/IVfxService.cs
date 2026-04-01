using Gamelab.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Services.Vfx;

public interface IVfxService
{
    void EmitBurst(ParticleEmitter emitter);

    void AddContinuous(ParticleEmitter emitter);

    void ClearAll();

    void Render(SpriteBatch spriteBatch);
}