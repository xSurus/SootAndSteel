//Code for Slider (Container)
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
partial class Slider : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Slider");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Slider - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new Slider(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(Slider)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Slider", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum SoundLevel
    {
    }

    SoundLevel? _soundLevelState;
    public SoundLevel? SoundLevelState
    {
        get => _soundLevelState;
        set
        {
            _soundLevelState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("SoundLevel"))
                {
                    var category = Visual.Categories["SoundLevel"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "SoundLevel");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public ColoredRectangleRuntime ColoredRectangleInstance1 { get; protected set; }
    public RectangleRuntime Rectangle { get; protected set; }

    public float ColoredRectangleInstance1X
    {
        get => ColoredRectangleInstance1.X;
        set => ColoredRectangleInstance1.X = value;
    }

    public Slider(InteractiveGue visual) : base(visual)
    {
    }
    public Slider()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        ColoredRectangleInstance1 = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance1") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        Rectangle = this.Visual?.GetGraphicalUiElementByName("Rectangle") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
