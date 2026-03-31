using System;
using Gamelab.PhysicalEntities.Projectiles;

namespace Gamelab.Events;

/// <summary>
/// Central event bus for game-wide communication between decoupled systems.
/// 
/// PURPOSE:
/// - Allows systems to communicate without direct references to each other
/// - Publishers fire events without knowing who listens
/// - Subscribers react to events without knowing who fired them
/// 
/// USAGE:
/// - Systems that CAUSE actions call the Fire methods (e.g., Cannon fires projectile)
/// - Systems that REACT to actions subscribe to events (e.g., EnemyManager tracks projectiles)
/// - GameplayScreen creates this and passes it to systems that need it
/// 
/// EXAMPLES:
/// - Cannon fires → GameEvents.FireCannonProjectile() → EnemyManager receives projectile
/// - Enemy breaks a wall → GameEvents.FireWallBreached() → Screen shake, UI update, sound effect
/// 
/// This pattern keeps systems decoupled:
/// - Train/Stations don't know about Enemies
/// - Enemies don't know about UI/Sound
/// - Each system only knows about GameEvents
/// </summary>
public class GameEvents
{
    // Projectile events
    public event Action<CannonProjectile> OnCannonProjectileFired;
    
    // Train events
    public event Action OnWallBreached;
    public event Action OnWallRepaired;
    
    public void FireCannonProjectile(CannonProjectile projectile)
    {
        OnCannonProjectileFired?.Invoke(projectile);
    }
    
    public void FireWallBreached()
    {
        OnWallBreached?.Invoke();
    }
    public void FireWallRepaired()
    {
        OnWallRepaired?.Invoke();
    }
}
