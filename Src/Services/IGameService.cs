using Microsoft.Xna.Framework;

namespace Gamelab.Services;

public interface IGameService
{
    void Initialize(GamelabGame game);
    void Update(GameTime gameTime);
    void Draw();
    void Shutdown();
}

