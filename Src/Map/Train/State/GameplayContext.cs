using System;
using System.Collections.Generic;
using Gamelab.Events;
using Gamelab.Serialization;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map.Train.State;

public class GameplayContext(Point virtualScreenSize, RunSession runSession) : IDisposable
{
    private bool _physicsLocked;
    private readonly Queue<Action> _deferredPhysicsActions = new();

    public World PhysicsWorld { get; } = new World(Vector2.Zero);
    public TrainMap Map { get; set; }
    public TrainState State { get; } = new TrainState(runSession);
    public GameEvents Events { get; } = new GameEvents();
    public int ScreenWidth { get; } = virtualScreenSize.X;
    public int ScreenHeight { get; } = virtualScreenSize.Y;
    public int WorldHeight { get; set; }
    public PatchManager PatchManager { get; set; }

    /// <summary>Called immediately before <see cref="World.Step(float)"/>.</summary>
    public void BeginPhysicsStep() => _physicsLocked = true;

    /// <summary>Called immediately after <see cref="World.Step(float)"/>; runs queued world mutations.</summary>
    public void EndPhysicsStep()
    {
        _physicsLocked = false;
        while (_deferredPhysicsActions.TryDequeue(out Action action))
            action();
    }

    /// <summary>Use for world mutations (e.g. <see cref="World.Remove"/>) that may run from contact callbacks.</summary>
    public void DeferOrExecutePhysicsAction(Action action)
    {
        if (_physicsLocked)
            _deferredPhysicsActions.Enqueue(action);
        else
            action();
    }

    public void Dispose()
    {
        State?.Dispose();
    }
}
