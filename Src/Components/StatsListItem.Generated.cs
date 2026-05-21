//Code for StatsListItem (Container)
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
partial class StatsListItem : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("StatsListItem");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named StatsListItem - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new StatsListItem(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(StatsListItem)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("StatsListItem", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime Index { get; protected set; }
    public TextRuntime Amount { get; protected set; }
    public TextRuntime ItemLable { get; protected set; }
    public TextRuntime ItemDescription { get; protected set; }

    public string AmountText
    {
        get => Amount.Text;
        set => Amount.Text = value;
    }

    public string IndexText
    {
        get => Index.Text;
        set => Index.Text = value;
    }

    public string ItemDescriptionText
    {
        get => ItemDescription.Text;
        set => ItemDescription.Text = value;
    }

    public string ItemLableText
    {
        get => ItemLable.Text;
        set => ItemLable.Text = value;
    }

    public StatsListItem(InteractiveGue visual) : base(visual)
    {
    }
    public StatsListItem()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Index = this.Visual?.GetGraphicalUiElementByName("Index") as global::MonoGameGum.GueDeriving.TextRuntime;
        Amount = this.Visual?.GetGraphicalUiElementByName("Amount") as global::MonoGameGum.GueDeriving.TextRuntime;
        ItemLable = this.Visual?.GetGraphicalUiElementByName("ItemLable") as global::MonoGameGum.GueDeriving.TextRuntime;
        ItemDescription = this.Visual?.GetGraphicalUiElementByName("ItemDescription") as global::MonoGameGum.GueDeriving.TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
