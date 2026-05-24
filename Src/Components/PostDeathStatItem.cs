using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace Gamelab.Components
{
    partial class PostDeathStatItem
    {
        partial void CustomInitialize()
        {
        
        }

        public void SetValue(string text)
        {
            StatText = text;
        }
    }
}
