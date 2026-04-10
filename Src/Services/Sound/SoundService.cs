using System;
using System.Collections.Generic;
using System.IO;
using FmodForFoxes;
using FmodForFoxes.Studio;
using Gamelab.Systems;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;

namespace Gamelab.Services.Sound;

public class SoundService : ISoundService,
    IDisposable,
    IGameSystem
{
    private readonly Logger logger = new ("SoundService");
    private readonly List<Bank> banks = [];
    private readonly List<ParameterBinding> parameterUpdates = [];
    private readonly Dictionary<string, EventDescription> eventDescriptions = new ();
    
    private class ParameterBinding(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
    {
        public EventInstance EventInstance { get; } = eventInstance;
        public string ParameterName { get; } = parameterName;
        public Func<float> ValueGetter { get; } = valueGetter;
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
        sound.Start();
        sound.Dispose();
    }

    public EventInstance GetSoundInstance(string id)
    {
        LoadSound(id);
        return eventDescriptions[id].CreateInstance();
    }

    public void RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
    {
        parameterUpdates.Add(new ParameterBinding(eventInstance, parameterName, valueGetter));
    }

    public void Initialize(GamelabGame game)
    {
        FmodManager.Init(game.nativeFmodLibrary, FmodInitMode.CoreAndStudio, Path.Combine(game.Content.RootDirectory, "soundbanks"));
        banks.Add(StudioSystem.LoadBank("Master.bank"));
        banks.Add(StudioSystem.LoadBank("Master.strings.bank"));
        banks.Add(StudioSystem.LoadBank("sfx.bank"));
        banks.Add(StudioSystem.LoadBank("music.bank"));
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
        FmodManager.Unload();
    }
}