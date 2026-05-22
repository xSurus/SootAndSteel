//Code for IngameHUD/IngameHudOverlay (Container)
using Gamelab.Components.IngameHUD;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace Gamelab.Components.IngameHUD;
partial class IngameHudOverlay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("IngameHUD/IngameHudOverlay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named IngameHUD/IngameHudOverlay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new IngameHudOverlay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(IngameHudOverlay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("IngameHUD/IngameHudOverlay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public DistanceTravelledDisplay DistanceTravelledDisplayInstance { get; protected set; }
    public Speedometer SpeedometerInstance { get; protected set; }

    public IngameHudOverlay(InteractiveGue visual) : base(visual)
    {
    }
    public IngameHudOverlay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        DistanceTravelledDisplayInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<DistanceTravelledDisplay>(this.Visual,"DistanceTravelledDisplayInstance");
        SpeedometerInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Speedometer>(this.Visual,"SpeedometerInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
