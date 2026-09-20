namespace Gamelab.PhysicalEntities.Interfaces
{
    // Stand-in for Src SeatedPlayer.PlayerConfiguration.Input.GetMovement(). Null source on the
    // cannon means nobody is seated (Src SeatedPlayer == null), so no aiming.
    public interface ICannonAimSource
    {
        System.Numerics.Vector2 GetMovement();
    }
}
