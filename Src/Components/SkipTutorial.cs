using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Attributes;

namespace Gamelab.Components
{
    partial class SkipTutorial
    {
        private float MaxWidth;
        
        partial void CustomInitialize()
        {
            MaxWidth = ProgressBar.GetAbsoluteWidth();
        }

        public void SetProgress(float progress)
        {
            ProgressBar.Width = MathHelper.Lerp(0, MaxWidth, progress);
        }
    }
}
