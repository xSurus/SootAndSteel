using System;
using System.Collections.Generic;
using Gamelab.Systems;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;

namespace Gamelab.Services.Animation;

public class AnimationService : IAnimationService, IGameSystem, IDisposable
{
    private class AnimationEntry(AnimatedSprite sprite, bool isActive)
    {
        public AnimatedSprite Sprite { get; } = sprite;
        public bool IsActive { get; set; } = isActive;
    }

    private readonly List<AnimationEntry> trackedSprites = new();

    public float TimeScale { get; set; } = 1.0f;

    public void Register(AnimatedSprite sprite, bool startActive = true)
    {
        if (!trackedSprites.Exists(e => e.Sprite == sprite))
        {
            trackedSprites.Add(new AnimationEntry(sprite, startActive));
        }
    }

    public void Unregister(AnimatedSprite sprite)
    {
        trackedSprites.RemoveAll(e => e.Sprite == sprite);
    }

    public void SetActive(AnimatedSprite sprite, bool isActive)
    {
        AnimationEntry entry = trackedSprites.Find(e => e.Sprite == sprite);
        if (entry != null)
        {
            entry.IsActive = isActive;
        }
    }

    public void Initialize(GamelabGame game)
    {
    }

    public void Update(GameTime gameTime)
    {
        for (int i = trackedSprites.Count - 1; i >= 0; i--)
        {
            if (trackedSprites[i].IsActive)
            {
                trackedSprites[i].Sprite.Update(gameTime);
            }
        }
    }

    public void Draw()
    {
    }

    public void Shutdown() => Dispose();

    public void Dispose()
    {
        trackedSprites.Clear();
    }
}