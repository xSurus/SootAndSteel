using System.Collections.Generic;
using System.Linq;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.UI;
using MonoGameGum;

namespace Gamelab.Components
{
    public partial class ControlsOverlay
    {
        private ISoundService soundService;

        partial void CustomInitialize()
        {
            XboxButtonGlyphs.ApplyFaceButton(CloseButton, XboxButtonAtlas.Face.A);
            this.AddToRoot();
            Visual.Visible = false;

            soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
            soundService.LoadSound(Sounds.MenuSelect);
        }

        public void Update(IReadOnlyList<PlayerConfiguration> players)
        {
            if (players.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsPauseJustPressed()))
            {
                soundService.PlayOnce(Sounds.MenuSelect);
                ClosePanel();
            }
        }

        public void OpenPanel() => Visual.Visible = true;
        public void ClosePanel() => Visual.Visible = false;
        public bool IsOpen => Visual.Visible;
    }
}