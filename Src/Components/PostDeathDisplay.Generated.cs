//Code for PostDeathDisplay (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace Gamelab.Components;
partial class PostDeathDisplay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("PostDeathDisplay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named PostDeathDisplay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PostDeathDisplay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PostDeathDisplay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("PostDeathDisplay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }

    public PostDeathDisplay(InteractiveGue visual) : base(visual)
    {
    }
    public PostDeathDisplay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
