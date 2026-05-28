//Code for IngameHUD/Speedometer (Container)
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
partial class Speedometer : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("IngameHUD/Speedometer");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named IngameHUD/Speedometer - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new Speedometer(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(Speedometer)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("IngameHUD/Speedometer", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public SpriteRuntime Needle { get; protected set; }
    public ContainerRuntime NeedleContainer { get; protected set; }

    public float NeedleContainerRotation
    {
        get => NeedleContainer.Rotation;
        set => NeedleContainer.Rotation = value;
    }

    public Speedometer(InteractiveGue visual) : base(visual)
    {
    }
    public Speedometer()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Needle = this.Visual?.GetGraphicalUiElementByName("Needle") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        NeedleContainer = this.Visual?.GetGraphicalUiElementByName("NeedleContainer") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
