using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Cannon;

public class CannonStation : AbstractStation
{
    public CannonAimingBar AimingBar { get; private set; }

    public CannonStation(Vector2 position, TrainContext trainContext)
        : base("Cannon", Color.DarkRed, position, trainContext)
    {
        AimingBar = new CannonAimingBar(PhysicsBody, position.ToMeters(), trainContext);
    }

    public override void OnInteract(Player interactingPlayer, TrainContext context)
    {
        // TODO
    }

    private void FireCannon()
    {
        // TODO
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        AimingBar?.Draw(spriteBatch);
    }
}