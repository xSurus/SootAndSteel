using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations.Resources;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class GunpowderResource(Vector2 position, GameplayContext gameplayContext) : AbstractResource("GunpowderResource", Color.DarkGray, "Gunpowder", position, gameplayContext);