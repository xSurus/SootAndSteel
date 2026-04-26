//Code for MainMenuButton (Container)
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
partial class MainMenuButton : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("MainMenuButton");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named MainMenuButton - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new MainMenuButton(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(MainMenuButton)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("MainMenuButton", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum Selection
    {
        Selected,
        Unselected,
    }

    Selection? _selectionState;
    public Selection? SelectionState
    {
        get => _selectionState;
        set
        {
            _selectionState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("Selection"))
                {
                    var category = Visual.Categories["Selection"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "Selection");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public ColoredRectangleRuntime ColoredRectangleInstance { get; protected set; }
    public TextRuntime Lable { get; protected set; }

    public string ButtonText
    {
        get => Lable.Text;
        set => Lable.Text = value;
    }

    public MainMenuButton(InteractiveGue visual) : base(visual)
    {
    }
    public MainMenuButton()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        ColoredRectangleInstance = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        Lable = this.Visual?.GetGraphicalUiElementByName("Lable") as global::MonoGameGum.GueDeriving.TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
