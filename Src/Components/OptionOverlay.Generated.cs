//Code for OptionOverlay (Container)
using Gamelab.Components;
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
partial class OptionOverlay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("OptionsOverlay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named OptionsOverlay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new OptionOverlay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(OptionOverlay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("OptionsOverlay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ColoredRectangleRuntime Vigniette { get; protected set; }
    public OptionsMenu OptionsMenuInstance { get; protected set; }

    public OptionOverlay(InteractiveGue visual) : base(visual)
    {
    }
    public OptionOverlay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Vigniette = this.Visual?.GetGraphicalUiElementByName("Vigniette") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        OptionsMenuInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<OptionsMenu>(this.Visual,"OptionsMenuInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
