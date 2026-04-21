using System;
using Gamelab.PhysicalEntities.Projectiles;

namespace Gamelab.Events;

public class GameEvents
{
    // Train events
    public event Action OnWallBreached;
    public event Action OnWallRepaired;
    
    public void FireWallBreached()
    {
        OnWallBreached?.Invoke();
    }
    public void FireWallRepaired()
    {
        OnWallRepaired?.Invoke();
    }
}
