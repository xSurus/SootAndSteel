namespace Gamelab.Services.Music;

public interface IMusicService
{    
    float Volume { get; set; }
    bool IsRepeating { get; set; }
    
    void Play(string songName, bool repeating = true, float volume = 1.0f);
    void FadeOut(double duration);
    void FadeOutAndPlay(string songName, float duration, bool repeating = true, float volume = 1.0f);
}