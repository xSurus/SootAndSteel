using UnityEngine;

namespace Gamelab.Services.Sound
{
    // Drives SoundService.Tick() once per frame and gives the rest of the
    // game a single place to get the sound service from. The MonoGame
    // original hooked this through IGameSystem.Update(GameTime); Unity has
    // no equivalent global system registry, so a single scene-persistent
    // MonoBehaviour does the same job.
    public class SoundServiceRunner : MonoBehaviour
    {
        public static SoundServiceRunner Instance { get; private set; }
        public SoundService SoundService { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SoundService = new SoundService();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SoundService?.Dispose();
            Instance = null;
        }

        private void Update()
        {
            SoundService?.Tick();
        }
    }
}
