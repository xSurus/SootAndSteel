using System;
using Gamelab.Services.Sound;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;

namespace Gamelab.UI;

/// <summary>
/// Plays a short stamp "slam" animation on a Gum sprite (scale/rotation pop then settle).
/// </summary>
public sealed class StampRevealAnimator
{
    private readonly SpriteRuntime stamp;
    private readonly float baseWidth;
    private readonly float baseHeight;
    private readonly float baseRotation;
    private readonly float popScale;
    private readonly float popRotationOffset;
    private readonly float durationSeconds;

    private float timer;
    private bool active;
    private bool soundPlayed;

    public StampRevealAnimator(
        SpriteRuntime stamp,
        float durationSeconds = 0.25f,
        float popScale = 1.35f,
        float popRotationOffset = -10f)
    {
        this.stamp = stamp;
        this.durationSeconds = durationSeconds;
        this.popScale = popScale;
        this.popRotationOffset = popRotationOffset;

        baseWidth = stamp.Width;
        baseHeight = stamp.Height;
        baseRotation = stamp.Rotation;
        stamp.Visible = false;
        GamelabGame.Instance.Services.GetService<ISoundService>().LoadSound(Sounds.Stamp);
    }

    public void Trigger()
    {
        active = true;
        timer = 0f;
        stamp.Visible = true;
        stamp.Width = baseWidth * popScale;
        stamp.Height = baseHeight * popScale;
        stamp.Rotation = baseRotation + popRotationOffset;
    }

    public void Update(float dt)
    {
        if (!active)
            return;

        timer += dt;
        float t = Math.Clamp(timer / durationSeconds, 0f, 1f);
        float eased = Easing.SmoothStep(t);

        if (eased > 0.5 && !soundPlayed)
        {
            GamelabGame.Instance.Services.GetService<ISoundService>().PlayOnce(Sounds.Stamp);
            soundPlayed = true;
        }
        
        stamp.Width = MathHelper.Lerp(baseWidth * popScale, baseWidth, eased);
        stamp.Height = MathHelper.Lerp(baseHeight * popScale, baseHeight, eased);
        stamp.Rotation = MathHelper.Lerp(baseRotation + popRotationOffset, baseRotation, eased);

        if (t >= 1f)
            active = false;
    }
}