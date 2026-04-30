using MonoGame.Extended.Graphics;

namespace Gamelab.Services.Animation;

public interface IAnimationService
{
    void Register(AnimatedSprite sprite);
    void Unregister(AnimatedSprite sprite);
}