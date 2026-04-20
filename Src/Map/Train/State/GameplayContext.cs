using Gamelab.Events;
using Gamelab.Serialization;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map.Train.State;

public class GameplayContext(Point virtualScreenSize, RunSession runSession)
{
    public World PhysicsWorld { get; } = new World(Vector2.Zero);
    public TrainMap Map { get; set; }
    public TrainState State { get; } = new TrainState(runSession);
    public GameEvents Events { get; } = new GameEvents();
    public int ScreenWidth { get; } = virtualScreenSize.X;
    public int ScreenHeight { get; } = virtualScreenSize.Y;
    public int WorldHeight { get; set; }
}