using System;
using Gamelab.Services.Sound;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;


namespace Gamelab.Map.Train;

public class TrainSound(ISoundService soundService)
{
    private Logger logger = new Logger("TrainSound");
    
    private readonly SoundHandle chug = soundService.RegisterSound("train_chug");
    private readonly SoundHandle ga = soundService.RegisterSound("train_ga");
    private readonly Random random = new Random();
    private readonly double strokeInterval = 75;
    private double timer;
    private int currentStroke;

    public void Update(GameTime gameTime, float speed)
    {
        timer += gameTime.ElapsedGameTime.TotalSeconds * speed;

        if (timer > strokeInterval)
        {
            timer = 0;
            
            float pitch = Math.Clamp((speed - 150) / 1200, -0.1f, 0.35f) +
                          ((float)random.NextDouble() * 0.1f - 0.05f);
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
}