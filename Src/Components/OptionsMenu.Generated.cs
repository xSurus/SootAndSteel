//Code for OptionsMenu (Container)
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
partial class OptionsMenu : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("OptionsMenu");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named OptionsMenu - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new OptionsMenu(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(OptionsMenu)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("OptionsMenu", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public MenuItemAdjustable Master { get; protected set; }
    public MenuItemAdjustable Ambient { get; protected set; }
    public MenuItemAdjustable Sound_Effects { get; protected set; }
    public TextRuntime Pause { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public TextRuntime ItemFunction { get; protected set; }
    public ContainerRuntime MenuItemContainer { get; protected set; }
    public SpriteRuntime Background { get; protected set; }

    public OptionsMenu(InteractiveGue visual) : base(visual)
    {
    }
    public OptionsMenu()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Master = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuItemAdjustable>(this.Visual,"Master");
        Ambient = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuItemAdjustable>(this.Visual,"Ambient");
        Sound_Effects = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MenuItemAdjustable>(this.Visual,"Sound Effects");
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
