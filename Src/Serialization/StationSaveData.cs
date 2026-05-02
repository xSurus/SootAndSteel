using Gamelab.Utils;

namespace Gamelab.Serialization;

public record StationSaveData(string KindId, int TileX, int TileY, GridDirection FacingDirection);