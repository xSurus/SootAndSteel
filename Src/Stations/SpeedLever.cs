using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Stations;

public class SpeedLever : AbstractStation
{
    public SpeedLever() : base("SpeedLever", Color.LightGreen)
    {
    }

    public override void Interact(Player interactingPlayer, TrainContext trainContext)
    {
        if (!trainContext.State.IsCoalOvenBurning)
        {
            trainContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
            return;
        }

        var allSpeeds = TrainSpeedSetting.All;
        int currentIndex = allSpeeds.IndexOf(trainContext.State.CurrentSpeed);
        int nextIndex = (currentIndex + 1) % allSpeeds.Count;

        trainContext.State.CurrentSpeed = allSpeeds[nextIndex];
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, int tileSize)
    {
        base.Draw(spriteBatch, position, tileSize);

        string speedText = "??";
        Color textColor = Color.White;
    }
}