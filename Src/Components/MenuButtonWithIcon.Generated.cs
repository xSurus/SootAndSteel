//Code for MenuButtonWithIcon (Container)
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
partial class MenuButtonWithIcon : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("MenuButtonWithIcon");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named MenuButtonWithIcon - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new MenuButtonWithIcon(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(MenuButtonWithIcon)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("MenuButtonWithIcon", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum Selected
    {
        isSelected,
        notSelected,
    }

    Selected? _selectedState;
    public Selected? SelectedState
    {
        get => _selectedState;
        set
        {
            _selectedState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("Selected"))
                {
                    var category = Visual.Categories["Selected"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "Selected");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public TextRuntime TextInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }

    public string TextInstanceText
    {
        get => TextInstance.Text;
        set => TextInstance.Text = value;
    }

    public MenuButtonWithIcon(InteractiveGue visual) : base(visual)
    {
    }
    public MenuButtonWithIcon()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
