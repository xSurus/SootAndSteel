using System;
using System.IO;
using NUnit.Framework;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    public class SoundSettingsTests
    {
        private string tempFile;

        [SetUp]
        public void SetUp()
        {
            tempFile = Path.Combine(Path.GetTempPath(), $"gamelab_audio_settings_{Guid.NewGuid():N}.json");
            SoundSettings.SettingsFilePathOverride = tempFile;
        }

        [TearDown]
        public void TearDown()
        {
            SoundSettings.SettingsFilePathOverride = null;
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }

        [Test]
        public void Load_WithNoFile_ReturnsDefaults()
        {
            var settings = SoundSettings.Load();

            Assert.AreEqual(0.8f, settings.MasterVolume, 0.0001f);
            Assert.AreEqual(0.6f, settings.MusicVolume, 0.0001f);
            Assert.AreEqual(0.8f, settings.SfxVolume, 0.0001f);
        }

        [Test]
        public void Load_WithOutOfRangeValues_ClampsTo0To1()
        {
            File.WriteAllText(tempFile, "{\"MasterVolume\": 5.0, \"MusicVolume\": -3.0, \"SfxVolume\": 0.5}");

            var settings = SoundSettings.Load();

            Assert.AreEqual(1f, settings.MasterVolume, 0.0001f);
            Assert.AreEqual(0f, settings.MusicVolume, 0.0001f);
            Assert.AreEqual(0.5f, settings.SfxVolume, 0.0001f);
        }

        [Test]
        public void Save_ThenLoad_RoundTripsValues()
        {
            var settings = new SoundSettings { MasterVolume = 0.3f, MusicVolume = 0.4f, SfxVolume = 0.9f };

            settings.Save();
            var loaded = SoundSettings.Load();

            Assert.AreEqual(0.3f, loaded.MasterVolume, 0.0001f);
            Assert.AreEqual(0.4f, loaded.MusicVolume, 0.0001f);
            Assert.AreEqual(0.9f, loaded.SfxVolume, 0.0001f);
        }
    }
}
