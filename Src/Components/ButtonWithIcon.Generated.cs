//Code for ButtonWithIcon (Container)
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

public partial class ButtonWithIcon : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("ButtonWithIcon");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named ButtonWithIcon - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new ButtonWithIcon(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(ButtonWithIcon)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("ButtonWithIcon", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum Affordability
    {
        canAfford,
        cantAfford,
    }

    Affordability? _affordabilityState;
    public Affordability? AffordabilityState
    {
        get => _affordabilityState;
        set
        {
            _affordabilityState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("Affordability"))
                {
                    var category = Visual.Categories["Affordability"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "Affordability");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public TextRuntime TextInstance { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public string ButtonIcon
    {
        set => SpriteInstance.SourceFileName = value;
    }

    public int TextInstanceBlue
    {
        get => TextInstance.Blue;
        set => TextInstance.Blue = value;
    }

    public int TextInstanceGreen
    {
        get => TextInstance.Green;
        set => TextInstance.Green = value;
    }

    public int TextInstanceRed
    {
        get => TextInstance.Red;
        set => TextInstance.Red = value;
    }

    public string ButtonText
    {
        get => TextInstance.Text;
        set => TextInstance.Text = value;
    }

    public ButtonWithIcon(InteractiveGue visual) : base(visual)
    {
    }
    public ButtonWithIcon()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
