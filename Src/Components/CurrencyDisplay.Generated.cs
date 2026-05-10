//Code for CurrencyDisplay (Container)
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
partial class CurrencyDisplay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("CurrencyDisplay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named CurrencyDisplay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new CurrencyDisplay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(CurrencyDisplay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("CurrencyDisplay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public SpriteRuntime CurrencyIcon { get; protected set; }
    public TextRuntime Amount { get; protected set; }

    public string AmountText
    {
        get => Amount.Text;
        set => Amount.Text = value;
    }

    public CurrencyDisplay(InteractiveGue visual) : base(visual)
    {
    }
    public CurrencyDisplay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        CurrencyIcon = this.Visual?.GetGraphicalUiElementByName("CurrencyIcon") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        Amount = this.Visual?.GetGraphicalUiElementByName("Amount") as global::MonoGameGum.GueDeriving.TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
