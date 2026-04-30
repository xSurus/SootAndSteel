using System;
using System.Collections.Generic;
using Gamelab.Systems;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;

namespace Gamelab.Services.Animation;

public class AnimationService : IAnimationService, IGameSystem, IDisposable
{
    private readonly List<AnimatedSprite> activeSprites = new();

    public float TimeScale { get; set; } = 1.0f;

    public void Register(AnimatedSprite sprite)
    {
        if (!activeSprites.Contains(sprite))
            activeSprites.Add(sprite);
    }

    public void Unregister(AnimatedSprite sprite)
    {
        activeSprites.Remove(sprite);
    }

    public void Initialize(GamelabGame game)
    {
    }

    public void Update(GameTime gameTime)
    {
        for (int i = activeSprites.Count - 1; i >= 0; i--)
        {
            activeSprites[i].Update(gameTime);
        }
    }

    public void Draw()
    {
    }

    public void Shutdown() => Dispose();

    public void Dispose()
    {
        activeSprites.Clear();
    }
}