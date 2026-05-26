using System;

namespace Gamelab.Events;

public class GameEvents
{
    // Train events
    public event Action OnWallBreached;
    public event Action OnWallRepaired;
    public event Action OnCannonFired;

    public void FireWallBreached()
    {
        OnWallBreached?.Invoke();
    }

    public void FireWallRepaired()
    {
        OnWallRepaired?.Invoke();
    }

    public void FireCannonFired()
    {
        OnCannonFired?.Invoke();
    }
}