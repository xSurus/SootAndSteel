//Code for PauseOverlay (Container)
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
partial class PauseOverlay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("PauseOverlay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named PauseOverlay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PauseOverlay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PauseOverlay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("PauseOverlay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ColoredRectangleRuntime Vigniette { get; protected set; }
    public MenuButtonWithIcon Continue { get; protected set; }
    public MenuButtonWithIcon Options { get; protected set; }
    public MenuButtonWithIcon Controls { get; protected set; }
    public MenuButtonWithIcon Exit { get; protected set; }
    public TextRuntime Pause { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public TextRuntime ItemFunction { get; protected set; }
    public ContainerRuntime MenuItemContainer { get; protected set; }
    public SpriteRuntime Background { get; protected set; }

    public PauseOverlay(InteractiveGue visual) : base(visual)
    {
    }
    public PauseOverlay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Vigniette = this.Visual?.GetGraphicalUiElementByName("Vigniette") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        Continue = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuButtonWithIcon>(this.Visual,"Continue");
        Options = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuButtonWithIcon>(this.Visual,"Options");
        Controls = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuButtonWithIcon>(this.Visual,"Controls");
        Exit = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuButtonWithIcon>(this.Visual,"Exit");
        Pause = this.Visual?.GetGraphicalUiElementByName("Pause") as global::MonoGameGum.GueDeriving.TextRuntime;
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        ItemFunction = this.Visual?.GetGraphicalUiElementByName("ItemFunction") as global::MonoGameGum.GueDeriving.TextRuntime;
        MenuItemContainer = this.Visual?.GetGraphicalUiElementByName("MenuItemContainer") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
