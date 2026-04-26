//Code for ShopItemType (Container)
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
partial class ShopItemType : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("ShopItemType");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named ShopItemType - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new ShopItemType(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(ShopItemType)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("ShopItemType", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ContainerRuntime Wrapper { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public string NineSliceInstanceSourceFile
    {
        set => Background.SourceFileName = value;
    }

    public string ShopItemTypeIcon
    {
        set => SpriteInstance.SourceFileName = value;
    }

    public string ShopItemTypeName
    {
        get => TextInstance.Text;
        set => TextInstance.Text = value;
    }

    public ShopItemType(InteractiveGue visual) : base(visual)
    {
    }
    public ShopItemType()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Wrapper = this.Visual?.GetGraphicalUiElementByName("Wrapper") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
