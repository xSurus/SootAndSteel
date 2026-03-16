using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gamelab.Services;

public class ServiceManager
{
    private readonly List<IGameService> services = [];
    private bool isInitialized;

    public void Add(IGameService service)
    {
        services.Add(service);
    }

    public void InitializeAll(GamelabGame game)
    {
        if (isInitialized)
        {
            return;
        }

        foreach (var service in services)
        {
            service.Initialize(game);
        }

        isInitialized = true;
    }

    public void UpdateAll(GameTime gameTime)
    {
        if (!isInitialized)
        {
            return;
        }

        foreach (var service in services)
        {
            service.Update(gameTime);
        }
    }

    public void DrawAll()
    {
        if (!isInitialized)
        {
            return;
        }

        foreach (var service in services)
        {
            service.Draw();
        }
    }

    public void ShutdownAll()
    {
        if (!isInitialized)
        {
            return;
        }

        for (int i = services.Count - 1; i >= 0; i--)
        {
            services[i].Shutdown();
        }

        isInitialized = false;
    }
}

