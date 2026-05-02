using MonoGame.Extended.Graphics;

namespace Gamelab.Services.Animation;

public interface IAnimationService
{
    void Register(AnimatedSprite sprite, bool startActive = true);
    void SetActive(AnimatedSprite sprite, bool isActive);
    void Unregister(AnimatedSprite sprite);
}