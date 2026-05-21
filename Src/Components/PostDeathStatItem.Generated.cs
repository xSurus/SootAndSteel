//Code for PostDeathStatItem (Container)
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
partial class PostDeathStatItem : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("PostDeathStatItem");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named PostDeathStatItem - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PostDeathStatItem(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PostDeathStatItem)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("PostDeathStatItem", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime Stat { get; protected set; }
    public TextRuntime Dots { get; protected set; }

    public string StatText
    {
        get => Stat.Text;
        set => Stat.Text = value;
    }

    public PostDeathStatItem(InteractiveGue visual) : base(visual)
    {
    }
    public PostDeathStatItem()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Stat = this.Visual?.GetGraphicalUiElementByName("Stat") as global::MonoGameGum.GueDeriving.TextRuntime;
        Dots = this.Visual?.GetGraphicalUiElementByName("Dots") as global::MonoGameGum.GueDeriving.TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
