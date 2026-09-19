using NUnit.Framework;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    // These tests exercise the real FMOD Studio runtime (RuntimeManager, the
    // banks under Src/Content/soundbanks). The native libraries are committed
    // under Assets/Plugins/FMOD/platforms (see "FMOD native libraries" in
    // Unity/CONVENTIONS.md). Verification uses FMOD's own RESULT codes and
    // event/parameter state, never audible playback.
    public class SoundServicePlayModeTests
    {
        private SoundService soundService;
        private string settingsPath;

        [SetUp]
        public void SetUp()
        {
            settingsPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gamelab_test_{System.Guid.NewGuid():N}.json");
            SoundSettings.SettingsFilePathOverride = settingsPath;
            soundService = new SoundService();
        }

        [TearDown]
        public void TearDown()
        {
            soundService?.Dispose();
            SoundSettings.SettingsFilePathOverride = null;
            System.IO.File.Delete(settingsPath);
        }

        [Test]
        public void PlayOnce_MenuSelectEvent_StartsSuccessfully()
        {
            EventInstance instance = soundService.GetSoundInstance(Sounds.MenuSelect);

            RESULT result = instance.start();

            Assert.AreEqual(RESULT.OK, result);

            instance.getPlaybackState(out PLAYBACK_STATE state);
            Assert.AreNotEqual(PLAYBACK_STATE.STOPPED, state);
        }

        [Test]
        public void RegisterGlobalParameter_UpdatesTemperatureOnTick()
        {
            float value = 0.25f;
            soundService.RegisterGlobalParameter("Temperature", () => value);

            soundService.Tick();

            RuntimeManager.StudioSystem.getParameterByName("Temperature", out float actual);
            Assert.AreEqual(0.25f, actual, 0.01f);
        }
    }
}
