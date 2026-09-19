using NUnit.Framework;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    // These tests exercise the real FMOD Studio runtime (RuntimeManager, the
    // vendored banks under Src/Content/soundbanks). They need the native FMOD
    // engine libraries for the current platform, which are not included in
    // the vendored Assets/Plugins/FMOD source (see this plan's "Known
    // blocker" section) - they must be added from an authenticated
    // FMOD/Unity Asset Store download before these tests can pass.
    // Verification uses FMOD's own RESULT codes and event/parameter state,
    // never audio playback - there's no way to listen for this in this
    // environment, and that's the point (per the work order).
    public class SoundServicePlayModeTests
    {
        private SoundService soundService;

        [SetUp]
        public void SetUp()
        {
            SoundSettings.SettingsFilePathOverride =
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gamelab_test_audio_settings.json");
            soundService = new SoundService();
        }

        [TearDown]
        public void TearDown()
        {
            soundService.Dispose();
            SoundSettings.SettingsFilePathOverride = null;
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
