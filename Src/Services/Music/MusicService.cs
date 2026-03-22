using System;
using Gamelab.Systems;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace Gamelab.Services.Music;

public class MusicService : IDisposable, IGameSystem, IMusicService
{
    private Logger logger = new Logger("MusicService");
    
    private double animDuration;
    private double totalElapsed;
    private Action<double> animation;

    public float Volume
    {
        get => MediaPlayer.Volume;
        set => MediaPlayer.Volume = value;
    }

    public bool IsRepeating
    {
        get => MediaPlayer.IsRepeating;
        set => MediaPlayer.IsRepeating = value;
    }
    
    public void Initialize(GamelabGame game)
    {
    }

    public void Play(string songName, bool repeating = true, float volume = 1.0f)
    {
        try
        {
            Song song = LoadSong(songName);
            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = repeating;
            MediaPlayer.Volume = volume;
            MediaPlayer.Play(song);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
        }
    }

    public void FadeIn(double duration, float volume = 1.0f)
    {
        animDuration = duration;
        totalElapsed = 0;
        animation = (elapsedTime) =>
        {
            totalElapsed += elapsedTime;
            MediaPlayer.Volume = (float)(Math.Clamp(totalElapsed / animDuration, 0.0, 1.0) * volume);
            if (totalElapsed >= animDuration)
            {
                animation = null;
            }
        };
    }

    public void FadeOut(double duration)
    {
        animDuration = duration;
        totalElapsed = 0;
        double preAnimVolume = MediaPlayer.Volume;
        animation = (elapsedTime) =>
        {
            totalElapsed += elapsedTime;
            MediaPlayer.Volume = (float) (Math.Clamp(1.0 - (totalElapsed / animDuration), 0.0, 1.0) * preAnimVolume);
            if (totalElapsed >= animDuration)
            {
                MediaPlayer.Stop();
                animation = null;
            }
        };
    }

    public void FadeOutAndPlay(string songName, float duration, bool repeating = true, float volume = 1.0f)
    {
        try
        {
            Song song = LoadSong(songName);

            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.IsRepeating = repeating;
                MediaPlayer.Volume = 0;
                MediaPlayer.Play(song);
                FadeIn(duration / 2f, volume);
                return;
            }

            animDuration = duration / 2f;
            totalElapsed = 0;
            double preAnimVolume = MediaPlayer.Volume;
            animation = (elapsedTime) =>
            {
                totalElapsed += elapsedTime;
                MediaPlayer.Volume = (float)(Math.Clamp(1.0 - (totalElapsed / animDuration), 0.0, 1.0) * preAnimVolume);
                if (totalElapsed >= animDuration)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.IsRepeating = repeating;
                    MediaPlayer.Volume = 0;
                    MediaPlayer.Play(song);
                    FadeIn(duration / 2f, volume);
                }
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
        }
}

    private Song LoadSong(string songName)
    {
        return GamelabGame.Instance.Content.Load<Song>("music/" + songName);
    }

    public void Dispose()
    {
    }

    public void Update(GameTime gameTime)
    {
        animation?.Invoke(gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Draw()
    {
    }

    public void Shutdown()
    {
    }
}