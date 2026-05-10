//Code for HubOverlay (Container)
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
partial class HubOverlay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("HubOverlay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named HubOverlay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new HubOverlay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(HubOverlay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("HubOverlay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public PlayersReady PlayersReadyInstance { get; protected set; }
    public CurrencyDisplay CurrencyDisplayInstance { get; protected set; }

    public PlayersReady.Ready? PlayersReadyInstanceReadyState
    {
        get => PlayersReadyInstance.ReadyState;
        set => PlayersReadyInstance.ReadyState = value;
    }

    public HubOverlay(InteractiveGue visual) : base(visual)
    {
    }
    public HubOverlay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        PlayersReadyInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PlayersReady>(this.Visual,"PlayersReadyInstance");
        CurrencyDisplayInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<CurrencyDisplay>(this.Visual,"CurrencyDisplayInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
