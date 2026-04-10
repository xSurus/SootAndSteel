using System;
using System.Collections.Generic;
using Gamelab.Events;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map.Train.State;

public class GameplayContext(Point virtualScreenSize)
{
    public World PhysicsWorld { get; } = new World(Vector2.Zero);
    public TrainMap Map { get; set; }
    public TrainState State { get; } = new TrainState();
    public GameEvents Events { get; } = new GameEvents();
    public int ScreenWidth { get; } = virtualScreenSize.X;
    public int ScreenHeight { get; } = virtualScreenSize.Y;

    private readonly Queue<Action> _deferredActions = new();

    /// <summary>
    /// Enqueues an action that mutates physics state (e.g. Body.Enabled) to run after
    /// the current <see cref="World.Step"/> completes. Safe to call from collision callbacks.
    /// </summary>
    public void DeferPhysicsAction(Action action) => _deferredActions.Enqueue(action);

    /// <summary>Call immediately after <see cref="World.Step"/> to flush deferred mutations.</summary>
    public void FlushDeferredPhysicsActions()
    {
        while (_deferredActions.TryDequeue(out var action))
            action();
    }
}