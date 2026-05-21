//Code for PostStatsDisplay (Container)
using Gamelab.Components;
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
partial class PostStatsDisplay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("PostStatsDisplay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named PostStatsDisplay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PostStatsDisplay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PostStatsDisplay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("PostStatsDisplay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public TextRuntime IncidentTitleDetail { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public TextRuntime SeperatorTop { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance { get; protected set; }
    public StatsListItem StatsListItemInstance { get; protected set; }
    public StatsListItem StatsListItemInstance1 { get; protected set; }
    public TextRuntime FinishStageText { get; protected set; }
    public ContainerRuntime SummaryMoney { get; protected set; }
    public TextRuntime SummaryDescription { get; protected set; }
    public TextRuntime Summary { get; protected set; }
    public SpriteRuntime Paid_Stamp { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }
    public SpriteRuntime SpriteInstance1 { get; protected set; }
    public SpriteRuntime SpriteInstance2 { get; protected set; }
    public SpriteRuntime SpriteInstance3 { get; protected set; }
    public SpriteRuntime SpriteInstance4 { get; protected set; }
    public SpriteRuntime SpriteInstance5 { get; protected set; }
    public SpriteRuntime SpriteInstance6 { get; protected set; }
    public SpriteRuntime SpriteInstance7 { get; protected set; }
    public SpriteRuntime SpriteInstance8 { get; protected set; }
    public SpriteRuntime SpriteInstance9 { get; protected set; }
    public SpriteRuntime SpriteInstance10 { get; protected set; }
    public SpriteRuntime SpriteInstance11 { get; protected set; }
    public SpriteRuntime SpriteInstance12 { get; protected set; }
    public SpriteRuntime SpriteInstance13 { get; protected set; }
    public SpriteRuntime SpriteInstance14 { get; protected set; }

    public string SummaryText
    {
        get => Summary.Text;
        set => Summary.Text = value;
    }

    public PostStatsDisplay(InteractiveGue visual) : base(visual)
    {
    }
    public PostStatsDisplay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        IncidentTitleDetail = this.Visual?.GetGraphicalUiElementByName("IncidentTitleDetail") as global::MonoGameGum.GueDeriving.TextRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        SeperatorTop = this.Visual?.GetGraphicalUiElementByName("SeperatorTop") as global::MonoGameGum.GueDeriving.TextRuntime;
        ButtonWithIconInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance");
        StatsListItemInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StatsListItem>(this.Visual,"StatsListItemInstance");
        StatsListItemInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StatsListItem>(this.Visual,"StatsListItemInstance1");
        FinishStageText = this.Visual?.GetGraphicalUiElementByName("FinishStageText") as global::MonoGameGum.GueDeriving.TextRuntime;
        SummaryMoney = this.Visual?.GetGraphicalUiElementByName("SummaryMoney") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        SummaryDescription = this.Visual?.GetGraphicalUiElementByName("SummaryDescription") as global::MonoGameGum.GueDeriving.TextRuntime;
        Summary = this.Visual?.GetGraphicalUiElementByName("Summary") as global::MonoGameGum.GueDeriving.TextRuntime;
        Paid_Stamp = this.Visual?.GetGraphicalUiElementByName("Paid Stamp") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance1 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance1") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance2 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance2") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance3 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance3") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance4 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance4") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance5 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance5") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance6 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance6") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance7 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance7") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance8 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance8") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance9 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance9") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance10 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance10") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance11 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance11") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance12 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance12") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance13 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance13") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        SpriteInstance14 = this.Visual?.GetGraphicalUiElementByName("SpriteInstance14") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
