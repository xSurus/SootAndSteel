//Code for ToolTip (Container)
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
partial class ToolTip : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("ToolTip");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named ToolTip - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new ToolTip(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(ToolTip)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("ToolTip", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public RectangleRuntime Seperator1 { get; protected set; }
    public RectangleRuntime Seperator2 { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance1 { get; protected set; }
    public TextRuntime ItemFunction { get; protected set; }
    public TextRuntime ItemName { get; protected set; }
    public ShopItemType ShopItemTypeInstance { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public ContainerRuntime Wrapper { get; protected set; }
    public ContainerRuntime Interactions { get; protected set; }

    public string Functionality
    {
        get => ItemFunction.Text;
        set => ItemFunction.Text = value;
    }

    public string Name
    {
        get => ItemName.Text;
        set => ItemName.Text = value;
    }

    public string ShopItemTypeIcon
    {
        set => ShopItemTypeInstance.ShopItemTypeIcon = value;
    }

    public string ItemDescription
    {
        get => TextInstance.Text;
        set => TextInstance.Text = value;
    }

    public ToolTip(InteractiveGue visual) : base(visual)
    {
    }
    public ToolTip()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Seperator1 = this.Visual?.GetGraphicalUiElementByName("Seperator1") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        Seperator2 = this.Visual?.GetGraphicalUiElementByName("Seperator2") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        ButtonWithIconInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance");
        ButtonWithIconInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance1");
        ItemFunction = this.Visual?.GetGraphicalUiElementByName("ItemFunction") as global::MonoGameGum.GueDeriving.TextRuntime;
        ItemName = this.Visual?.GetGraphicalUiElementByName("ItemName") as global::MonoGameGum.GueDeriving.TextRuntime;
        ShopItemTypeInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ShopItemType>(this.Visual,"ShopItemTypeInstance");
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        Wrapper = this.Visual?.GetGraphicalUiElementByName("Wrapper") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Interactions = this.Visual?.GetGraphicalUiElementByName("Interactions") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
