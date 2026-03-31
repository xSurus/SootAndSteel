using System.ComponentModel.DataAnnotations;
using Gamelab.Events;
using Microsoft.Xna.Framework;

namespace Gamelab.Map.Train.State;

public class GameplayContext(TrainMap map, TrainState state, GameEvents gameEvents, Point virtualScreenSize)
{
    public TrainMap Map { get; } = map;
    public TrainState State { get; } = state;
    public GameEvents Events { get; } = gameEvents;
    public int ScreenWidth { get; } = virtualScreenSize.X;
    public int ScreenHeight { get; } = virtualScreenSize.Y;
}