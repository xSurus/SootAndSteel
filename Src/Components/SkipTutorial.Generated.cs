//Code for SkipTutorial (Container)
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
partial class SkipTutorial : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("SkipTutorial");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named SkipTutorial - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new SkipTutorial(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(SkipTutorial)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("SkipTutorial", () => 
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
    public ColoredRectangleRuntime ProgressBarBackground { get; protected set; }
    public ColoredRectangleRuntime ProgressBarProgress { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }
    public ContainerRuntime ProgressBar { get; protected set; }

    public string TextInstanceText
    {
        get => TextInstance.Text;
        set => TextInstance.Text = value;
    }

    public SkipTutorial(InteractiveGue visual) : base(visual)
    {
    }
    public SkipTutorial()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        ProgressBarBackground = this.Visual?.GetGraphicalUiElementByName("ProgressBarBackground") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        ProgressBarProgress = this.Visual?.GetGraphicalUiElementByName("ProgressBarProgress") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        ProgressBar = this.Visual?.GetGraphicalUiElementByName("ProgressBar") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
