using UnityEngine;

namespace Gamelab.Map.Train
{
    /// <summary>Src DoorWall physics: open turns the collider into a sensor (Src fixture.IsSensor).</summary>
    public class DoorWallRuntime : MonoBehaviour
    {
        public bool IsTop { get; set; }
        public bool IsOpen { get; private set; }

        /// <summary>Overlay showing the open or closed door art. Set by the map builder.</summary>
        public SpriteRenderer StateRenderer { get; set; }

        public void Toggle()
        {
            IsOpen = !IsOpen;
            GetComponent<BoxCollider2D>().isTrigger = IsOpen;
            if (StateRenderer != null)
                StateRenderer.sprite = MapSprites.Get(IsOpen ? "Walls/WallTileTopDoorOpen" : "Walls/WallTileTopDoorClosed");
        }
    }
}
