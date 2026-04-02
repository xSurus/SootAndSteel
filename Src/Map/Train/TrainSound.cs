using System;
using Gamelab.Map.Train.State;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;

namespace Gamelab.Map.Train;

public class TrainSound(ISoundService soundService) : IDisposable
{
    private readonly SoundHandle chug = soundService.RegisterSound("train_chug");
    private readonly SoundHandle ga = soundService.RegisterSound("train_ga");
    private readonly Random random = Random.Shared;
    private readonly double strokeInterval = 50;
    private double timer;
    private int currentStroke;
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    public void Update(GameTime gameTime)
    {
        timer += gameTime.ElapsedGameTime.TotalSeconds * gameplayContext.State.actualSpeed;

        if (timer > strokeInterval)
        {
            timer = 0;

            float pitch = Math.Clamp((gameplayContext.State.actualSpeed - 150) / 1600, -0.1f, 0.2f) +
                          ((float)random.NextDouble() * 0.05f - 0.025f);
            float volume = 1f - currentStroke / 8f;

            if (currentStroke % 2 == 0)
            {
                chug.Play(pitch: pitch, volume: volume);
            }
            else
            {
                ga.Play(pitch: pitch, volume: volume);
            }

            currentStroke = (currentStroke + 1) % 4;
        }
    }

    public void Dispose()
    {
        chug.Dispose();
        ga.Dispose();
    }
}