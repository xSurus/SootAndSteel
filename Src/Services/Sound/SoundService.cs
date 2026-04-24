using System;
using System.Collections.Generic;
using System.IO;
using FmodForFoxes;
using FmodForFoxes.Studio;
using Gamelab.Systems;
using Gamelab.Utils;
using Microsoft.Xna.Framework;

namespace Gamelab.Services.Sound;

public class SoundService : ISoundService,
    IDisposable,
    IGameSystem
{
    private readonly Logger logger = new("SoundService");
    private readonly List<Bank> banks = [];
    private readonly List<ParameterBinding> parameterUpdates = [];
    private readonly Dictionary<string, EventDescription> eventDescriptions = new();
    private static readonly Dictionary<string, SoundCategory> eventCategories = new()
    {
        [Sounds.MenuSelect] = SoundCategory.Sfx,
        [Sounds.Train] = SoundCategory.Sfx,
        [Sounds.AmbientSong] = SoundCategory.Music,
    };

    private readonly List<TrackedInstance> liveInstances = [];

    public SoundSettings Settings { get; private set; } = new();

    private class ParameterBinding(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
    {
        public EventInstance EventInstance { get; } = eventInstance;
        public string ParameterName { get; } = parameterName;
        public Func<float> ValueGetter { get; } = valueGetter;
    }

    private readonly struct TrackedInstance(EventInstance instance, SoundCategory category)
    {
        public EventInstance Instance { get; } = instance;
        public SoundCategory Category { get; } = category;
    }

    public void LoadSound(string id)
    {
        if (!eventDescriptions.ContainsKey(id))
        {
            logger.Debug($"Registering sound: {id}");
            var eventDescription = StudioSystem.GetEvent(id);
            eventDescriptions.Add(id, eventDescription);
            eventDescription.LoadSampleData();
        }
    }

    public void UnloadSound(string id)
    {
        logger.Debug($"Unregistering sound: {id}");
        if (!eventDescriptions.ContainsKey(id)) return;
        eventDescriptions[id].ReleaseAllInstances();
        eventDescriptions[id].UnloadSampleData();
        eventDescriptions.Remove(id);
    }

    public void PlayOnce(string id)
    {
        LoadSound(id);
        var sound = eventDescriptions[id].CreateInstance();
        sound.Volume = ComputeCategoryVolume(GetCategoryFor(id));
        sound.Start();
        sound.Dispose();
    }

    public EventInstance GetSoundInstance(string id)
    {
        LoadSound(id);
        var instance = eventDescriptions[id].CreateInstance();
        SoundCategory category = GetCategoryFor(id);
        instance.Volume = ComputeCategoryVolume(category);
        liveInstances.Add(new TrackedInstance(instance, category));
        return instance;
    }

    public void RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
    {
        parameterUpdates.Add(new ParameterBinding(eventInstance, parameterName, valueGetter));
    }

    public void SetMasterVolume(float volume)
    {
        Settings.MasterVolume = Math.Clamp(volume, 0f, 1f);
        ApplyVolumeToLiveInstances();
        Settings.Save();
    }

    public void SetMusicVolume(float volume)
    {
        Settings.MusicVolume = Math.Clamp(volume, 0f, 1f);
        ApplyVolumeToLiveInstances();
        Settings.Save();
    }

    public void SetSfxVolume(float volume)
    {
        Settings.SfxVolume = Math.Clamp(volume, 0f, 1f);
        ApplyVolumeToLiveInstances();
        Settings.Save();
    }

    public void Initialize(GamelabGame game)
    {
        FmodManager.Init(game.nativeFmodLibrary, FmodInitMode.CoreAndStudio,
            Path.Combine(game.Content.RootDirectory, "soundbanks"));
        banks.Add(StudioSystem.LoadBank("Master.bank"));
        banks.Add(StudioSystem.LoadBank("Master.strings.bank"));
        banks.Add(StudioSystem.LoadBank("sfx.bank"));
        banks.Add(StudioSystem.LoadBank("music.bank"));

        Settings = SoundSettings.Load();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var binding in parameterUpdates)
        {
            binding.EventInstance.SetParameterValue(binding.ParameterName, binding.ValueGetter());
        }

        FmodManager.Update();
    }

    public void Draw()
    {
    }

    public void Shutdown()
    {
        Dispose();
    }

    public void Dispose()
    {
        foreach (var id in eventDescriptions.Keys)
        {
            UnloadSound(id);
        }

        foreach (var bank in banks)
        {
            bank.Unload();
        }

        banks.Clear();
        parameterUpdates.Clear();
        eventDescriptions.Clear();
        liveInstances.Clear();
        FmodManager.Unload();
    }

    private static SoundCategory GetCategoryFor(string id) =>
        eventCategories.TryGetValue(id, out var category) ? category : SoundCategory.Master;

    private float ComputeCategoryVolume(SoundCategory category)
    {
        float volume = Settings.MasterVolume;
        volume *= category switch
        {
            SoundCategory.Music => Settings.MusicVolume,
            SoundCategory.Sfx => Settings.SfxVolume,
            _ => 1f,
        };
        return volume;
    }

    private void ApplyVolumeToLiveInstances()
    {
        liveInstances.RemoveAll(IsReleased);

        foreach (var tracked in liveInstances)
        {
            try
            {
                tracked.Instance.Volume = ComputeCategoryVolume(tracked.Category);
            }
            catch
            {
                // A disposed native handle would surface here; the next RemoveAll pass clears it.
            }
        }
    }

    private static bool IsReleased(TrackedInstance tracked)
    {
        try
        {
            return !tracked.Instance.Native.isValid();
        }
        catch
        {
            return true;
        }
    }
}
