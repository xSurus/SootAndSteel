using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Interactable.Stations.Resources;

public abstract class AbstractResource(string stationType, Color stationColor, string resourceId, Color resourceColor)
    : AbstractStation(stationType, stationColor)
{
    protected string ResourceId { get; } = resourceId;
    protected Color ResourceColor { get; } = resourceColor;

    public override void OnGrabAction(Player interactingPlayer, TrainContext trainContext)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new Item(ResourceId, ResourceColor);
        }
        else if (interactingPlayer.HeldItem.Id == ResourceId)
        {
            interactingPlayer.HeldItem = null;
        }
    }
}