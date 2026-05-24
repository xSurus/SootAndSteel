using System.Collections.Generic;

namespace Gamelab.Services.Sound;

public static class Sounds
{
    //TODO: Maybe add enemy grunt on hit
    // SFX
    public const string MenuSelect = "event:/UI/Menu Select";
    public const string Stamp = "event:/UI/Stamp";
    public const string Train = "event:/Gameplay/Train";
    public const string ShovelUp = "event:/Gameplay/Shovel Up";
    public const string ShovelDown = "event:/Gameplay/Shovel Down";
    public const string CannonLoad = "event:/Gameplay/Cannon Load";
    public const string CannonFire = "event:/Gameplay/Cannon Fire";
    public const string Craft =  "event:/Gameplay/Craft";
    public const string WallHit = "event:/Gameplay/Wall Hit";
    public const string WallBreak = "event:/Gameplay/Wall Break";
    public const string WallFix = "event:/Gameplay/Wall Fix";
    public const string WallFixed = "event:/Gameplay/Wall Fixed";
    public const string Walk = "event:/Gameplay/Walk";
    public const string PickupItem = "event:/Gameplay/Pickup Item";
    public const string DropItem = "event:/Gameplay/Drop Item";
    public const string OpenDoor = "event:/Gameplay/Open Door";
    public const string CloseDoor = "event:/Gameplay/Close Door";
    public const string SpeedChange = "event:/Gameplay/Speed Change";
    public const string Purchase =  "event:/Gameplay/Purchase";
    public const string GrabStation =  "event:/Gameplay/Grab Station";
    public const string DropStation =  "event:/Gameplay/Drop Station";
    public const string Freeze = "event:/Gameplay/Freeze";
    public const string Fall = "event:/Gameplay/Fall";
    public const string HorseRiding = "event:/Gameplay/Enemy/Horse Riding";
    public const string HorseFlee = "event:/Gameplay/Enemy/Horse Flee";
    public const string EnemyHit = "event:/Gameplay/Enemy/Enemy Hit";
    public const string EnemyFire = "event:/Gameplay/Enemy/Enemy Fire";
    public const string GameOver = "event:/Gameplay/Game Over";
    
    // Music
    public const string AmbientSong = "event:/Music/Ambient";
    public const string BattleTheme = "event:/Music/Battle Theme";
    
    // Default Global Parameter Values
    public static readonly List<KeyValuePair<string, float>> DefaultGlobalParameterValues = [
        new ("Temperature", 1f)
    ];
}