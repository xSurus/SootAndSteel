using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gamelab.Systems;

public class SystemManager
{
    private readonly List<IGameSystem> systems = [];
    private bool isInitialized;

    public void Add(IGameSystem system)
    {
        systems.Add(system);
    }

    public void InitializeAll(GamelabGame game)
    {
        if (isInitialized)
        {
            return;
        }

        foreach (var system in systems)
        {
            system.Initialize(game);
        }

        isInitialized = true;
    }

    public void UpdateAll(GameTime gameTime)
    {
        if (!isInitialized)
        {
            return;
        }

        foreach (var system in systems)
        {
            system.Update(gameTime);
        }
    }

    public void DrawAll()
    {
        if (!isInitialized)
        {
            return;
        }

        foreach (var system in systems)
        {
            system.Draw();
        }
    }

    public void ShutdownAll()
    {
        if (!isInitialized)
        {
            return;
        }

        for (int i = systems.Count - 1; i >= 0; i--)
        {
            systems[i].Shutdown();
        }

        isInitialized = false;
    }
}

