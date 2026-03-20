using Microsoft.Xna.Framework;

namespace Gamelab.Systems;

public interface IGameSystem
{
    void Initialize(GamelabGame game);
    void Update(GameTime gameTime);
    void Draw();
    void Shutdown();
}

