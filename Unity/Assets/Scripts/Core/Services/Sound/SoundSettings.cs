using System;
using System.IO;
using Newtonsoft.Json;

namespace Gamelab.Services.Sound
{
    public class SoundSettings
    {
        private static readonly string SettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gamelab");

        // Test-only seam: when set, Load()/Save() use this file path instead
        // of the real per-user AppData settings file. Tests must reset this
        // to null in TearDown.
        public static string SettingsFilePathOverride;

        private static string SettingsFile =>
            SettingsFilePathOverride ?? Path.Combine(SettingsDirectory, "audio_settings.json");

        public float MasterVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.6f;
        public float SfxVolume { get; set; } = 0.8f;

        public static SoundSettings Load()
        {
            if (!File.Exists(SettingsFile)) return new SoundSettings();

            try
            {
                string json = File.ReadAllText(SettingsFile);
                var loaded = JsonConvert.DeserializeObject<SoundSettings>(json) ?? new SoundSettings();
                loaded.ClampAll();
                return loaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SoundSettings] Failed to load audio settings, using defaults: {ex.Message}");
                return new SoundSettings();
            }
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFile);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SoundSettings] Failed to save audio settings: {ex.Message}");
            }
        }

        private void ClampAll()
        {
            MasterVolume = Math.Clamp(MasterVolume, 0f, 1f);
            MusicVolume = Math.Clamp(MusicVolume, 0f, 1f);
            SfxVolume = Math.Clamp(SfxVolume, 0f, 1f);
        }
    }
}
