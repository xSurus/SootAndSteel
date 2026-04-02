using Gamelab.Particles;
using Gamelab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Services.Vfx;

public class VfxService : IVfxService, IGameSystem
{
    private readonly ParticleManager particleManager = new ParticleManager();

    public void Initialize(GamelabGame game)
    {
    }

    public void EmitBurst(ParticleEmitter emitter)
    {
        emitter.Emit();
        emitter.ShouldRemove = true;
        particleManager.AddEmitter(emitter);
    }

    public void AddContinuous(ParticleEmitter emitter)
    {
        particleManager.AddEmitter(emitter);
    }

    public void ClearAll()
    {
        particleManager.Clear();
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        particleManager.Update(dt);
    }


    public void Draw()
    {
        // nop, otherwise drawn over ui
    }

    public void Render(SpriteBatch spriteBatch)
    {
        particleManager.Draw(spriteBatch);
    }

    public void Shutdown()
    {
        ClearAll();
    }
}