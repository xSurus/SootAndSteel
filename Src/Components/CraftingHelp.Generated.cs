//Code for CraftingHelp (Container)
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
partial class CraftingHelp : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("CraftingHelp");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named CraftingHelp - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new CraftingHelp(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(CraftingHelp)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("CraftingHelp", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime TextInstance { get; protected set; }
    public TextRuntime CraftingRecipeDescription { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public ContainerRuntime Wrapper { get; protected set; }

    public CraftingHelp(InteractiveGue visual) : base(visual)
    {
    }
    public CraftingHelp()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        CraftingRecipeDescription = this.Visual?.GetGraphicalUiElementByName("CraftingRecipeDescription") as global::MonoGameGum.GueDeriving.TextRuntime;
        ButtonWithIconInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance");
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        Wrapper = this.Visual?.GetGraphicalUiElementByName("Wrapper") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
