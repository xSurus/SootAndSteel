using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

namespace Gamelab.Services.Sound
{
    public class SoundService : ISoundService, IDisposable
    {
        private readonly List<ParameterBinding> parameterUpdates = new List<ParameterBinding>();
        private readonly Dictionary<string, EventDescription> eventDescriptions = new Dictionary<string, EventDescription>();
        private readonly Dictionary<string, EventInstance> playOnceInstances = new Dictionary<string, EventInstance>();

        public SoundSettings Settings { get; }

        public SoundService()
        {
            Settings = SoundSettings.Load();
            SetMasterVolume(Settings.MasterVolume);
            SetMusicVolume(Settings.MusicVolume);
            SetSfxVolume(Settings.SfxVolume);
            ResetGlobalParameters();
        }

        public void LoadSound(string id)
        {
            if (eventDescriptions.ContainsKey(id)) return;

            EventDescription eventDescription = RuntimeManager.GetEventDescription(id);
            eventDescriptions.Add(id, eventDescription);
            eventDescription.loadSampleData();
            eventDescription.createInstance(out EventInstance instance);
            playOnceInstances.Add(id, instance);
        }

        public void UnloadSound(string id)
        {
            if (!eventDescriptions.ContainsKey(id)) return;

            eventDescriptions[id].releaseAllInstances();
            eventDescriptions[id].unloadSampleData();
            eventDescriptions.Remove(id);
            playOnceInstances[id].release();
            playOnceInstances.Remove(id);
        }

        public void PlayOnce(string id)
        {
            LoadSound(id);
            EventInstance sound = playOnceInstances[id];
            sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sound.start();
        }

        public EventInstance GetSoundInstance(string id)
        {
            LoadSound(id);
            eventDescriptions[id].createInstance(out EventInstance instance);
            return instance;
        }

        public ParameterBinding RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
        {
            ParameterBinding binding = ParameterBinding.Local(eventInstance, parameterName, valueGetter);
            parameterUpdates.Add(binding);
            return binding;
        }

        public ParameterBinding RegisterGlobalParameter(string parameterName, Func<float> valueGetter)
        {
            ParameterBinding binding = ParameterBinding.Global(parameterName, valueGetter);
            parameterUpdates.Add(binding);
            return binding;
        }

        public void SetGlobalParameter(string parameterName, float value)
        {
            RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
        }

        public void SetMasterVolume(float volume)
        {
            Settings.MasterVolume = Math.Clamp(volume, 0f, 1f);
            Settings.Save();
            RuntimeManager.GetBus("bus:/").setVolume(Settings.MasterVolume);
        }

        public void SetMusicVolume(float volume)
        {
            Settings.MusicVolume = Math.Clamp(volume, 0f, 1f);
            Settings.Save();
            RuntimeManager.GetBus("bus:/Music").setVolume(Settings.MusicVolume);
        }

        public void SetSfxVolume(float volume)
        {
            Settings.SfxVolume = Math.Clamp(volume, 0f, 1f);
            Settings.Save();
            RuntimeManager.GetBus("bus:/Sounds").setVolume(Settings.SfxVolume);
        }

        public void ResetGlobalParameters()
        {
            foreach (KeyValuePair<string, float> pair in Sounds.DefaultGlobalParameterValues)
            {
                SetGlobalParameter(pair.Key, pair.Value);
            }
        }

        public void Dispose()
        {
            foreach (string id in new List<string>(eventDescriptions.Keys))
            {
                UnloadSound(id);
            }
            parameterUpdates.Clear();
        }

        // Advances every registered parameter binding. Call once per frame
        // (see SoundServiceRunner) - this replaces the MonoGame
        // SoundService's IGameSystem.Update(GameTime) hook, which has no
        // Unity equivalent.
        public void Tick()
        {
            parameterUpdates.RemoveAll(binding =>
            {
                if (!binding.active) return true;
                try
                {
                    binding.Update();
                    return false;
                }
                catch (Exception)
                {
                    binding.Deactivate();
                    return true;
                }
            });
        }
    }
}
