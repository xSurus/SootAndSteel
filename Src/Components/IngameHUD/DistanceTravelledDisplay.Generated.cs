//Code for IngameHUD/DistanceTravelledDisplay (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace Gamelab.Components.IngameHUD;
partial class DistanceTravelledDisplay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("IngameHUD/DistanceTravelledDisplay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named IngameHUD/DistanceTravelledDisplay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new DistanceTravelledDisplay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(DistanceTravelledDisplay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("IngameHUD/DistanceTravelledDisplay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public RectangleRuntime RectangleInstance { get; protected set; }
    public TextRuntime DistanceVSMax { get; protected set; }
    public SpriteRuntime TrainSprite { get; protected set; }
    public ColoredRectangleRuntime MovingDistanceBox { get; protected set; }
    public ColoredRectangleRuntime ColoredRectangleInstance { get; protected set; }
    public ColoredRectangleRuntime ColoredRectangleInstance1 { get; protected set; }
    public ColoredRectangleRuntime ColoredRectangleInstance2 { get; protected set; }
    public ColoredRectangleRuntime ColoredRectangleInstance3 { get; protected set; }
    public ColoredRectangleRuntime ColoredRectangleInstance4 { get; protected set; }
    public ContainerRuntime DistanceContainer { get; protected set; }
    public NineSliceRuntime BackgroundParchement { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }
    public TextRuntime FinalDistance { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }
    public ContainerRuntime MovingTrain { get; protected set; }

    public string DistanceVSMaxText
    {
        get => DistanceVSMax.Text;
        set => DistanceVSMax.Text = value;
    }

    public string FinalDistanceText
    {
        get => FinalDistance.Text;
        set => FinalDistance.Text = value;
    }

    public float MovingTrainX
    {
        get => MovingTrain.X;
        set => MovingTrain.X = value;
    }

    public DistanceTravelledDisplay(InteractiveGue visual) : base(visual)
    {
    }
    public DistanceTravelledDisplay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        RectangleInstance = this.Visual?.GetGraphicalUiElementByName("RectangleInstance") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        DistanceVSMax = this.Visual?.GetGraphicalUiElementByName("DistanceVSMax") as global::MonoGameGum.GueDeriving.TextRuntime;
        TrainSprite = this.Visual?.GetGraphicalUiElementByName("TrainSprite") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        MovingDistanceBox = this.Visual?.GetGraphicalUiElementByName("MovingDistanceBox") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        ColoredRectangleInstance = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        ColoredRectangleInstance1 = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance1") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        ColoredRectangleInstance2 = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance2") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        ColoredRectangleInstance3 = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance3") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        ColoredRectangleInstance4 = this.Visual?.GetGraphicalUiElementByName("ColoredRectangleInstance4") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        DistanceContainer = this.Visual?.GetGraphicalUiElementByName("DistanceContainer") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        BackgroundParchement = this.Visual?.GetGraphicalUiElementByName("BackgroundParchement") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        FinalDistance = this.Visual?.GetGraphicalUiElementByName("FinalDistance") as global::MonoGameGum.GueDeriving.TextRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        MovingTrain = this.Visual?.GetGraphicalUiElementByName("MovingTrain") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
