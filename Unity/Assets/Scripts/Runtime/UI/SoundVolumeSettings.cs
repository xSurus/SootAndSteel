using Gamelab.Services.Sound;
using Gamelab.UI.ViewModels;

namespace Gamelab.UI.Runtime
{
    /// <summary>IVolumeSettings over ISoundService. The service clamps and persists.</summary>
    public class SoundVolumeSettings : IVolumeSettings
    {
        private readonly ISoundService sound;

        public SoundVolumeSettings(ISoundService sound)
        {
            this.sound = sound;
        }

        public float MasterVolume => sound.Settings.MasterVolume;
        public float MusicVolume => sound.Settings.MusicVolume;
        public float SfxVolume => sound.Settings.SfxVolume;

        public void SetMasterVolume(float value) => sound.SetMasterVolume(value);
        public void SetMusicVolume(float value) => sound.SetMusicVolume(value);
        public void SetSfxVolume(float value) => sound.SetSfxVolume(value);
    }
}
