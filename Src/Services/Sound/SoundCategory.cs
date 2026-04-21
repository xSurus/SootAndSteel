namespace Gamelab.Services.Sound;

/// Logical category for an FMOD event. Used by the sound service to scale event
/// instances' volumes according to the user-facing sliders (Master, Music, SFX),
/// independent of how the FMOD project routes its buses internally.
public enum SoundCategory
{
    Master,
    Music,
    Sfx,
}
